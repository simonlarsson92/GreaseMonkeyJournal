using GreaseMonkeyJournal.Api.Components.Models;

namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Provides contract for maintenance log entry management operations, enabling comprehensive
/// tracking of vehicle service history and maintenance activities in the Grease Monkey Journal application.
/// </summary>
/// <remarks>
/// This service interface defines operations for managing log entries that record maintenance
/// activities, service visits, repairs, and other vehicle-related events. Log entries are
/// associated with specific vehicles and include detailed information about work performed,
/// dates, mileage, and costs.
/// 
/// All operations are designed to work with the Entity Framework change tracking system
/// and support asynchronous execution for optimal performance in web applications.
/// </remarks>
/// <example>
/// Basic service usage in a Blazor component:
/// <code>
/// [Inject] public ILogEntryService LogEntryService { get; set; } = default!
/// 
/// // Getting maintenance history for a vehicle
/// var vehicleHistory = await LogEntryService.GetByVehicleIdAsync(vehicleId);
/// 
/// // Adding a new maintenance record
/// var logEntry = new LogEntry
/// {
///     VehicleId = vehicleId,
///     Description = "Oil change and filter replacement",
///     Date = DateTime.Now,
///     Mileage = 45000,
///     Cost = 89.99m
/// };
/// await LogEntryService.AddAsync(logEntry);
/// </code>
/// </example>
public interface ILogEntryService
{
    /// <summary>
    /// Retrieves all log entries from the database with associated vehicle information for comprehensive reporting.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list of all
    /// <see cref="LogEntry"/> entities with their associated <see cref="Vehicle"/> navigation properties
    /// populated. Returns an empty list if no log entries are found in the database.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context is not properly initialized, when there are database
    /// connectivity issues, or when the Vehicle navigation property cannot be loaded.
    /// </exception>
    /// <remarks>
    /// This method includes the Vehicle navigation property to provide complete information
    /// for reporting and display purposes. For large datasets, consider implementing
    /// pagination or filtering to improve performance.
    /// </remarks>
    /// <example>
    /// <code>
    /// var allEntries = await logEntryService.GetAllAsync();
    /// foreach (var entry in allEntries)
    /// {
    ///     Console.WriteLine($"{entry.Date:d} - {entry.Vehicle?.Make} {entry.Vehicle?.Model}: {entry.Description}");
    /// }
    /// </code>
    /// </example>
    Task<List<LogEntry>> GetAllAsync();

    /// <summary>
    /// Retrieves a specific log entry by its unique identifier with associated vehicle information.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the log entry to retrieve. Must be a positive integer
    /// representing an existing log entry in the database.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the
    /// <see cref="LogEntry"/> entity with its associated <see cref="Vehicle"/> navigation property
    /// if found, or null if no log entry exists with the specified ID.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided ID is less than or equal to zero.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context is not properly initialized, when there are database
    /// connectivity issues, or when the Vehicle navigation property cannot be loaded.
    /// </exception>
    /// <example>
    /// <code>
    /// var logEntry = await logEntryService.GetByIdAsync(456);
    /// if (logEntry != null)
    /// {
    ///     Console.WriteLine($"Entry: {logEntry.Description} for {logEntry.Vehicle?.Make} {logEntry.Vehicle?.Model}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Log entry not found");
    /// }
    /// </code>
    /// </example>
    Task<LogEntry?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves all log entries associated with a specific vehicle, ordered by date for chronological history.
    /// </summary>
    /// <param name="vehicleId">
    /// The unique identifier of the vehicle whose log entries should be retrieved.
    /// Must be a positive integer representing an existing vehicle in the database.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list of
    /// <see cref="LogEntry"/> entities associated with the specified vehicle, including
    /// the Vehicle navigation property. Returns an empty list if no entries are found
    /// for the specified vehicle.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided vehicle ID is less than or equal to zero.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context is not properly initialized, when there are database
    /// connectivity issues, or when the Vehicle navigation property cannot be loaded.
    /// </exception>
    /// <remarks>
    /// This method is optimized for displaying maintenance history for a specific vehicle.
    /// The results include the vehicle information for consistency with other service methods.
    /// </remarks>
    /// <example>
    /// <code>
    /// var vehicleHistory = await logEntryService.GetByVehicleIdAsync(123);
    /// Console.WriteLine($"Found {vehicleHistory.Count} maintenance records");
    /// 
    /// foreach (var entry in vehicleHistory)
    /// {
    ///     Console.WriteLine($"{entry.Date:yyyy-MM-dd}: {entry.Description}");
    ///     if (entry.Cost.HasValue)
    ///         Console.WriteLine($"  Cost: ${entry.Cost:F2}");
    /// }
    /// </code>
    /// </example>
    Task<List<LogEntry>> GetByVehicleIdAsync(int vehicleId);

