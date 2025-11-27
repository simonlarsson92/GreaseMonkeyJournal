using GreaseMonkeyJournal.Api.Components.Models;

namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Interface for managing notification settings.
/// </summary>
public interface INotificationSettingsService
{
    /// <summary>
    /// Gets notification settings for a specific email address.
    /// </summary>
    Task<NotificationSettings?> GetByEmailAsync(string email);

    /// <summary>
    /// Gets all notification settings with notifications enabled.
    /// </summary>
    Task<List<NotificationSettings>> GetAllEnabledAsync();

    /// <summary>
    /// Creates or updates notification settings.
    /// </summary>
    Task<NotificationSettings> SaveAsync(NotificationSettings settings);

    /// <summary>
    /// Gets the first available notification settings, or creates default if none exist.
    /// </summary>
    Task<NotificationSettings> GetOrCreateDefaultAsync();
}
