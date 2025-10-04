using GreaseMonkeyJournal.Api.Components.Models;

namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Provides contract for vehicle maintenance reminder management operations, enabling proactive
/// maintenance scheduling and completion tracking in the Grease Monkey Journal application.
/// </summary>
/// <remarks>
/// This service interface defines operations for managing maintenance reminders that help users
/// stay on top of scheduled maintenance tasks such as oil changes, tire rotations, inspections,
/// and other recurring vehicle maintenance activities. The service supports reminder creation,
/// completion tracking, and automatic log entry generation when maintenance is performed.
/// 
/// Reminders can be completed with options to automatically create maintenance log entries
/// and schedule follow-up reminders, providing a comprehensive maintenance workflow management system.
/// </remarks>
/// <example>
/// Basic service usage for maintenance reminder management:
/// <code>
/// [Inject] public IReminderService ReminderService { get; set; } = default!;
/// 
/// // Getting upcoming reminders for a vehicle
/// var upcomingReminders = await ReminderService.GetRemindersForVehicleAsync(vehicleId);
/// 
/// // Creating a new reminder
/// var oilChangeReminder = new Reminder
/// {
///     VehicleId = vehicleId,
///     Description = "Oil Change Due",
///     DueDate = DateTime.Today.AddDays(30),
///     Type = "Maintenance"
/// };
/// await ReminderService.AddReminderAsync(oilChangeReminder);
/// 
/// // Completing a reminder with automatic log entry and follow-up scheduling
/// await ReminderService.CompleteReminderAsync(
///     reminderId: reminder.Id,
///     logDescription: "Oil change completed - 5W-30 synthetic oil",
///     logDate: DateTime.Today,
///     recreate: true,
///     newDueDate: DateTime.Today.AddMonths(6)
/// );
/// </code>
/// </example>
public interface IReminderService
{
    /// <summary>
    /// Retrieves all active reminders associated with a specific vehicle for maintenance planning.
    /// </summary>
    /// <param name="vehicleId">
    /// The unique identifier of the vehicle whose reminders should be retrieved.
    /// Must be a positive integer representing an existing vehicle in the database.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list of
    /// <see cref="Reminder"/> entities associated with the specified vehicle, including both
    /// active and completed reminders. Returns an empty list if no reminders are found
    /// for the specified vehicle.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided vehicle ID is less than or equal to zero.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context is not properly initialized or when there are
    /// database connectivity issues.
    /// </exception>
    /// <example>
    /// <code>
    /// var vehicleReminders = await reminderService.GetRemindersForVehicleAsync(123);
    /// var activeReminders = vehicleReminders.Where(r => !r.IsCompleted).ToList();
    /// 
    /// Console.WriteLine($"Vehicle has {activeReminders.Count} active reminders:");
    /// foreach (var reminder in activeReminders)
    /// {
    ///     Console.WriteLine($"- {reminder.Description} due on {reminder.DueDate:yyyy-MM-dd}");
    /// }
    /// </code>
    /// </example>
    Task<List<Reminder>> GetRemindersForVehicleAsync(int vehicleId);

    /// <summary>
    /// Retrieves a specific reminder by its unique identifier for detailed viewing or editing.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the reminder to retrieve. Must be a positive integer
    /// representing an existing reminder in the database.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the
    /// <see cref="Reminder"/> entity if found, or null if no reminder exists with the specified ID.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided ID is less than or equal to zero.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context is not properly initialized or when there are
    /// database connectivity issues.
    /// </exception>
    /// <example>
    /// <code>
    /// var reminder = await reminderService.GetReminderByIdAsync(456);
    /// if (reminder != null)
    /// {
    ///     Console.WriteLine($"Reminder: {reminder.Description}");
    ///     Console.WriteLine($"Due: {reminder.DueDate:yyyy-MM-dd}");
    ///     Console.WriteLine($"Status: {(reminder.IsCompleted ? "Completed" : "Active")}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Reminder not found");
    /// }
    /// </code>
    /// </example>
    Task<Reminder?> GetReminderByIdAsync(int id);

