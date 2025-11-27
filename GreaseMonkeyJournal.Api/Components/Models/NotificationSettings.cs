using System;
using System.ComponentModel.DataAnnotations;

namespace GreaseMonkeyJournal.Api.Components.Models;

/// <summary>
/// Represents user preferences for email notification delivery settings.
/// </summary>
public class NotificationSettings
{
    [Key]
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Master switch to enable or disable all email notifications.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Enable notifications for reminders due in 7 days.
    /// </summary>
    public bool Send7DayReminders { get; set; } = true;

    /// <summary>
    /// Enable notifications for reminders due in 1 day.
    /// </summary>
    public bool Send1DayReminders { get; set; } = true;

    /// <summary>
    /// Enable notifications for reminders due today.
    /// </summary>
    public bool SendTodayReminders { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
