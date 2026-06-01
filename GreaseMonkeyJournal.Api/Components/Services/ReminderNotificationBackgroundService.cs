namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Background service that sends daily reminder notifications at 7:00 AM (server local time).
/// Note: when running inside Docker on a UTC host, 7 AM refers to 7 AM UTC.
/// Adjust the server timezone or modify <see cref="CalculateDelayUntilNext7AM"/> as needed.
/// </summary>
public class ReminderNotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReminderNotificationBackgroundService> _logger;

    public ReminderNotificationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ReminderNotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reminder Notification Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateDelayUntilNext7AM();
            var next7AM = DateTime.Now.Add(delay);
            _logger.LogInformation(
                "Next notification dispatch scheduled for {Time} (in {Delay:hh\\:mm\\:ss})",
                next7AM, delay);

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

    private static TimeSpan CalculateDelayUntilNext7AM()
    {
        var now = DateTime.Now;
        var next7AM = DateTime.Today.AddHours(7);
        if (now >= next7AM)
            next7AM = next7AM.AddDays(1);
        return next7AM - now;
    }
}
