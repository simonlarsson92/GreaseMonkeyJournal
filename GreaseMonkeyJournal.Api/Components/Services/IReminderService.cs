using GreaseMonkeyJournal.Api.Components.Models;

namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Interface for reminder service operations
/// </summary>
public interface IReminderService
{
    /// <summary>
    /// Get all reminders for a specific vehicle
    /// </summary>
    /// <param name="vehicleId">Vehicle ID</param>
    /// <returns>List of reminders for the vehicle</returns>
    Task<List<Reminder>> GetRemindersForVehicleAsync(int vehicleId);

    /// <summary>
    /// Get a reminder by its ID
    /// </summary>
    /// <param name="id">Reminder ID</param>
    /// <returns>Reminder if found, null otherwise</returns>
    Task<Reminder?> GetReminderByIdAsync(int id);

    /// <summary>
    /// Add a new reminder
    /// </summary>
    /// <param name="reminder">Reminder to add</param>
    Task AddReminderAsync(Reminder reminder);

    /// <summary>
    /// Update an existing reminder
    /// </summary>
    /// <param name="reminder">Reminder to update</param>
    Task UpdateReminderAsync(Reminder reminder);

    /// <summary>
    /// Delete a reminder by ID
    /// </summary>
    /// <param name="id">Reminder ID to delete</param>
    Task DeleteReminderAsync(int id);

    /// <summary>
    /// Complete a reminder and optionally create a log entry and new reminder
    /// </summary>
    /// <param name="reminderId">Reminder ID to complete</param>
    /// <param name="logDescription">Description for the log entry</param>
    /// <param name="logDate">Date for the log entry</param>
    /// <param name="recreate">Whether to create a new reminder</param>
    /// <param name="newDueDate">Due date for the new reminder (if recreating)</param>
    Task CompleteReminderAsync(int reminderId, string logDescription, DateTime logDate, bool recreate, DateTime? newDueDate = null);

    /// <summary>
    /// Get all reminders with vehicle information
    /// </summary>
    /// <returns>List of all reminders with vehicle information</returns>
    Task<List<Reminder>> GetAllRemindersWithVehicleAsync();
}