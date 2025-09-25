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

// Add MariaDB EF Core
builder.Services.AddDbContext<VehicleLogDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MariaDbConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MariaDbConnection"))
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

// Run database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VehicleLogDbContext>();
    context.Database.Migrate();
}

app.Run();
