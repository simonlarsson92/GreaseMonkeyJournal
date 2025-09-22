using GreaseMonkeyJournal.Api.Components.DbContext;
using GreaseMonkeyJournal.Api.Components.Models;
using GreaseMonkeyJournal.Api.Components.Services;
using Microsoft.EntityFrameworkCore;

namespace GreaseMonkeyJournal.Tests.Services;

public class LogEntryServiceTests
{
    private VehicleLogDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<VehicleLogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new VehicleLogDbContext(options);
        
        // Seed the database with test data
        context.Vehicles.Add(new Vehicle 
        { 
            Id = 1, 
            Make = "Toyota", 
            Model = "Corolla", 
            Year = 2020, 
            Registration = "ABC123",
            SpeedometerType = SpeedometerType.KM
        });
        
        context.LogEntries.AddRange(
            new LogEntry 
            { 
                Id = 1, 
                VehicleId = 1, 
                Date = new DateTime(2023, 1, 15), 
                Description = "Oil Change", 
                Cost = 50.00m,
                Type = "Maintenance",
                SpeedometerReading = 15000 
            },
            new LogEntry 
            { 
                Id = 2, 
                VehicleId = 1, 
                Date = new DateTime(2023, 2, 20), 
                Description = "Tire Rotation", 
                Cost = 30.00m,
                Type = "Maintenance",
                SpeedometerReading = 18000 
            }
        );
        
        context.SaveChanges();
        
        return context;
    }
    
    [Fact]
    public async Task GetAllAsync_ReturnsAllLogEntries()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new LogEntryService(context);
        
        // Act
        var result = await service.GetAllAsync();
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, e => e.Description == "Oil Change");
        Assert.Contains(result, e => e.Description == "Tire Rotation");
    }
    
    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectLogEntry()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new LogEntryService(context);
        
        // Act
        var result = await service.GetByIdAsync(1);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Oil Change", result.Description);
        Assert.Equal(50.00m, result.Cost);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new LogEntryService(context);
        
        // Act
        var result = await service.GetByIdAsync(999);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetByVehicleIdAsync_ReturnsEntriesForSpecificVehicle()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new LogEntryService(context);
        
        // Act
        var result = await service.GetByVehicleIdAsync(1);
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, entry => Assert.Equal(1, entry.VehicleId));
    }
    
    [Fact]
    public async Task AddAsync_AddsLogEntryToDatabase()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new LogEntryService(context);
        var newLogEntry = new LogEntry
        {
            VehicleId = 1,
            Date = new DateTime(2023, 3, 10),
            Description = "Brake Pad Replacement",
            Cost = 200.00m,
            Type = "Repair"
        };
        
        // Act
        await service.AddAsync(newLogEntry);
        
        // Assert
        var entry = await context.LogEntries.FirstOrDefaultAsync(e => e.Description == "Brake Pad Replacement");
        Assert.NotNull(entry);
        Assert.Equal(200.00m, entry.Cost);
        Assert.Equal("Repair", entry.Type);
    }
    
    [Fact]
    public async Task UpdateAsync_UpdatesExistingLogEntry()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new LogEntryService(context);
        
        // Get the log entry to update
        var logEntry = await context.LogEntries.FindAsync(1);
        Assert.NotNull(logEntry);
        
        // Modify the log entry
        logEntry.Description = "Full Service";
        logEntry.Cost = 120.00m;
        
        // Detach the entity to simulate a real-world scenario
        context.Entry(logEntry).State = EntityState.Detached;
        
        // Act
        await service.UpdateAsync(logEntry);
        
        // Assert
        var updatedEntry = await context.LogEntries.FindAsync(1);
        Assert.NotNull(updatedEntry);
        Assert.Equal("Full Service", updatedEntry.Description);
        Assert.Equal(120.00m, updatedEntry.Cost);
    }
    
    [Fact]
    public async Task DeleteAsync_RemovesLogEntryFromDatabase()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new LogEntryService(context);
        
        // Verify log entry exists before delete
        var logEntry = await context.LogEntries.FindAsync(1);
        Assert.NotNull(logEntry);
        
        // Act
        await service.DeleteAsync(1);
        
        // Assert
        logEntry = await context.LogEntries.FindAsync(1);
        Assert.Null(logEntry);
    }
    
    [Fact]
    public async Task DeleteAsync_WithInvalidId_DoesNotThrowException()
    {
        // Arrange
        using var context = GetDbContext();
        var service = new LogEntryService(context);
        
        // Act & Assert
        await service.DeleteAsync(999); // Should not throw exception
    }
}
