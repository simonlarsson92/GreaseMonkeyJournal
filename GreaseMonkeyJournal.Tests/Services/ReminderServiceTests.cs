using GreaseMonkeyJournal.Api.Components.DbContext;
using GreaseMonkeyJournal.Api.Components.Models;
using GreaseMonkeyJournal.Api.Components.Services;
using Microsoft.EntityFrameworkCore;
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
        var logEntryServiceMock = new Mock<LogEntryService>(context);
        var service = new ReminderService(context, logEntryServiceMock.Object);
        
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
        var logEntryServiceMock = new Mock<LogEntryService>(context);
        var service = new ReminderService(context, logEntryServiceMock.Object);
        
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
        var logEntryServiceMock = new Mock<LogEntryService>(context);
        var service = new ReminderService(context, logEntryServiceMock.Object);
        
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
        var logEntryServiceMock = new Mock<LogEntryService>(context);
        var service = new ReminderService(context, logEntryServiceMock.Object);
        
        var newReminder = new Reminder
        {
            VehicleId = 1,
            Description = "Brake Check",
            DueDate = DateTime.Now.AddDays(90),
            Type = "Maintenance",
            IsCompleted = false,
            DueSpeedometerReading = 30000
        };
        
        // Act
        await service.AddReminderAsync(newReminder);
        
        // Assert
        var reminder = await context.Reminders.FirstOrDefaultAsync(r => r.Description == "Brake Check");
        Assert.NotNull(reminder);
        Assert.Equal(1, reminder.VehicleId);
        Assert.Equal("Maintenance", reminder.Type);
    }
    
    [Fact]
    public async Task UpdateReminderAsync_UpdatesExistingReminder()
    {
        // Arrange
        using var context = GetDbContext();
        var logEntryServiceMock = new Mock<LogEntryService>(context);
        var service = new ReminderService(context, logEntryServiceMock.Object);
        
        var reminder = await context.Reminders.FindAsync(1);
        Assert.NotNull(reminder);
        
        // Modify the reminder
        reminder.Description = "Oil Change and Filter";
        reminder.Type = "Scheduled Maintenance";
        
        // Act
        await service.UpdateReminderAsync(reminder);
        
        // Assert
        var updatedReminder = await context.Reminders.FindAsync(1);
        Assert.NotNull(updatedReminder);
        Assert.Equal("Oil Change and Filter", updatedReminder.Description);
        Assert.Equal("Scheduled Maintenance", updatedReminder.Type);
    }
    
    [Fact]
    public async Task DeleteReminderAsync_RemovesReminderFromDatabase()
    {
        // Arrange
        using var context = GetDbContext();
        var logEntryServiceMock = new Mock<LogEntryService>(context);
        var service = new ReminderService(context, logEntryServiceMock.Object);
        
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
    public async Task DeleteReminderAsync_WithInvalidId_DoesNotThrowException()
    {
        // Arrange
        using var context = GetDbContext();
        var logEntryServiceMock = new Mock<LogEntryService>(context);
        var service = new ReminderService(context, logEntryServiceMock.Object);
        
        // Act & Assert
        await service.DeleteReminderAsync(999); // Should not throw exception
    }
    
    [Fact]
    public async Task CompleteReminderAsync_CompletesReminderAndAddsLogEntry()
    {
        // Arrange
        using var context = GetDbContext();
        var logEntryService = new LogEntryService(context);
        var service = new ReminderService(context, logEntryService);
        
        // Act
        await service.CompleteReminderAsync(
            1, 
            "Completed oil change", 
            DateTime.Now, 
            false);
        
        // Assert
        var reminder = await context.Reminders.FindAsync(1);
        Assert.NotNull(reminder);
        Assert.True(reminder.IsCompleted);
        
        // Verify log entry was created
        var logEntry = await context.LogEntries
            .FirstOrDefaultAsync(l => l.Description == "Completed oil change");
        Assert.NotNull(logEntry);
        Assert.Equal(1, logEntry.VehicleId);
    }
    
    [Fact]
    public async Task CompleteReminderAsync_WithRecreate_CreatesNewReminder()
    {
        // Arrange
        using var context = GetDbContext();
        var logEntryService = new LogEntryService(context);
        var service = new ReminderService(context, logEntryService);
        var futureDueDate = DateTime.Now.AddMonths(3);
        
        // Count reminders before test
        var reminderCountBefore = await context.Reminders.CountAsync();
        
        // Act
        await service.CompleteReminderAsync(
            1, 
            "Completed oil change", 
            DateTime.Now, 
            true, 
            futureDueDate);
        
        // Assert
        var reminder = await context.Reminders.FindAsync(1);
        Assert.NotNull(reminder);
        Assert.True(reminder.IsCompleted);
        
        // Verify new reminder was created
        var reminderCountAfter = await context.Reminders.CountAsync();
        Assert.Equal(reminderCountBefore + 1, reminderCountAfter);
        
        // Verify the new reminder has correct properties
        var newReminder = await context.Reminders
            .FirstOrDefaultAsync(r => r.Description == "Oil Change" && !r.IsCompleted);
        Assert.NotNull(newReminder);
        Assert.Equal(1, newReminder.VehicleId);
        Assert.Equal(futureDueDate.Date, newReminder.DueDate.Date);
    }
    
    [Fact]
    public async Task GetAllRemindersWithVehicleAsync_ReturnsAllRemindersWithVehicle()
    {
        // Arrange
        using var context = GetDbContext();
        var logEntryServiceMock = new Mock<LogEntryService>(context);
        var service = new ReminderService(context, logEntryServiceMock.Object);
        
        // Act
        var result = await service.GetAllRemindersWithVehicleAsync();
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.NotNull(r.Vehicle));
        Assert.Contains(result, r => r.Description == "Oil Change");
    }
}
