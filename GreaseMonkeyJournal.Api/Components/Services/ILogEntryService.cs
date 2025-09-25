using GreaseMonkeyJournal.Api.Components.Models;

namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Interface for log entry service operations
/// </summary>
public interface ILogEntryService
{
    /// <summary>
    /// Get all log entries
    /// </summary>
    /// <returns>List of all log entries with vehicle information</returns>
    Task<List<LogEntry>> GetAllAsync();

    /// <summary>
    /// Get a log entry by its ID
    /// </summary>
    /// <param name="id">Log entry ID</param>
    /// <returns>Log entry if found, null otherwise</returns>
    Task<LogEntry?> GetByIdAsync(int id);

    /// <summary>
    /// Get all log entries for a specific vehicle
    /// </summary>
    /// <param name="vehicleId">Vehicle ID</param>
    /// <returns>List of log entries for the vehicle</returns>
    Task<List<LogEntry>> GetByVehicleIdAsync(int vehicleId);

    /// <summary>
    /// Add a new log entry
    /// </summary>
    /// <param name="entry">Log entry to add</param>
    Task AddAsync(LogEntry entry);

    /// <summary>
    /// Update an existing log entry
    /// </summary>
    /// <param name="entry">Log entry to update</param>
    Task UpdateAsync(LogEntry entry);

    /// <summary>
    /// Delete a log entry by ID
    /// </summary>
    /// <param name="id">Log entry ID to delete</param>
    Task DeleteAsync(int id);
}