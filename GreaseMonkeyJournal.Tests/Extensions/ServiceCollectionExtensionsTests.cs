using GreaseMonkeyJournal.Api.Components.DbContext;
using GreaseMonkeyJournal.Api.Components.Services;
using GreaseMonkeyJournal.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GreaseMonkeyJournal.Tests.Extensions;

/// <summary>
/// Tests for ServiceCollectionExtensions to verify proper DI registration
/// </summary>
public class ServiceCollectionExtensionsTests
{
    /// <summary>
    /// Test that AddApplicationServices registers all required services
    /// </summary>
    [Fact]
    public void AddApplicationServices_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<VehicleLogDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Act
        services.AddApplicationServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var vehicleService = serviceProvider.GetService<IVehicleService>();
        var logEntryService = serviceProvider.GetService<ILogEntryService>();
        var reminderService = serviceProvider.GetService<IReminderService>();

        Assert.NotNull(vehicleService);
        Assert.NotNull(logEntryService);
        Assert.NotNull(reminderService);
        
        Assert.IsType<VehicleService>(vehicleService);
        Assert.IsType<LogEntryService>(logEntryService);
        Assert.IsType<ReminderService>(reminderService);
    }

    /// <summary>
    /// Test that individual service extension methods work correctly
    /// </summary>
    [Fact]
    public void IndividualServiceExtensions_RegisterCorrectServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<VehicleLogDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Act - Register services individually
        services.AddVehicleServices()
                .AddLogEntryServices()
                .AddReminderServices();
        
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var vehicleService = serviceProvider.GetService<IVehicleService>();
        var logEntryService = serviceProvider.GetService<ILogEntryService>();
        var reminderService = serviceProvider.GetService<IReminderService>();

        Assert.NotNull(vehicleService);
        Assert.NotNull(logEntryService);
        Assert.NotNull(reminderService);
    }

    /// <summary>
    /// Test that services have correct lifetime (scoped)
    /// </summary>
    [Fact]
    public void AddApplicationServices_RegistersServicesAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<VehicleLogDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Act
        services.AddApplicationServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Test scoped lifetime
        IVehicleService service1, service2, service3, service4;

        using (var scope1 = serviceProvider.CreateScope())
        {
            service1 = scope1.ServiceProvider.GetRequiredService<IVehicleService>();
            service2 = scope1.ServiceProvider.GetRequiredService<IVehicleService>();
        }

        using (var scope2 = serviceProvider.CreateScope())
        {
            service3 = scope2.ServiceProvider.GetRequiredService<IVehicleService>();
            service4 = scope2.ServiceProvider.GetRequiredService<IVehicleService>();
        }

        // Same scope = same instance
        Assert.Same(service1, service2);
        Assert.Same(service3, service4);
        
        // Different scope = different instance
        Assert.NotSame(service1, service3);
    }

    /// <summary>
    /// Test that extension method supports method chaining
    /// </summary>
    [Fact]
    public void ExtensionMethods_SupportMethodChaining()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddDbContext<VehicleLogDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Should not throw and should support chaining
        var result = services.AddApplicationServices();

        // Assert
        Assert.Same(services, result); // Should return the same collection for chaining
    }

    /// <summary>
    /// Test that services can be resolved and dependencies are properly injected
    /// </summary>
    [Fact]
    public void AddApplicationServices_InjectsDependenciesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<VehicleLogDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        services.AddApplicationServices();
        
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - Verify ReminderService gets ILogEntryService dependency
        using var scope = serviceProvider.CreateScope();
        var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
        
        // This should not throw if dependencies are properly injected
        Assert.NotNull(reminderService);
        Assert.IsType<ReminderService>(reminderService);
    }

    /// <summary>
    /// Test the configuration-based extension method
    /// </summary>
    [Fact]
    public void AddApplicationServicesWithConfiguration_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<VehicleLogDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddApplicationServices(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var vehicleService = serviceProvider.GetService<IVehicleService>();
        var logEntryService = serviceProvider.GetService<ILogEntryService>();
        var reminderService = serviceProvider.GetService<IReminderService>();

        Assert.NotNull(vehicleService);
        Assert.NotNull(logEntryService);
        Assert.NotNull(reminderService);
    }

    /// <summary>
    /// Test conditional service registration
    /// </summary>
    [Fact]
    public void AddApplicationServicesWhen_RegistersServicesOnlyWhenConditionIsTrue()
    {
        // Arrange
        var services1 = new ServiceCollection();
        var services2 = new ServiceCollection();
        
        services1.AddDbContext<VehicleLogDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        services2.AddDbContext<VehicleLogDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Act
        services1.AddApplicationServicesWhen(() => true);  // Should register
        services2.AddApplicationServicesWhen(() => false); // Should not register
        
        var serviceProvider1 = services1.BuildServiceProvider();
        var serviceProvider2 = services2.BuildServiceProvider();

        // Assert
        var vehicleService1 = serviceProvider1.GetService<IVehicleService>();
        var vehicleService2 = serviceProvider2.GetService<IVehicleService>();

        Assert.NotNull(vehicleService1);  // Should be registered
        Assert.Null(vehicleService2);     // Should not be registered
    }

    /// <summary>
    /// Test development-specific service registration
    /// </summary>
    [Fact]
    public void AddDevelopmentApplicationServices_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<VehicleLogDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Act
        services.AddDevelopmentApplicationServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Should register all core services
        var vehicleService = serviceProvider.GetService<IVehicleService>();
        var logEntryService = serviceProvider.GetService<ILogEntryService>();
        var reminderService = serviceProvider.GetService<IReminderService>();

        Assert.NotNull(vehicleService);
        Assert.NotNull(logEntryService);
        Assert.NotNull(reminderService);
    }

    /// <summary>
    /// Test production-specific service registration
    /// </summary>
    [Fact]
    public void AddProductionApplicationServices_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<VehicleLogDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Act
        services.AddProductionApplicationServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Should register all core services
        var vehicleService = serviceProvider.GetService<IVehicleService>();
        var logEntryService = serviceProvider.GetService<ILogEntryService>();
        var reminderService = serviceProvider.GetService<IReminderService>();

        Assert.NotNull(vehicleService);
        Assert.NotNull(logEntryService);
        Assert.NotNull(reminderService);
    }
}