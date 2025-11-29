using GreaseMonkeyJournal.Api.Components.DbContext;
using GreaseMonkeyJournal.Api.Components.Models;
using GreaseMonkeyJournal.Api.Components.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

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
        var vehicle = new Vehicle 
        { 
            Id = 1, 
            Make = "Toyota", 
            Model = "Corolla", 
            Year = 2020, 
            Registration = "ABC123",
            SpeedometerType = SpeedometerType.KM
        };
        context.Vehicles.Add(vehicle);
        
        context.LogEntries.AddRange(
            new LogEntry
            { 
                Id = 1, 
                VehicleId = 1, 
                Description = "Oil Change", 
                Date = DateTime.Now.AddDays(-30),
                Type = "maintenance",
                Cost = 50.00m,
                SpeedometerReading = 15000
            },
            new LogEntry
            { 
                Id = 2, 
                VehicleId = 1, 
                Description = "Brake Repair", 
                Date = DateTime.Now.AddDays(-60),
                Type = "repair",
                Cost = 200.00m,
                SpeedometerReading = 14000
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
        var mockLogger = new Mock<ILogger<LogEntryService>>();
        ILogEntryService service = new LogEntryService(context, mockLogger.Object);
        
        // Act
        var result = await service.GetAllAsync();
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, le => le.Description == "Oil Change");
        Assert.Contains(result, le => le.Description == "Brake Repair");
        Assert.All(result, le => Assert.NotNull(le.Vehicle));
    }
    
    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectLogEntry()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogger = new Mock<ILogger<LogEntryService>>();
        ILogEntryService service = new LogEntryService(context, mockLogger.Object);
        
        // Act
        var result = await service.GetByIdAsync(1);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Oil Change", result.Description);
        Assert.Equal(1, result.VehicleId);
        Assert.NotNull(result.Vehicle);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogger = new Mock<ILogger<LogEntryService>>();
        ILogEntryService service = new LogEntryService(context, mockLogger.Object);
        
        // Act
        var result = await service.GetByIdAsync(999);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetByVehicleIdAsync_ReturnsLogEntriesForVehicle()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogger = new Mock<ILogger<LogEntryService>>();
        ILogEntryService service = new LogEntryService(context, mockLogger.Object);
        
        // Act
        var result = await service.GetByVehicleIdAsync(1);
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, le => Assert.Equal(1, le.VehicleId));
        Assert.All(result, le => Assert.NotNull(le.Vehicle));
    }
    
    [Fact]
    public async Task AddAsync_AddsLogEntryToDatabase()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogger = new Mock<ILogger<LogEntryService>>();
        ILogEntryService service = new LogEntryService(context, mockLogger.Object);
        var newLogEntry = new LogEntry
        {
            VehicleId = 1,
            Description = "Tire Replacement",
            Date = DateTime.Now,
            Type = "maintenance",
            Cost = 400.00m,
            SpeedometerReading = 16000
        };
        
        // Act
        await service.AddAsync(newLogEntry);
        
        // Assert
        var logEntry = await context.LogEntries.FirstOrDefaultAsync(le => le.Description == "Tire Replacement");
        Assert.NotNull(logEntry);
        Assert.Equal(1, logEntry.VehicleId);
        Assert.Equal("maintenance", logEntry.Type);
        Assert.Equal(400.00m, logEntry.Cost);
    }
    
    [Fact]
    public async Task UpdateAsync_UpdatesExistingLogEntry()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogger = new Mock<ILogger<LogEntryService>>();
        ILogEntryService service = new LogEntryService(context, mockLogger.Object);
        var logEntry = await context.LogEntries.FindAsync(1);
        Assert.NotNull(logEntry);
        
        // Modify the log entry
        logEntry.Description = "Oil Change - Updated";
        logEntry.Cost = 60.00m;
        
        // Act
        await service.UpdateAsync(logEntry);
        
        // Assert
        var updatedLogEntry = await context.LogEntries.FindAsync(1);
        Assert.NotNull(updatedLogEntry);
        Assert.Equal("Oil Change - Updated", updatedLogEntry.Description);
        Assert.Equal(60.00m, updatedLogEntry.Cost);
    }
    
    [Fact]
    public async Task DeleteAsync_RemovesLogEntryFromDatabase()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogger = new Mock<ILogger<LogEntryService>>();
        ILogEntryService service = new LogEntryService(context, mockLogger.Object);
        
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
        var mockLogger = new Mock<ILogger<LogEntryService>>();
        ILogEntryService service = new LogEntryService(context, mockLogger.Object);
        
        // Act & Assert
        await service.DeleteAsync(999); // Should not throw exception
    }

    [Fact]
    public async Task AddAsync_WithNullLogEntry_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogger = new Mock<ILogger<LogEntryService>>();
        ILogEntryService service = new LogEntryService(context, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.AddAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_WithNullLogEntry_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogger = new Mock<ILogger<LogEntryService>>();
        ILogEntryService service = new LogEntryService(context, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateAsync(null!));
    }
}
