using GreaseMonkeyJournal.Api.Components.DbContext;
using GreaseMonkeyJournal.Api.Components.Models;
using GreaseMonkeyJournal.Api.Components.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GreaseMonkeyJournal.Tests.Services;

public class VehicleServiceTests
{
    private VehicleLogDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<VehicleLogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new VehicleLogDbContext(options);
        
        // Seed the database with test data
        context.Vehicles.AddRange(
            new Vehicle { Id = 1, Make = "Toyota", Model = "Corolla", Year = 2020, Registration = "ABC123", SpeedometerType = SpeedometerType.KM },
            new Vehicle { Id = 2, Make = "Honda", Model = "Civic", Year = 2019, Registration = "DEF456", SpeedometerType = SpeedometerType.Hours }
        );
        context.SaveChanges();
        
        return context;
    }
    
    [Fact]
    public async Task GetAllAsync_ReturnsAllVehicles()
    {
        // Arrange
        using var context = GetDbContext();
        IVehicleService service = new VehicleService(context);
        
        // Act
        var result = await service.GetAllAsync();
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, v => v.Make == "Toyota");
        Assert.Contains(result, v => v.Make == "Honda");
    }
    
    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectVehicle()
    {
        // Arrange
        using var context = GetDbContext();
        IVehicleService service = new VehicleService(context);
        
        // Act
        var result = await service.GetByIdAsync(1);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Toyota", result.Make);
        Assert.Equal("Corolla", result.Model);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        using var context = GetDbContext();
        IVehicleService service = new VehicleService(context);
        
        // Act
        var result = await service.GetByIdAsync(999);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task AddAsync_AddsVehicleToDatabase()
    {
        // Arrange
        using var context = GetDbContext();
        IVehicleService service = new VehicleService(context);
        var newVehicle = new Vehicle
        {
            Make = "Ford",
            Model = "Mustang",
            Year = 2021,
            Registration = "GHI789",
            SpeedometerType = SpeedometerType.KM
        };
        
        // Act
        await service.AddAsync(newVehicle);
        
        // Assert
        var vehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.Make == "Ford");
        Assert.NotNull(vehicle);
        Assert.Equal("Mustang", vehicle.Model);
        Assert.Equal(2021, vehicle.Year);
    }
    
    [Fact]
    public async Task UpdateAsync_UpdatesExistingVehicle()
    {
        // Arrange
        using var context = GetDbContext();
        IVehicleService service = new VehicleService(context);
        var vehicle = await context.Vehicles.FindAsync(1);
        Assert.NotNull(vehicle);
        
        // Modify the vehicle
        vehicle.Make = "Toyota Updated";
        vehicle.Year = 2022;
        
        // Act
        await service.UpdateAsync(vehicle);
        
        // Assert
        var updatedVehicle = await context.Vehicles.FindAsync(1);
        Assert.NotNull(updatedVehicle);
        Assert.Equal("Toyota Updated", updatedVehicle.Make);
        Assert.Equal(2022, updatedVehicle.Year);
    }
    
    [Fact]
    public async Task DeleteAsync_RemovesVehicleFromDatabase()
    {
        // Arrange
        using var context = GetDbContext();
        IVehicleService service = new VehicleService(context);
        
        // Verify vehicle exists before delete
        var vehicle = await context.Vehicles.FindAsync(1);
        Assert.NotNull(vehicle);
        
        // Act
        await service.DeleteAsync(1);
        
        // Assert
        vehicle = await context.Vehicles.FindAsync(1);
        Assert.Null(vehicle);
    }
    
    [Fact]
    public async Task DeleteAsync_WithInvalidId_DoesNotThrowException()
    {
        // Arrange
        using var context = GetDbContext();
        IVehicleService service = new VehicleService(context);
        
        // Act & Assert
        await service.DeleteAsync(999); // Should not throw exception
    }

    [Fact]
    public async Task AddAsync_WithNullVehicle_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = GetDbContext();
        IVehicleService service = new VehicleService(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.AddAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_WithNullVehicle_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = GetDbContext();
        IVehicleService service = new VehicleService(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateAsync(null!));
    }
}
