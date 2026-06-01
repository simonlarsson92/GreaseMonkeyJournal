using GreaseMonkeyJournal.Api.Components.Models;

namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Scoped service that resolves which reminders to send and dispatches emails.
/// Keeping this logic separate from the background service makes it testable and
/// callable on-demand (e.g. from the dev trigger in Settings).
/// </summary>
public class NotificationDispatcherService : INotificationDispatcherService
{
    private readonly IReminderService _reminderService;
    private readonly INotificationSettingsService _notificationSettingsService;
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationDispatcherService> _logger;

    public NotificationDispatcherService(
        IReminderService reminderService,
        INotificationSettingsService notificationSettingsService,
        IEmailService emailService,
        ILogger<NotificationDispatcherService> logger)
    {
        _reminderService = reminderService;
        _notificationSettingsService = notificationSettingsService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<int> SendRemindersAsync()
    {
        _logger.LogInformation("Starting reminder notification dispatch at {Time}", DateTime.UtcNow);

        var enabledSettings = await _notificationSettingsService.GetAllEnabledAsync();

        if (enabledSettings.Count == 0)
        {
            _logger.LogInformation("No enabled notification settings found — skipping dispatch");
            return 0;
        }

        var allReminders = await _reminderService.GetAllRemindersWithVehicleAsync();
        var activeReminders = allReminders.Where(r => !r.IsCompleted).ToList();

        _logger.LogInformation("Found {Count} active reminders across all vehicles", activeReminders.Count);

        var emailsSent = 0;

        foreach (var settings in enabledSettings)
        {
            if (await SendRemindersForUserAsync(settings, activeReminders))
                emailsSent++;
        }

        _logger.LogInformation("Dispatch complete — sent {Count} email(s)", emailsSent);
        return emailsSent;
    }

    private async Task<bool> SendRemindersForUserAsync(
        NotificationSettings settings,
        List<Reminder> allReminders)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var in7Days = today.AddDays(7);

        var remindersToSend = new Dictionary<string, List<Reminder>>();

        if (settings.SendTodayReminders)
        {
            var list = allReminders.Where(r => r.DueDate.Date == today).ToList();
            if (list.Count > 0) remindersToSend["Today"] = list;
        }

        if (settings.Send1DayReminders)
        {
            var list = allReminders.Where(r => r.DueDate.Date == tomorrow).ToList();
            if (list.Count > 0) remindersToSend["1 Day"] = list;
        }

        if (settings.Send7DayReminders)
        {
            var list = allReminders.Where(r => r.DueDate.Date == in7Days).ToList();
            if (list.Count > 0) remindersToSend["7 Days"] = list;
        }

        // Also include overdue reminders (past due, not yet completed)
        var overdue = allReminders.Where(r => r.DueDate.Date < today).ToList();
        if (overdue.Count > 0) remindersToSend["Overdue"] = overdue;

        if (remindersToSend.Count == 0)
        {
            _logger.LogDebug("No reminders to send for {Email}", settings.UserEmail);
            return false;
        }

        try
        {
            await _emailService.SendReminderEmailAsync(settings.UserEmail, remindersToSend);
            _logger.LogInformation(
                "Sent reminder email to {Email} with {Count} reminder(s)",
                settings.UserEmail,
                remindersToSend.Values.Sum(l => l.Count));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send reminder email to {Email}", settings.UserEmail);
            return false;
        }
    }
}
