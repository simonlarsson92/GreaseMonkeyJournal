namespace GreaseMonkeyJournal.Api.Components.Models;

public class AppSettings
{
    public const string SectionName = "AppSettings";

    public string ApplicationTitle { get; set; } = "Application";

    /// <summary>
    /// The hour of the day (0–23) at which daily reminder notifications are dispatched.
    /// When running in Docker (UTC), set this to the desired UTC hour.
    /// </summary>
    public int NotificationDispatchHour { get; set; } = 7;
}
