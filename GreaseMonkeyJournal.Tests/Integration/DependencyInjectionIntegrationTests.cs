using GreaseMonkeyJournal.Api.Components.Services;
using GreaseMonkeyJournal.Api.Components.DbContext;
using GreaseMonkeyJournal.Api.Components.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace GreaseMonkeyJournal.Tests.Integration;

/// <summary>
/// Integration tests to verify proper dependency injection configuration
/// </summary>
public class DependencyInjectionIntegrationTests
{
    /// <summary>
    /// Test that all services can be resolved from DI container
    /// </summary>
    [Fact]
    public void ServiceProvider_CanResolveAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Add logging and in-memory database
        services.AddLogging();
        services.AddDbContext<VehicleLogDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
            
        // Register services as they are in Program.cs
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<ILogEntryService, LogEntryService>();
        services.AddScoped<IReminderService, ReminderService>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Act & Assert - Verify all services can be resolved
        var vehicleService = serviceProvider.GetService<IVehicleService>();
        Assert.NotNull(vehicleService);
        Assert.IsType<VehicleService>(vehicleService);
        
        var logEntryService = serviceProvider.GetService<ILogEntryService>();
        Assert.NotNull(logEntryService);
        Assert.IsType<LogEntryService>(logEntryService);
        
        var reminderService = serviceProvider.GetService<IReminderService>();
        Assert.NotNull(reminderService);
        Assert.IsType<ReminderService>(reminderService);
    }
    
    /// <summary>
    /// Test that services work together properly through DI
    /// </summary>
    [Fact]
    public async Task ReminderService_WorksWithRealLogEntryService()
    {
        // Arrange
        var services = new ServiceCollection();
        
        services.AddLogging();
        services.AddDbContext<VehicleLogDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
            
        // Register real services to test integration
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<ILogEntryService, LogEntryService>();
        services.AddScoped<IReminderService, ReminderService>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Act & Assert - Test that services can interact
        using (var scope = serviceProvider.CreateScope())
        {
            var vehicleService = scope.ServiceProvider.GetRequiredService<IVehicleService>();
            var logEntryService = scope.ServiceProvider.GetRequiredService<ILogEntryService>();
            var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
            
            // Test that we can create a vehicle through the service
            var vehicle = new Vehicle 
            { 
                Make = "Test", 
                Model = "Car", 
                Year = 2020, 
                Registration = "TEST123",
                SpeedometerType = SpeedometerType.KM 
            };
            await vehicleService.AddAsync(vehicle);
            
            // Verify vehicle was created
            var vehicles = await vehicleService.GetAllAsync();
            Assert.Single(vehicles);
            Assert.Equal("Test", vehicles.First().Make);
            
            // Test that we can create a log entry
            var logEntry = new LogEntry
            {
                VehicleId = vehicles.First().Id,
                Description = "Test log entry",
                Date = DateTime.Today,
                Type = "maintenance",
                Cost = 50.00m
            };
            await logEntryService.AddAsync(logEntry);
            
            // Verify log entry was created
            var logEntries = await logEntryService.GetAllAsync();
            Assert.Single(logEntries);
            Assert.Equal("Test log entry", logEntries.First().Description);
        }
    }
    
    /// <summary>
    /// Test service lifetime scoping works correctly
    /// </summary>
    [Fact]
    public void Services_HaveCorrectLifetimeScoping()
    {
        // Arrange
        var services = new ServiceCollection();
        
        services.AddLogging();
        services.AddDbContext<VehicleLogDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
            
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<ILogEntryService, LogEntryService>();
        services.AddScoped<IReminderService, ReminderService>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Act - Get services from different scopes
        IVehicleService vehicleService1, vehicleService2, vehicleService3, vehicleService4;
        
        using (var scope1 = serviceProvider.CreateScope())
        {
            vehicleService1 = scope1.ServiceProvider.GetRequiredService<IVehicleService>();
            vehicleService2 = scope1.ServiceProvider.GetRequiredService<IVehicleService>();
        }
        
        using (var scope2 = serviceProvider.CreateScope())
        {
            vehicleService3 = scope2.ServiceProvider.GetRequiredService<IVehicleService>();
            vehicleService4 = scope2.ServiceProvider.GetRequiredService<IVehicleService>();
        }
        
        // Assert - Same instance within scope, different instances across scopes
        Assert.Same(vehicleService1, vehicleService2); // Same scope = same instance
        Assert.Same(vehicleService3, vehicleService4); // Same scope = same instance
        Assert.NotSame(vehicleService1, vehicleService3); // Different scope = different instance
    }

    /// <summary>
    /// Test that mock services can be used for testing
    /// </summary>
    [Fact]
    public async Task Services_CanBeMockedForTesting()
    {
        // Arrange
        var mockVehicleService = new Mock<IVehicleService>();
        var testVehicles = new List<Vehicle>
        {
            new Vehicle { Id = 1, Make = "Mock", Model = "Vehicle", Year = 2023, Registration = "MOCK123", SpeedometerType = SpeedometerType.KM }
        };
        
        mockVehicleService.Setup(x => x.GetAllAsync()).ReturnsAsync(testVehicles);
        
        // Act
        var vehicles = await mockVehicleService.Object.GetAllAsync();
        
        // Assert
        Assert.Single(vehicles);
        Assert.Equal("Mock", vehicles.First().Make);
        mockVehicleService.Verify(x => x.GetAllAsync(), Times.Once);
    }
}