    /// <summary>
    /// Adds a new maintenance reminder to the database with validation and default status settings.
    /// </summary>
    /// <param name="reminder">
    /// The <see cref="Reminder"/> entity to add. Must contain all required fields including
    /// VehicleId, Description, and DueDate. The IsCompleted property will be set to false
    /// by default if not specified.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The reminder will be persisted
    /// to the database and the entity will have its Id property populated upon successful completion.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the reminder parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when required reminder properties (VehicleId, Description, DueDate) are missing
    /// or invalid, or when the VehicleId references a non-existent vehicle.
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
    /// var inspectionReminder = new Reminder
    /// {
    ///     VehicleId = 123,
    ///     Description = "Annual State Inspection Due",
    ///     DueDate = new DateTime(2024, 12, 31),
    ///     Type = "Inspection",
    ///     IsCompleted = false
    /// };
    /// 
    /// await reminderService.AddReminderAsync(inspectionReminder);
    /// Console.WriteLine($"Created reminder with ID: {inspectionReminder.Id}");
    /// </code>
    /// </example>
    Task AddReminderAsync(Reminder reminder);

    /// <summary>
    /// Updates an existing maintenance reminder in the database with the provided information.
    /// </summary>
    /// <param name="reminder">
    /// The <see cref="Reminder"/> entity with updated information. Must have a valid Id
    /// that exists in the database. All properties will be updated to match the provided entity.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The reminder record in the database
    /// will be updated with the provided information upon successful completion.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the reminder parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the reminder Id is invalid (less than or equal to zero) or when
    /// required properties are missing or invalid.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no reminder exists with the specified Id, or when the database context
    /// is not properly initialized, or when there are database connectivity issues.
    /// </exception>
    /// <exception cref="DbUpdateException">
    /// Thrown when there are database constraint violations during the update operation.
    /// </exception>
    /// <exception cref="DbUpdateConcurrencyException">
    /// Thrown when the reminder has been modified by another process since it was loaded.
    /// </exception>
    /// <example>
    /// <code>
    /// var existingReminder = await reminderService.GetReminderByIdAsync(789);
    /// if (existingReminder != null)
    /// {
    ///     existingReminder.DueDate = existingReminder.DueDate.AddDays(7); // Extend due date
    ///     existingReminder.Description += " - Extended due to parts availability";
    ///     
    ///     await reminderService.UpdateReminderAsync(existingReminder);
    ///     Console.WriteLine("Reminder updated successfully");
    /// }
    /// </code>
    /// </example>
    Task UpdateReminderAsync(Reminder reminder);

    /// <summary>
    /// Removes a maintenance reminder from the database permanently.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the reminder to delete. Must be a positive integer
    /// representing an existing reminder in the database.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The reminder will be permanently
    /// removed from the database upon successful completion. No error is thrown if the
    /// reminder doesn't exist.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided ID is less than or equal to zero.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context is not properly initialized or when there are
    /// database connectivity issues.
    /// </exception>
    /// <exception cref="DbUpdateException">
    /// Thrown when there are foreign key constraint violations that prevent deletion.
    /// </exception>
    /// <remarks>
    /// This operation is destructive and cannot be undone. Consider marking reminders as
    /// completed instead of deleting them to maintain maintenance history and scheduling records.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Delete a cancelled reminder
    /// await reminderService.DeleteReminderAsync(789);
    /// 
    /// // Verify deletion
    /// var deletedReminder = await reminderService.GetReminderByIdAsync(789);
    /// if (deletedReminder == null)
    /// {
    ///     Console.WriteLine("Reminder successfully deleted");
    /// }
    /// </code>
    /// </example>
    Task DeleteReminderAsync(int id);

