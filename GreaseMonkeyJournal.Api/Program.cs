using GreaseMonkeyJournal.Api.Components.DbContext;
using GreaseMonkeyJournal.Api.Components.Models;
using GreaseMonkeyJournal.Api.Extensions;
using GreaseMonkeyJournal.Components;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Exceptions;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithExceptionDetails()
    .CreateLogger();

try
{
    Log.Information("Starting GreaseMonkeyJournal application");

var builder = WebApplication.CreateBuilder(args);

// Replace default logging with Serilog
builder.Host.UseSerilog();

// Configure AppSettings
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection(AppSettings.SectionName));

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<VehicleLogDbContext>();

// Add MariaDB EF Core with fixed server version to avoid auto-detect connection issues
builder.Services.AddDbContext<VehicleLogDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MariaDbConnection"),
        ServerVersion.Create(new Version(11, 0, 0), Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MariaDb)
    ));

// Add Blazor components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add application services using extension method
builder.Services.AddApplicationServices();

var app = builder.Build();

// Add Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
    };
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Add health check endpoint
app.MapHealthChecks("/health");

// Only use HTTPS redirection in development
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Run database migrations on startup with retry logic
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VehicleLogDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    // Retry logic for database connection
    var maxRetries = 10;
    var delay = TimeSpan.FromSeconds(5);
    
    logger.LogInformation("Starting database migration with {MaxRetries} retries", maxRetries);
    
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            await context.Database.CanConnectAsync();
            logger.LogInformation("Database connection established successfully");
            
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");
            break;
        }
        catch (Exception ex) when (i < maxRetries - 1)
        {
            logger.LogWarning(ex, "Database connection attempt {Attempt} of {MaxRetries} failed. Retrying in {DelaySeconds} seconds", 
                i + 1, maxRetries, delay.TotalSeconds);
            await Task.Delay(delay);
        }
        catch (Exception ex) when (i == maxRetries - 1)
        {
            logger.LogCritical(ex, "Failed to connect to database after {MaxRetries} attempts. Application cannot start", maxRetries);
            throw;
        }
    }
}

Log.Information("Application started successfully. Environment: {Environment}", app.Environment.EnvironmentName);
app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
