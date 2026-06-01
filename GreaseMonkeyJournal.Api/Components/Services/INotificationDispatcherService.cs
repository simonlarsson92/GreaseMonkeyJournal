namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Dispatches maintenance reminder email notifications to all users with enabled notification settings.
/// Extracted from the background service so it can be called on-demand (e.g. dev tooling, tests).
/// </summary>
public interface INotificationDispatcherService
{
    /// <summary>
    /// Evaluates all active reminders and sends emails to every user whose notification
    /// settings are enabled. Returns the total number of emails sent.
    /// </summary>
    Task<int> SendRemindersAsync();
}
