using GreaseMonkeyJournal.Api.Components.Models;
using GreaseMonkeyJournal.Api.Components.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GreaseMonkeyJournal.Tests.Services;

public class NotificationDispatcherServiceTests
{
    private static NotificationSettings MakeSettings(bool today = true, bool oneDay = true, bool sevenDay = true) =>
        new NotificationSettings
        {
            Id = 1,
            UserEmail = "test@example.com",
            IsEnabled = true,
            SendTodayReminders = today,
            Send1DayReminders = oneDay,
            Send7DayReminders = sevenDay
        };

    private static Reminder ActiveReminder(DateTime dueDate) =>
        new Reminder
        {
            Id = 1,
            VehicleId = 1,
            Description = "Oil Change",
            Type = "Maintenance",
            DueDate = dueDate,
            IsCompleted = false,
            Vehicle = new Vehicle { Id = 1, Make = "Toyota", Model = "Corolla", Year = 2020, Registration = "ABC123" }
        };

    private static NotificationDispatcherService BuildService(
        Mock<IReminderService> mockReminder,
        Mock<INotificationSettingsService> mockSettings,
        Mock<IEmailService> mockEmail)
    {
        var mockLogger = new Mock<ILogger<NotificationDispatcherService>>();
        return new NotificationDispatcherService(mockReminder.Object, mockSettings.Object, mockEmail.Object, mockLogger.Object);
    }

