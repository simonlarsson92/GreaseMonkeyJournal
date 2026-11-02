using GreaseMonkeyJournal.Api.Components.Models;
using Microsoft.Extensions.Options;

namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Background service that sends daily reminder notifications at a configurable hour
/// (default 7:00 AM). Set <c>AppSettings:NotificationDispatchHour</c> in appsettings.json
/// to change the dispatch time. When running in Docker (UTC), set the value to the desired UTC hour.
/// </summary>
public class ReminderNotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReminderNotificationBackgroundService> _logger;
    private readonly int _dispatchHour;

    public ReminderNotificationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ReminderNotificationBackgroundService> logger,
        IOptions<AppSettings> appSettings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _dispatchHour = appSettings.Value.NotificationDispatchHour;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Reminder Notification Background Service started (dispatch hour: {Hour:D2}:00)",
            _dispatchHour);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateDelayUntilNextDispatch();
            var nextDispatch = DateTime.Now.Add(delay);
            _logger.LogInformation(
                "Next notification dispatch scheduled for {Time} (in {Delay:hh\\:mm\\:ss})",
                nextDispatch, delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (stoppingToken.IsCancellationRequested)
                break;

            using var scope = _serviceProvider.CreateScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcherService>();

            try
            {
                await dispatcher.SendRemindersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error during scheduled reminder dispatch");
            }
        }

        _logger.LogInformation("Reminder Notification Background Service stopped");
    }

    private TimeSpan CalculateDelayUntilNextDispatch()
    {
        var now = DateTime.Now;
        var nextDispatch = DateTime.Today.AddHours(_dispatchHour);
        if (now >= nextDispatch)
            nextDispatch = nextDispatch.AddDays(1);
        return nextDispatch - now;
    }
}
