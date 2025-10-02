using GreaseMonkeyJournal.Api.Components.DbContext;
using GreaseMonkeyJournal.Api.Components.Models;
using GreaseMonkeyJournal.Api.Extensions;
using GreaseMonkeyJournal.Components;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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
    
    // Retry logic for database connection
    var maxRetries = 10;
    var delay = TimeSpan.FromSeconds(5);
    
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            await context.Database.CanConnectAsync();
            await context.Database.MigrateAsync();
            break;
        }
        catch (Exception ex) when (i < maxRetries - 1)
        {
            Console.WriteLine($"Database connection attempt {i + 1} failed: {ex.Message}");
            Console.WriteLine($"Retrying in {delay.TotalSeconds} seconds...");
            await Task.Delay(delay);
        }
    }
}

app.Run();