    /// <summary>
    /// Completes a maintenance reminder and optionally creates a maintenance log entry and schedules a follow-up reminder.
    /// This method provides a comprehensive workflow for handling completed maintenance tasks.
    /// </summary>
    /// <param name="reminderId">
    /// The unique identifier of the reminder to complete. Must be a positive integer
    /// representing an existing active reminder in the database.
    /// </param>
    /// <param name="logDescription">
    /// A detailed description of the maintenance work performed. This will be used to create
    /// a log entry recording the completed maintenance. Cannot be null or empty.
    /// </param>
    /// <param name="logDate">
    /// The date when the maintenance was actually performed. This will be recorded in the
    /// maintenance log entry for accurate historical tracking.
    /// </param>
    /// <param name="recreate">
    /// Whether to create a new reminder for the next occurrence of this maintenance task.
    /// Set to true for recurring maintenance like oil changes, false for one-time tasks.
    /// </param>
    /// <param name="newDueDate">
    /// The due date for the new reminder if recreate is true. Must be provided when recreate
    /// is true. Should be null when recreate is false.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. Upon completion, the original reminder
    /// will be marked as completed, a maintenance log entry will be created, and optionally
    /// a new reminder will be scheduled.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when reminderId is less than or equal to zero, when logDescription is null or empty,
    /// or when recreate is true but newDueDate is not provided.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no reminder exists with the specified ID, when the reminder is already completed,
    /// when the database context is not properly initialized, or when there are database connectivity issues.
    /// </exception>
    /// <exception cref="DbUpdateException">
    /// Thrown when there are database constraint violations during the completion process.
    /// </exception>
    /// <remarks>
    /// This method performs multiple database operations as a logical unit of work:
    /// 1. Marks the original reminder as completed
    /// 2. Creates a detailed maintenance log entry
    /// 3. Optionally creates a new reminder for future maintenance
    /// 
    /// The method includes the Vehicle navigation property when loading the reminder to ensure
    /// proper context for log entry creation.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Complete an oil change reminder with follow-up scheduling
    /// await reminderService.CompleteReminderAsync(
    ///     reminderId: 123,
    ///     logDescription: "Oil change completed - 5W-30 synthetic oil, new filter installed. " +
    ///                    "Checked fluid levels and tire pressure. Next change in 6 months or 5000 miles.",
    ///     logDate: DateTime.Today,
    ///     recreate: true,
    ///     newDueDate: DateTime.Today.AddMonths(6)
    /// );
    /// 
    /// // Complete a one-time inspection reminder
    /// await reminderService.CompleteReminderAsync(
    ///     reminderId: 456,
    ///     logDescription: "Annual state inspection completed - passed all requirements",
    ///     logDate: DateTime.Today,
    ///     recreate: false,
    ///     newDueDate: null
    /// );
    /// </code>
    /// </example>
    Task CompleteReminderAsync(int reminderId, string logDescription, DateTime logDate, bool recreate, DateTime? newDueDate = null);

    /// <summary>
    /// Retrieves all reminders from the database with associated vehicle information for comprehensive reporting and management.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list of all
    /// <see cref="Reminder"/> entities with their associated <see cref="Vehicle"/> navigation properties
    /// populated. Returns an empty list if no reminders are found in the database.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context is not properly initialized, when there are database
    /// connectivity issues, or when the Vehicle navigation property cannot be loaded.
    /// </exception>
    /// <remarks>
    /// This method includes the Vehicle navigation property to provide complete information
    /// for administrative reporting, dashboard displays, and cross-vehicle maintenance planning.
    /// For applications with large amounts of reminder data, consider implementing pagination
    /// or filtering to improve performance.
    /// </remarks>
    /// <example>
    /// <code>
    /// var allReminders = await reminderService.GetAllRemindersWithVehicleAsync();
    /// var upcomingReminders = allReminders
    ///     .Where(r => !r.IsCompleted && r.DueDate <= DateTime.Today.AddDays(30))
    ///     .OrderBy(r => r.DueDate)
    ///     .ToList();
    /// 
    /// Console.WriteLine($"Upcoming maintenance in the next 30 days ({upcomingReminders.Count} items):");
    /// foreach (var reminder in upcomingReminders)
    /// {
    ///     Console.WriteLine($"- {reminder.DueDate:yyyy-MM-dd}: {reminder.Vehicle?.Make} {reminder.Vehicle?.Model} - {reminder.Description}");
    /// }
    /// </code>
    /// </example>
    Task<List<Reminder>> GetAllRemindersWithVehicleAsync();
}