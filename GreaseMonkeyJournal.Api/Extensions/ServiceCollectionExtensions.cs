using GreaseMonkeyJournal.Api.Components.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace GreaseMonkeyJournal.Api.Extensions;

/// <summary>
/// Extension methods for configuring application services in the DI container
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all application services to the DI container with their respective interfaces
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register vehicle-related services
        services.AddScoped<IVehicleService, VehicleService>();
        
        // Register log entry services
        services.AddScoped<ILogEntryService, LogEntryService>();
        
        // Register reminder services (depends on ILogEntryService)
        services.AddScoped<IReminderService, ReminderService>();
        
        return services;
    }

    /// <summary>
    /// Adds all application services with configuration options
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <param name="configuration">Configuration instance for service configuration</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Future: Could add configuration-based service registration here
        // For example: different implementations based on environment
        
        return services.AddApplicationServices();
    }

    /// <summary>
    /// Adds vehicle management services to the DI container
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddVehicleServices(this IServiceCollection services)
    {
        services.AddScoped<IVehicleService, VehicleService>();
        return services;
    }

    /// <summary>
    /// Adds log entry management services to the DI container
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddLogEntryServices(this IServiceCollection services)
    {
        services.AddScoped<ILogEntryService, LogEntryService>();
        return services;
    }

    /// <summary>
    /// Adds reminder management services to the DI container
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddReminderServices(this IServiceCollection services)
    {
        services.AddScoped<IReminderService, ReminderService>();
        return services;
    }

    /// <summary>
    /// Adds application services conditionally based on a predicate
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <param name="condition">Condition to evaluate before adding services</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddApplicationServicesWhen(this IServiceCollection services, Func<bool> condition)
    {
        if (condition())
        {
            services.AddApplicationServices();
        }
        
        return services;
    }

    /// <summary>
    /// Adds application services for development environment with additional debugging services
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddDevelopmentApplicationServices(this IServiceCollection services)
    {
        // Add core services
        services.AddApplicationServices();
        
        // Future: Add development-specific services like:
        // - Enhanced logging
        // - Mock services for external dependencies
        // - Development-only diagnostic services
        
        return services;
    }

    /// <summary>
    /// Adds application services for production environment with optimizations
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddProductionApplicationServices(this IServiceCollection services)
    {
        // Add core services
        services.AddApplicationServices();
        
        // Future: Add production-specific services like:
        // - Caching decorators
        // - Performance monitoring
        // - Circuit breakers for external services
        
        return services;
    }
}