    [Fact]
    public async Task SendRemindersAsync_NoEnabledSettings_ReturnsZeroAndNeverSendsEmail()
    {
        var mockReminder = new Mock<IReminderService>();
        var mockSettings = new Mock<INotificationSettingsService>();
        var mockEmail = new Mock<IEmailService>();

        mockSettings.Setup(s => s.GetAllEnabledAsync()).ReturnsAsync(new List<NotificationSettings>());

        var service = BuildService(mockReminder, mockSettings, mockEmail);
        var result = await service.SendRemindersAsync();

        Assert.Equal(0, result);
        mockEmail.Verify(e => e.SendReminderEmailAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, List<Reminder>>>()), Times.Never);
    }

    [Fact]
    public async Task SendRemindersAsync_NoActiveReminders_ReturnsZeroAndNeverSendsEmail()
    {
        var mockReminder = new Mock<IReminderService>();
        var mockSettings = new Mock<INotificationSettingsService>();
        var mockEmail = new Mock<IEmailService>();

        mockSettings.Setup(s => s.GetAllEnabledAsync()).ReturnsAsync(new List<NotificationSettings> { MakeSettings() });
        mockReminder.Setup(r => r.GetAllRemindersWithVehicleAsync()).ReturnsAsync(new List<Reminder>());

        var service = BuildService(mockReminder, mockSettings, mockEmail);
        var result = await service.SendRemindersAsync();

        Assert.Equal(0, result);
        mockEmail.Verify(e => e.SendReminderEmailAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, List<Reminder>>>()), Times.Never);
    }

    [Fact]
    public async Task SendRemindersAsync_ReminderDueToday_SendsEmailAndReturnsOne()
    {
        var mockReminder = new Mock<IReminderService>();
        var mockSettings = new Mock<INotificationSettingsService>();
        var mockEmail = new Mock<IEmailService>();

        mockSettings.Setup(s => s.GetAllEnabledAsync()).ReturnsAsync(new List<NotificationSettings> { MakeSettings() });
        mockReminder.Setup(r => r.GetAllRemindersWithVehicleAsync())
            .ReturnsAsync(new List<Reminder> { ActiveReminder(DateTime.UtcNow.Date) });
        mockEmail.Setup(e => e.SendReminderEmailAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, List<Reminder>>>()))
            .Returns(Task.CompletedTask);

        var service = BuildService(mockReminder, mockSettings, mockEmail);
        var result = await service.SendRemindersAsync();

        Assert.Equal(1, result);
        mockEmail.Verify(e => e.SendReminderEmailAsync("test@example.com", It.IsAny<Dictionary<string, List<Reminder>>>()), Times.Once);
    }

    [Fact]
    public async Task SendRemindersAsync_ReminderDueTomorrow_SendsEmail()
    {
        var mockReminder = new Mock<IReminderService>();
        var mockSettings = new Mock<INotificationSettingsService>();
        var mockEmail = new Mock<IEmailService>();

        mockSettings.Setup(s => s.GetAllEnabledAsync()).ReturnsAsync(new List<NotificationSettings> { MakeSettings() });
        mockReminder.Setup(r => r.GetAllRemindersWithVehicleAsync())
            .ReturnsAsync(new List<Reminder> { ActiveReminder(DateTime.UtcNow.Date.AddDays(1)) });
        mockEmail.Setup(e => e.SendReminderEmailAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, List<Reminder>>>()))
            .Returns(Task.CompletedTask);

        var service = BuildService(mockReminder, mockSettings, mockEmail);
        var result = await service.SendRemindersAsync();

        Assert.Equal(1, result);
        mockEmail.Verify(e => e.SendReminderEmailAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, List<Reminder>>>()), Times.Once);
    }

    [Fact]
    public async Task SendRemindersAsync_ReminderDueIn7Days_SendsEmail()
    {
        var mockReminder = new Mock<IReminderService>();
        var mockSettings = new Mock<INotificationSettingsService>();
        var mockEmail = new Mock<IEmailService>();

        mockSettings.Setup(s => s.GetAllEnabledAsync()).ReturnsAsync(new List<NotificationSettings> { MakeSettings() });
        mockReminder.Setup(r => r.GetAllRemindersWithVehicleAsync())
            .ReturnsAsync(new List<Reminder> { ActiveReminder(DateTime.UtcNow.Date.AddDays(7)) });
        mockEmail.Setup(e => e.SendReminderEmailAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, List<Reminder>>>()))
            .Returns(Task.CompletedTask);

        var service = BuildService(mockReminder, mockSettings, mockEmail);
        var result = await service.SendRemindersAsync();

        Assert.Equal(1, result);
        mockEmail.Verify(e => e.SendReminderEmailAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, List<Reminder>>>()), Times.Once);
    }

    [Fact]
    public async Task SendRemindersAsync_CompletedReminder_NotSent()
    {
        var mockReminder = new Mock<IReminderService>();
        var mockSettings = new Mock<INotificationSettingsService>();
        var mockEmail = new Mock<IEmailService>();

        var completed = ActiveReminder(DateTime.UtcNow.Date);
        completed.IsCompleted = true;

        mockSettings.Setup(s => s.GetAllEnabledAsync()).ReturnsAsync(new List<NotificationSettings> { MakeSettings() });
        mockReminder.Setup(r => r.GetAllRemindersWithVehicleAsync()).ReturnsAsync(new List<Reminder> { completed });

        var service = BuildService(mockReminder, mockSettings, mockEmail);
        var result = await service.SendRemindersAsync();

        Assert.Equal(0, result);
        mockEmail.Verify(e => e.SendReminderEmailAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, List<Reminder>>>()), Times.Never);
    }

    [Fact]
    public async Task SendRemindersAsync_AllTogglesOff_SevenDayReminderNotSent()
    {
        var mockReminder = new Mock<IReminderService>();
        var mockSettings = new Mock<INotificationSettingsService>();
        var mockEmail = new Mock<IEmailService>();

        mockSettings.Setup(s => s.GetAllEnabledAsync())
            .ReturnsAsync(new List<NotificationSettings> { MakeSettings(today: false, oneDay: false, sevenDay: false) });
        mockReminder.Setup(r => r.GetAllRemindersWithVehicleAsync())
            .ReturnsAsync(new List<Reminder> { ActiveReminder(DateTime.UtcNow.Date.AddDays(7)) });

        var service = BuildService(mockReminder, mockSettings, mockEmail);
        var result = await service.SendRemindersAsync();

        Assert.Equal(0, result);
        mockEmail.Verify(e => e.SendReminderEmailAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, List<Reminder>>>()), Times.Never);
    }

    [Fact]
    public async Task SendRemindersAsync_EmailFails_ReturnsZeroWithoutThrowing()
    {
        var mockReminder = new Mock<IReminderService>();
        var mockSettings = new Mock<INotificationSettingsService>();
        var mockEmail = new Mock<IEmailService>();

        mockSettings.Setup(s => s.GetAllEnabledAsync()).ReturnsAsync(new List<NotificationSettings> { MakeSettings() });
        mockReminder.Setup(r => r.GetAllRemindersWithVehicleAsync())
            .ReturnsAsync(new List<Reminder> { ActiveReminder(DateTime.UtcNow.Date) });
        mockEmail.Setup(e => e.SendReminderEmailAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, List<Reminder>>>()))
            .ThrowsAsync(new InvalidOperationException("SMTP failed"));

        var service = BuildService(mockReminder, mockSettings, mockEmail);

        var exception = await Record.ExceptionAsync(() => service.SendRemindersAsync());

        Assert.Null(exception);
    }
}