    /// <summary>
    /// Adds a new maintenance log entry to the database with validation and timestamp management.
    /// </summary>
    /// <param name="entry">
    /// The <see cref="LogEntry"/> entity to add. Must contain all required fields including
    /// VehicleId, Description, and Date. The Vehicle navigation property should be null to
    /// avoid relationship conflicts during insertion.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The log entry will be persisted
    /// to the database and the entity will have its Id property populated upon successful completion.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the entry parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when required entry properties (VehicleId, Description, Date) are missing or invalid,
    /// or when the VehicleId references a non-existent vehicle.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context is not properly initialized or when there are
    /// database connectivity issues.
    /// </exception>
    /// <exception cref="DbUpdateException">
    /// Thrown when there are database constraint violations, such as foreign key violations
    /// for invalid VehicleId values.
    /// </exception>
    /// <example>
    /// <code>
    /// var maintenanceEntry = new LogEntry
    /// {
    ///     VehicleId = 123,
    ///     Description = "Brake pad replacement - front axle",
    ///     Date = DateTime.Today,
    ///     Mileage = 78500,
    ///     Cost = 245.50m,
    ///     Notes = "Brake pads were worn to 2mm. Rotors inspected and are in good condition."
    /// };
    /// 
    /// await logEntryService.AddAsync(maintenanceEntry);
    /// // maintenanceEntry.Id now contains the database-generated ID
    /// Console.WriteLine($"Added maintenance record with ID: {maintenanceEntry.Id}");
    /// </code>
    /// </example>
    Task AddAsync(LogEntry entry);

    /// <summary>
    /// Updates an existing maintenance log entry in the database with the provided information.
    /// </summary>
    /// <param name="entry">
    /// The <see cref="LogEntry"/> entity with updated information. Must have a valid Id
    /// that exists in the database. The Vehicle navigation property should be null to
    /// avoid relationship conflicts during the update operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The log entry record in the database
    /// will be updated with the provided information upon successful completion.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the entry parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the entry Id is invalid (less than or equal to zero) or when
    /// required properties are missing or invalid.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no log entry exists with the specified Id, or when the database context
    /// is not properly initialized, or when there are database connectivity issues.
    /// </exception>
    /// <exception cref="DbUpdateException">
    /// Thrown when there are database constraint violations during the update operation.
    /// </exception>
    /// <exception cref="DbUpdateConcurrencyException">
    /// Thrown when the entry has been modified by another process since it was loaded.
    /// </exception>
    /// <remarks>
    /// This method uses Entity Framework's change tracking to detect and persist only
    /// the modified properties. The Vehicle navigation property is cleared to prevent
    /// relationship conflicts during the update operation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var existingEntry = await logEntryService.GetByIdAsync(789);
    /// if (existingEntry != null)
    /// {
    ///     existingEntry.Cost = 195.75m; // Update cost after getting final invoice
    ///     existingEntry.Notes += " - Final invoice received";
    ///     
    ///     // Clear navigation property before update
    ///     existingEntry.Vehicle = null;
    ///     
    ///     await logEntryService.UpdateAsync(existingEntry);
    ///     Console.WriteLine("Maintenance record updated successfully");
    /// }
    /// </code>
    /// </example>
    Task UpdateAsync(LogEntry entry);

    /// <summary>
    /// Removes a maintenance log entry from the database permanently, including all associated data.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the log entry to delete. Must be a positive integer
    /// representing an existing log entry in the database.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The log entry will be permanently
    /// removed from the database upon successful completion. No error is thrown if the
    /// log entry doesn't exist.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided ID is less than or equal to zero.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context is not properly initialized or when there are
    /// database connectivity issues.
    /// </exception>
    /// <exception cref="DbUpdateException">
    /// Thrown when there are foreign key constraint violations that prevent deletion,
    /// such as when the log entry is referenced by other entities.
    /// </exception>
    /// <remarks>
    /// This operation is destructive and cannot be undone. Consider implementing
    /// soft deletes or archival functionality for production use cases where
    /// maintenance history recovery might be necessary for warranty or legal purposes.
    /// 
    /// The deletion will succeed silently if the log entry doesn't exist, making
    /// this method idempotent and safe to call multiple times.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Delete a maintenance record
    /// await logEntryService.DeleteAsync(789);
    /// 
    /// // Verify deletion
    /// var deletedEntry = await logEntryService.GetByIdAsync(789);
    /// if (deletedEntry == null)
    /// {
    ///     Console.WriteLine("Maintenance record successfully deleted");
    /// }
    /// </code>
    /// </example>
    Task DeleteAsync(int id);
}