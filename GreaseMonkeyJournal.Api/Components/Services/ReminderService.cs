using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GreaseMonkeyJournal.Api.Components.DbContext;
using GreaseMonkeyJournal.Api.Components.Models;

namespace GreaseMonkeyJournal.Api.Components.Services
{
    /// <summary>
    /// Concrete implementation of <see cref="IReminderService"/> that provides maintenance reminder
    /// management operations using Entity Framework Core with MariaDB backend and integrated log entry creation.
    /// </summary>
    /// <remarks>
    /// This service handles all CRUD operations for reminder entities, including complex workflow operations
    /// such as reminder completion with automatic log entry generation and follow-up reminder scheduling.
    /// The service coordinates with <see cref="ILogEntryService"/> to maintain consistency between
    /// reminders and maintenance history records.
    /// 
    /// The implementation is designed to support comprehensive maintenance workflow management,
    /// including proactive reminder scheduling, completion tracking, and automatic record generation
    /// that provides a seamless experience for users managing vehicle maintenance schedules.
    /// 
    /// The service is registered as scoped in the dependency injection container to ensure proper
    /// DbContext lifecycle management and transaction coordination with dependent services.
    /// </remarks>
    /// <example>
    /// Service registration in Program.cs with dependencies:
    /// <code>
    /// builder.Services.AddScoped&lt;ILogEntryService, LogEntryService&gt;();
    /// builder.Services.AddScoped&lt;IReminderService, ReminderService&gt;();
    /// </code>
    /// 
    /// Usage in a Blazor component for maintenance workflow:
    /// <code>
    /// @inject IReminderService ReminderService
    /// 
    /// private List&lt;Reminder&gt; upcomingReminders = new();
    /// 
    /// protected override async Task OnInitializedAsync()
    /// {
    ///     await LoadUpcomingReminders();
    /// }
    /// 
    /// private async Task LoadUpcomingReminders()
    /// {
    ///     var allReminders = await ReminderService.GetAllRemindersWithVehicleAsync();
    ///     upcomingReminders = allReminders
    ///         .Where(r => !r.IsCompleted && r.DueDate <= DateTime.Today.AddDays(30))
    ///         .OrderBy(r => r.DueDate)
    ///         .ToList();
    /// }
    /// 
    /// private async Task CompleteMaintenanceTask(Reminder reminder, string workDescription)
    /// {
    ///     await ReminderService.CompleteReminderAsync(
    ///         reminder.Id,
    ///         workDescription,
    ///         DateTime.Today,
    ///         true, // Create follow-up reminder
    ///         DateTime.Today.AddMonths(6)
    ///     );
    ///     await LoadUpcomingReminders(); // Refresh the list
    /// }
    /// </code>
    /// </example>
    public class ReminderService : IReminderService
    {
        private readonly VehicleLogDbContext _context;
        private readonly ILogEntryService _logEntryService;
        private readonly ILogger<ReminderService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReminderService"/> class with the required dependencies.
        /// </summary>
        /// <param name="context">
        /// The Entity Framework database context for reminder operations. This context is used
        /// for all database interactions and must be properly configured with a valid connection string
        /// and appropriate entity relationship mappings.
        /// </param>
        /// <param name="logEntryService">
        /// The log entry service dependency used for creating maintenance records when reminders
        /// are completed. This service enables the integrated workflow between reminder completion
        /// and maintenance history tracking.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either the context or logEntryService parameter is null, which would prevent
        /// proper service functionality and database operations.
        /// </exception>
        /// <remarks>
        /// The constructor validates that all required dependencies are provided and stores them
        /// for use throughout the service lifetime. Both the DbContext and ILogEntryService are
        /// managed by the DI container and will be disposed automatically when the service scope ends.
        /// 
        /// The dependency on ILogEntryService enables the CompleteReminderAsync method to create
        /// maintenance log entries automatically, providing a seamless workflow for users completing
        /// maintenance tasks directly from reminder notifications.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Typically handled by dependency injection, but manual instantiation would be:
        /// var dbContext = new VehicleLogDbContext(options);
        /// var logEntryService = new LogEntryService(dbContext);
        /// var reminderService = new ReminderService(dbContext, logEntryService);
        /// </code>
        /// </example>
        public ReminderService(VehicleLogDbContext context, ILogEntryService logEntryService, ILogger<ReminderService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logEntryService = logEntryService ?? throw new ArgumentNullException(nameof(logEntryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        /// <remarks>
        /// This implementation filters reminders by vehicle ID using Entity Framework's Where clause
        /// to efficiently query the database. The method returns all reminders (both active and completed)
        /// for the specified vehicle, allowing clients to filter by completion status as needed.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the provided vehicle ID is less than or equal to zero.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the database context has been disposed or when there are connectivity issues.
        /// </exception>
        async Task<List<Reminder>> IReminderService.GetRemindersForVehicleAsync(int vehicleId)
        {
            if (vehicleId <= 0)
            {
                _logger.LogWarning("Attempted to retrieve reminders with invalid vehicle ID: {VehicleId}", vehicleId);
                throw new ArgumentException("Vehicle ID must be a positive integer.", nameof(vehicleId));
            }
                
            _logger.LogDebug("Retrieving reminders for vehicle ID: {VehicleId}", vehicleId);
            try
            {
                var reminders = await _context.Reminders.Where(r => r.VehicleId == vehicleId).ToListAsync();
                _logger.LogInformation("Retrieved {ReminderCount} reminders for vehicle {VehicleId}", reminders.Count, vehicleId);
                return reminders;
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                _logger.LogError(ex, "Failed to retrieve reminders for vehicle ID {VehicleId}", vehicleId);
                throw new InvalidOperationException($"Failed to retrieve reminders for vehicle ID {vehicleId} from the database.", ex);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// This implementation uses Entity Framework's FindAsync method which first checks the
        /// change tracker for the entity before querying the database, providing optimal performance
        /// for scenarios where the entity might already be loaded in the current context.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the provided ID is less than or equal to zero.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the database context has been disposed or when there are connectivity issues.
        /// </exception>
        async Task<Reminder?> IReminderService.GetReminderByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Attempted to retrieve reminder with invalid ID: {ReminderId}", id);
                throw new ArgumentException("Reminder ID must be a positive integer.", nameof(id));
            }
                
            _logger.LogDebug("Retrieving reminder with ID: {ReminderId}", id);
            try
            {
                var reminder = await _context.Reminders.FindAsync(id);
                if (reminder != null)
                {
                    _logger.LogDebug("Found reminder {ReminderId} for vehicle {VehicleId}", id, reminder.VehicleId);
                }
                else
                {
                    _logger.LogDebug("Reminder with ID {ReminderId} not found", id);
                }
                return reminder;
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                _logger.LogError(ex, "Failed to retrieve reminder with ID {ReminderId}", id);
                throw new InvalidOperationException($"Failed to retrieve reminder with ID {id} from the database.", ex);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// This implementation adds the reminder to the change tracker and persists it to the database.
        /// The entity's ID will be populated with the database-generated value after SaveChangesAsync completes.
        /// Database foreign key constraints will validate that the VehicleId references an existing vehicle.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the reminder parameter is null.
        /// </exception>
        /// <exception cref="DbUpdateException">
        /// Thrown when there are database constraint violations, such as foreign key violations
        /// for invalid VehicleId values, or when required fields are missing.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the database context has been disposed or when there are connectivity issues.
        /// </exception>
        async Task IReminderService.AddReminderAsync(Reminder reminder)
        {
            if (reminder == null)
            {
                _logger.LogWarning("Attempted to add null reminder");
                throw new ArgumentNullException(nameof(reminder));
            }
                
            _logger.LogInformation("Adding new reminder for vehicle {VehicleId}: {Type}", reminder.VehicleId, reminder.Type);
            try
            {
                _context.Reminders.Add(reminder);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully added reminder with ID: {ReminderId} for vehicle {VehicleId}", reminder.Id, reminder.VehicleId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database constraint violation while adding reminder for vehicle {VehicleId}", reminder.VehicleId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add reminder for vehicle {VehicleId}", reminder.VehicleId);
                throw new InvalidOperationException("Failed to add the reminder to the database.", ex);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// This implementation updates the entire entity using Entity Framework's Update method,
        /// which marks all properties as modified. The method ensures that changes are properly
        /// tracked and persisted to the database while maintaining referential integrity.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the reminder parameter is null.
        /// </exception>
        /// <exception cref="DbUpdateException">
        /// Thrown when there are database constraint violations during the update operation.
        /// </exception>
        /// <exception cref="DbUpdateConcurrencyException">
        /// Thrown when the reminder has been modified by another process since it was loaded.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the database context has been disposed or when there are connectivity issues.
        /// </exception>
        async Task IReminderService.UpdateReminderAsync(Reminder reminder)
        {
            if (reminder == null)
                throw new ArgumentNullException(nameof(reminder));
                
            try
            {
                _context.Reminders.Update(reminder);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Re-throw database-specific exceptions as-is for proper handling upstream
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update the reminder in the database.", ex);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// This implementation first attempts to find the reminder by ID, and only removes it if found.
        /// This approach prevents exceptions when trying to delete non-existent entities and makes
        /// the method idempotent. The operation will succeed silently if the reminder doesn't exist.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the provided ID is less than or equal to zero.
        /// </exception>
        /// <exception cref="DbUpdateException">
        /// Thrown when there are foreign key constraint violations that prevent deletion.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the database context has been disposed or when there are connectivity issues.
        /// </exception>
        async Task IReminderService.DeleteReminderAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Reminder ID must be a positive integer.", nameof(id));
                
            try
            {
                var reminder = await _context.Reminders.FindAsync(id);
                if (reminder != null)
                {
                    _context.Reminders.Remove(reminder);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException)
            {
                // Re-throw database-specific exceptions as-is for proper handling upstream
                throw;
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                throw new InvalidOperationException($"Failed to delete reminder with ID {id} from the database.", ex);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method implements a comprehensive maintenance completion workflow that performs
        /// multiple coordinated operations:
        /// 
        /// 1. Loads the reminder with Vehicle navigation property for context
        /// 2. Marks the reminder as completed and saves the change
        /// 3. Creates a detailed maintenance log entry via ILogEntryService
        /// 4. Optionally creates a new reminder for future maintenance if requested
        /// 
        /// The operations are performed sequentially with proper error handling to ensure
        /// data consistency. If any operation fails, previous changes may already be committed
        /// to the database, so consider implementing transaction management for critical scenarios.
        /// 
        /// The method validates all parameters before beginning operations and provides
        /// comprehensive error information for troubleshooting integration issues.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when reminderId is less than or equal to zero or when logDescription is null or empty.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when recreate is true but newDueDate is not provided, or when newDueDate is
        /// provided but recreate is false.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no reminder exists with the specified ID, when the reminder is already completed,
        /// when the database context has been disposed, or when there are connectivity issues.
        /// </exception>
        /// <exception cref="DbUpdateException">
        /// Thrown when there are database constraint violations during any of the database operations.
        /// </exception>
        async Task IReminderService.CompleteReminderAsync(int reminderId, string logDescription, DateTime logDate, bool recreate, DateTime? newDueDate = null)
        {
            if (reminderId <= 0)
                throw new ArgumentException("Reminder ID must be a positive integer.", nameof(reminderId));
                
            if (string.IsNullOrWhiteSpace(logDescription))
                throw new ArgumentException("Log description cannot be null or empty", nameof(logDescription));

            if (recreate && !newDueDate.HasValue)
                throw new ArgumentException("New due date must be provided when recreate is true.", nameof(newDueDate));
                
            if (!recreate && newDueDate.HasValue)
                throw new ArgumentException("New due date should not be provided when recreate is false.", nameof(newDueDate));
                
            try
            {
                // Load the reminder with vehicle information for context
                var reminder = await _context.Reminders.Include(r => r.Vehicle).FirstOrDefaultAsync(r => r.Id == reminderId);
                if (reminder == null)
                {
                    throw new InvalidOperationException($"Reminder with ID {reminderId} not found in the database.");
                }

                if (reminder.IsCompleted)
                {
                    throw new InvalidOperationException($"Reminder with ID {reminderId} is already completed.");
                }

                // Mark reminder as completed
                reminder.IsCompleted = true;
                await _context.SaveChangesAsync();

                // Create maintenance log entry
                var logEntry = new LogEntry
                {
                    VehicleId = reminder.VehicleId,
                    Description = logDescription,
                    Date = logDate
                };
                await _logEntryService.AddAsync(logEntry);

                // Optionally recreate reminder for future maintenance
                if (recreate && newDueDate.HasValue)
                {
                    var newReminder = new Reminder
                    {
                        VehicleId = reminder.VehicleId,
                        Description = reminder.Description,
                        DueDate = newDueDate.Value,
                        Type = reminder.Type,
                        IsCompleted = false
                    };
                    _context.Reminders.Add(newReminder);
                    await _context.SaveChangesAsync();
                }
            }
            catch (ArgumentException)
            {
                // Re-throw our specific argument exceptions as-is
                throw;
            }
            catch (InvalidOperationException)
            {
                // Re-throw our specific operation exceptions as-is
                throw;
            }
            catch (DbUpdateException)
            {
                // Re-throw database-specific exceptions as-is for proper handling upstream
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to complete reminder with ID {reminderId}.", ex);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// This implementation uses Entity Framework's Include method to eagerly load the Vehicle
        /// navigation property, providing complete information for each reminder. This is particularly
        /// useful for administrative dashboards, reporting, and cross-vehicle maintenance planning.
        /// 
        /// The query executes asynchronously using ToListAsync to avoid blocking the calling thread.
        /// For applications with large amounts of reminder data, consider implementing pagination
        /// or filtering mechanisms to optimize performance and reduce memory usage.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the database context has been disposed, when there are connectivity issues,
        /// or when the Vehicle navigation property cannot be loaded due to relationship mapping problems.
        /// </exception>
        async Task<List<Reminder>> IReminderService.GetAllRemindersWithVehicleAsync()
        {
            try
            {
                return await _context.Reminders.Include(r => r.Vehicle).ToListAsync();
            }
            catch (Exception ex) when (!(ex is ArgumentNullException))
            {
                throw new InvalidOperationException("Failed to retrieve reminders with vehicle information from the database.", ex);
            }
        }
    }
}
