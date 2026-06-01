using GreaseMonkeyJournal.Api.Components.Models;

namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Interface for email notification services.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a reminder notification email to a user with a list of upcoming reminders.
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="reminders">List of reminders grouped by category (today, 1 day, 7 days)</param>
    /// <returns>Task representing the async operation</returns>
    Task SendReminderEmailAsync(string toEmail, Dictionary<string, List<Reminder>> reminders);
}
