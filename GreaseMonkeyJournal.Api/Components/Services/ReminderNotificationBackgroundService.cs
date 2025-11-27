using GreaseMonkeyJournal.Api.Components.Models;
using Microsoft.EntityFrameworkCore;

namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Background service that sends daily reminder notifications at 7:00 AM.
/// </summary>
public class ReminderNotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReminderNotificationBackgroundService> _logger;
    private Timer? _timer;

    public ReminderNotificationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ReminderNotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reminder Notification Background Service started");

        // Calculate initial delay until next 7:00 AM
        var now = DateTime.Now;
        var next7AM = DateTime.Today.AddHours(7);
        
        if (now >= next7AM)
        {
            // If it's already past 7 AM today, schedule for tomorrow
            next7AM = next7AM.AddDays(1);
        }

        var initialDelay = next7AM - now;
        _logger.LogInformation("Next notification check scheduled for {Time} (in {Delay})", 
            next7AM, initialDelay);

        // Start timer with initial delay, then run every 24 hours
        _timer = new Timer(
            async _ => await SendRemindersAsync(),
            null,
            initialDelay,
            TimeSpan.FromHours(24));

        return Task.CompletedTask;
    }

    private async Task SendRemindersAsync()
    {
        _logger.LogInformation("Starting daily reminder notification check at {Time}", DateTime.Now);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
            var notificationSettingsService = scope.ServiceProvider.GetRequiredService<INotificationSettingsService>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // Get all enabled notification settings
            var enabledSettings = await notificationSettingsService.GetAllEnabledAsync();
            
            if (!enabledSettings.Any())
            {
                _logger.LogInformation("No enabled notification settings found");
                return;
            }

            // Get all active reminders with vehicle info
            var allReminders = await reminderService.GetAllRemindersWithVehicleAsync();
            var activeReminders = allReminders.Where(r => !r.IsCompleted).ToList();

            _logger.LogInformation("Found {Count} active reminders", activeReminders.Count);

            foreach (var settings in enabledSettings)
            {
                await SendRemindersForUserAsync(settings, activeReminders, emailService);
            }

            _logger.LogInformation("Daily reminder notification check completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending reminder notifications");
        }
    }

    private async Task SendRemindersForUserAsync(
        NotificationSettings settings, 
        List<Reminder> allReminders,
        IEmailService emailService)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var in7Days = today.AddDays(7);

        var remindersToSend = new Dictionary<string, List<Reminder>>();

        // Get reminders due today
        if (settings.SendTodayReminders)
        {
            var todayReminders = allReminders
                .Where(r => r.DueDate.Date == today)
                .ToList();
            
            if (todayReminders.Any())
            {
                remindersToSend["Today"] = todayReminders;
            }
        }

        // Get reminders due in 1 day
        if (settings.Send1DayReminders)
        {
            var tomorrowReminders = allReminders
                .Where(r => r.DueDate.Date == tomorrow)
                .ToList();
            
            if (tomorrowReminders.Any())
            {
                remindersToSend["1 Day"] = tomorrowReminders;
            }
        }

        // Get reminders due in 7 days
        if (settings.Send7DayReminders)
        {
            var in7DaysReminders = allReminders
                .Where(r => r.DueDate.Date == in7Days)
                .ToList();
            
            if (in7DaysReminders.Any())
            {
                remindersToSend["7 Days"] = in7DaysReminders;
            }
        }

        // Send email if there are any reminders
        if (remindersToSend.Any())
        {
            try
            {
                await emailService.SendReminderEmailAsync(settings.UserEmail, remindersToSend);
                _logger.LogInformation("Sent reminder email to {Email} with {Count} reminder(s)", 
                    settings.UserEmail, 
                    remindersToSend.Values.Sum(list => list.Count));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reminder email to {Email}", settings.UserEmail);
            }
        }
        else
        {
            _logger.LogDebug("No reminders to send to {Email}", settings.UserEmail);
        }
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}
