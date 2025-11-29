using GreaseMonkeyJournal.Api.Components.DbContext;
using GreaseMonkeyJournal.Api.Components.Models;
using GreaseMonkeyJournal.Api.Components.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace GreaseMonkeyJournal.Tests.Services;

public class ReminderServiceTests
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
        
        context.Reminders.AddRange(
            new Reminder
            { 
                Id = 1, 
                VehicleId = 1, 
                Description = "Oil Change", 
                DueDate = DateTime.Now.AddDays(30),
                Type = "Maintenance",
                IsCompleted = false,
                DueSpeedometerReading = 20000
            },
            new Reminder
            { 
                Id = 2, 
                VehicleId = 1, 
                Description = "Tire Rotation", 
                DueDate = DateTime.Now.AddDays(60),
                Type = "Maintenance",
                IsCompleted = false,
                DueSpeedometerReading = 25000
            }
        );
        
        context.SaveChanges();
        
        return context;
    }
    
    [Fact]
    public async Task GetRemindersForVehicleAsync_ReturnsCorrectReminders()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogEntryService = new Mock<ILogEntryService>();
        var mockLogger = new Mock<ILogger<ReminderService>>();
        IReminderService service = new ReminderService(context, mockLogEntryService.Object, mockLogger.Object);
        
        // Act
        var result = await service.GetRemindersForVehicleAsync(1);
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Description == "Oil Change");
        Assert.Contains(result, r => r.Description == "Tire Rotation");
    }
    
    [Fact]
    public async Task GetReminderByIdAsync_ReturnsCorrectReminder()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogEntryService = new Mock<ILogEntryService>();
        var mockLogger = new Mock<ILogger<ReminderService>>();
        IReminderService service = new ReminderService(context, mockLogEntryService.Object, mockLogger.Object);
        
        // Act
        var result = await service.GetReminderByIdAsync(1);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Oil Change", result.Description);
        Assert.Equal(1, result.VehicleId);
    }
    
    [Fact]
    public async Task GetReminderByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogEntryService = new Mock<ILogEntryService>();
        var mockLogger = new Mock<ILogger<ReminderService>>();
        IReminderService service = new ReminderService(context, mockLogEntryService.Object, mockLogger.Object);
        
        // Act
        var result = await service.GetReminderByIdAsync(999);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task AddReminderAsync_AddsReminderToDatabase()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogEntryService = new Mock<ILogEntryService>();
        var mockLogger = new Mock<ILogger<ReminderService>>();
        IReminderService service = new ReminderService(context, mockLogEntryService.Object, mockLogger.Object);
        var newReminder = new Reminder
        {
            VehicleId = 1,
            Description = "Brake Inspection",
            Type = "Maintenance",
            DueDate = DateTime.Now.AddDays(90),
            IsCompleted = false
        };
        
        // Act
        await service.AddReminderAsync(newReminder);
        
        // Assert
        var reminder = await context.Reminders.FirstOrDefaultAsync(r => r.Description == "Brake Inspection");
        Assert.NotNull(reminder);
        Assert.Equal(1, reminder.VehicleId);
        Assert.Equal("Maintenance", reminder.Type);
    }
    
    [Fact]
    public async Task UpdateReminderAsync_UpdatesExistingReminder()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogEntryService = new Mock<ILogEntryService>();
        var mockLogger = new Mock<ILogger<ReminderService>>();
        IReminderService service = new ReminderService(context, mockLogEntryService.Object, mockLogger.Object);
        var reminder = await context.Reminders.FindAsync(1);
        Assert.NotNull(reminder);
        
        // Modify the reminder
        reminder.Description = "Oil Change - Updated";
        reminder.Type = "Repair";
        
        // Act
        await service.UpdateReminderAsync(reminder);
        
        // Assert
        var updatedReminder = await context.Reminders.FindAsync(1);
        Assert.NotNull(updatedReminder);
        Assert.Equal("Oil Change - Updated", updatedReminder.Description);
        Assert.Equal("Repair", updatedReminder.Type);
    }
    
    [Fact]
    public async Task DeleteReminderAsync_RemovesReminderFromDatabase()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogEntryService = new Mock<ILogEntryService>();
        var mockLogger = new Mock<ILogger<ReminderService>>();
        IReminderService service = new ReminderService(context, mockLogEntryService.Object, mockLogger.Object);
        
        // Verify reminder exists before delete
        var reminder = await context.Reminders.FindAsync(1);
        Assert.NotNull(reminder);
        
        // Act
        await service.DeleteReminderAsync(1);
        
        // Assert
        reminder = await context.Reminders.FindAsync(1);
        Assert.Null(reminder);
    }
    
    [Fact]
    public async Task CompleteReminderAsync_MarksReminderCompletedAndCreatesLogEntry()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogEntryService = new Mock<ILogEntryService>();
        mockLogEntryService
            .Setup(x => x.AddAsync(It.IsAny<LogEntry>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        var mockLogger = new Mock<ILogger<ReminderService>>();
        
        IReminderService service = new ReminderService(context, mockLogEntryService.Object, mockLogger.Object);
        
        // Act
        await service.CompleteReminderAsync(1, "Completed oil change", DateTime.Today, false);
        
        // Assert
        var reminder = await context.Reminders.FindAsync(1);
        Assert.NotNull(reminder);
        Assert.True(reminder.IsCompleted);
        
        // Verify log entry service was called
        mockLogEntryService.Verify(x => x.AddAsync(It.Is<LogEntry>(le => 
            le.VehicleId == 1 && 
            le.Description == "Completed oil change" && 
            le.Date == DateTime.Today)), Times.Once);
    }
    
    [Fact]
    public async Task CompleteReminderAsync_WithRecreate_CreatesNewReminder()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogEntryService = new Mock<ILogEntryService>();
        mockLogEntryService
            .Setup(x => x.AddAsync(It.IsAny<LogEntry>()))
            .Returns(Task.CompletedTask);
        var mockLogger = new Mock<ILogger<ReminderService>>();
        
        IReminderService service = new ReminderService(context, mockLogEntryService.Object, mockLogger.Object);
        var newDueDate = DateTime.Today.AddDays(30);
        
        // Act
        await service.CompleteReminderAsync(1, "Completed oil change", DateTime.Today, true, newDueDate);
        
        // Assert
        var originalReminder = await context.Reminders.FindAsync(1);
        Assert.NotNull(originalReminder);
        Assert.True(originalReminder.IsCompleted);
        
        var newReminder = await context.Reminders
            .FirstOrDefaultAsync(r => r.Id != 1 && r.VehicleId == 1 && r.Description == "Oil Change" && !r.IsCompleted);
        Assert.NotNull(newReminder);
        Assert.Equal(newDueDate, newReminder.DueDate);
    }

    [Fact]
    public async Task GetAllRemindersWithVehicleAsync_ReturnsRemindersWithVehicleInfo()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogEntryService = new Mock<ILogEntryService>();
        var mockLogger = new Mock<ILogger<ReminderService>>();
        IReminderService service = new ReminderService(context, mockLogEntryService.Object, mockLogger.Object);
        
        // Act
        var result = await service.GetAllRemindersWithVehicleAsync();
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.NotNull(r.Vehicle));
        Assert.Contains(result, r => r.Vehicle != null && r.Vehicle.Make == "Toyota");
    }

    [Fact]
    public async Task AddReminderAsync_WithNullReminder_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogEntryService = new Mock<ILogEntryService>();
        var mockLogger = new Mock<ILogger<ReminderService>>();
        IReminderService service = new ReminderService(context, mockLogEntryService.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.AddReminderAsync(null!));
    }

    [Fact]
    public async Task CompleteReminderAsync_WithEmptyDescription_ThrowsArgumentException()
    {
        // Arrange
        using var context = GetDbContext();
        var mockLogEntryService = new Mock<ILogEntryService>();
        var mockLogger = new Mock<ILogger<ReminderService>>();
        IReminderService service = new ReminderService(context, mockLogEntryService.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.CompleteReminderAsync(1, "", DateTime.Today, false));
    }
}
