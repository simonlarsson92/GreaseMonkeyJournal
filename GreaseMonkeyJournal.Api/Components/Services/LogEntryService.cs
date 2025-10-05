using Microsoft.EntityFrameworkCore;
using GreaseMonkeyJournal.Api.Components.Models;
using GreaseMonkeyJournal.Api.Components.DbContext;

namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Concrete implementation of <see cref="ILogEntryService"/> that provides maintenance log entry
/// management operations using Entity Framework Core with MariaDB backend.
/// </summary>
/// <remarks>
/// This service handles all CRUD operations for log entry entities, including proper relationship
/// management with Vehicle entities, navigation property handling, and database transaction management.
/// The service is registered as scoped in the dependency injection container to ensure proper
/// DbContext lifecycle management and change tracking.
/// 
/// The implementation includes specialized handling for Entity Framework navigation properties
/// to prevent relationship conflicts during add and update operations, while ensuring that
/// read operations include complete entity graphs for comprehensive data display.
/// </remarks>
/// <example>
/// Service registration in Program.cs:
/// <code>
/// builder.Services.AddScoped&lt;ILogEntryService, LogEntryService&gt;();
/// </code>
/// 
/// Usage in a Blazor component for maintenance tracking:
/// <code>
/// @inject ILogEntryService LogEntryService
/// 
/// private List&lt;LogEntry&gt; maintenanceHistory = new();
/// 
/// protected override async Task OnInitializedAsync()
/// {
///     if (VehicleId.HasValue)
///     {
///         maintenanceHistory = await LogEntryService.GetByVehicleIdAsync(VehicleId.Value);
///     }
/// }
/// 
/// private async Task AddMaintenanceRecord(LogEntry entry)
/// {
///     await LogEntryService.AddAsync(entry);
///     await RefreshHistory();
/// }
/// </code>
/// </example>
public class LogEntryService : ILogEntryService
{
    private readonly VehicleLogDbContext _context;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LogEntryService"/> class with the required dependencies.
    /// </summary>
    /// <param name="context">
    /// The Entity Framework database context for log entry operations. This context is used
    /// for all database interactions and must be properly configured with a valid connection string
    /// and appropriate entity relationship mappings.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the context parameter is null, which would prevent any database operations
    /// from being performed.
    /// </exception>
    /// <remarks>
    /// The constructor validates that the required dependencies are provided and stores them
    /// for use throughout the service lifetime. The DbContext is managed by the DI container
    /// and will be disposed automatically when the service scope ends, ensuring proper
    /// resource cleanup and connection management.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Typically handled by dependency injection, but manual instantiation would be:
    /// var dbContext = new VehicleLogDbContext(options);
    /// var logEntryService = new LogEntryService(dbContext);
    /// </code>
    /// </example>
    public LogEntryService(VehicleLogDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    /// <remarks>
    /// This implementation uses Entity Framework's Include method to eagerly load the Vehicle
    /// navigation property, providing complete information for each log entry. The query is
    /// executed asynchronously using ToListAsync to avoid blocking the calling thread.
    /// 
    /// For applications with large amounts of maintenance data, consider implementing
    /// pagination, filtering, or projection to optimize performance and reduce memory usage.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context has been disposed, when there are connectivity issues,
    /// or when the Vehicle navigation property cannot be loaded due to relationship mapping problems.
    /// </exception>
    async Task<List<LogEntry>> ILogEntryService.GetAllAsync()
    {
        try
        {
            return await _context.LogEntries.Include(le => le.Vehicle).ToListAsync();
        }
        catch (Exception ex) when (!(ex is ArgumentNullException))
        {
            throw new InvalidOperationException("Failed to retrieve log entries from the database.", ex);
        }
    }
    
    /// <inheritdoc />
    /// <remarks>
    /// This implementation uses Entity Framework's Include method combined with FirstOrDefaultAsync
    /// to load the specific log entry with its associated vehicle information. The method uses
    /// a LINQ expression to filter by ID rather than FindAsync to enable the Include operation.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided ID is less than or equal to zero.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context has been disposed, when there are connectivity issues,
    /// or when the Vehicle navigation property cannot be loaded.
    /// </exception>
    async Task<LogEntry?> ILogEntryService.GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Log entry ID must be a positive integer.", nameof(id));
            
        try
        {
            return await _context.LogEntries.Include(le => le.Vehicle).FirstOrDefaultAsync(le => le.Id == id);
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            throw new InvalidOperationException($"Failed to retrieve log entry with ID {id} from the database.", ex);
        }
    }
    
    /// <inheritdoc />
    /// <remarks>
    /// This implementation filters log entries by vehicle ID and includes the Vehicle navigation
    /// property for consistency with other service methods. The query uses Entity Framework's
    /// Where clause to filter efficiently at the database level before materializing results.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided vehicle ID is less than or equal to zero.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context has been disposed, when there are connectivity issues,
    /// or when the Vehicle navigation property cannot be loaded.
    /// </exception>
    async Task<List<LogEntry>> ILogEntryService.GetByVehicleIdAsync(int vehicleId)
    {
        if (vehicleId <= 0)
            throw new ArgumentException("Vehicle ID must be a positive integer.", nameof(vehicleId));
            
        try
        {
            return await _context.LogEntries
                .Include(le => le.Vehicle)
                .Where(le => le.VehicleId == vehicleId)
                .ToListAsync();
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            throw new InvalidOperationException($"Failed to retrieve log entries for vehicle ID {vehicleId} from the database.", ex);
        }
    }
    
    /// <inheritdoc />
    /// <remarks>
    /// This implementation explicitly sets the Vehicle navigation property to null before adding
    /// the entry to prevent Entity Framework relationship conflicts. The entry is added to the
    /// change tracker and persisted using SaveChangesAsync, which will populate the Id property
    /// with the database-generated value.
    /// 
    /// The method validates that the VehicleId references an existing vehicle through database
    /// foreign key constraints, which will throw a DbUpdateException if the reference is invalid.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the entry parameter is null.
    /// </exception>
    /// <exception cref="DbUpdateException">
    /// Thrown when there are database constraint violations, such as foreign key violations
    /// for invalid VehicleId values, or when required fields are missing.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context has been disposed or when there are connectivity issues.
    /// </exception>
    async Task ILogEntryService.AddAsync(LogEntry entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));
            
        try
        {
            // Ensure navigation property is not set for add to avoid relationship conflicts
            entry.Vehicle = null;
            _context.LogEntries.Add(entry);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Re-throw database-specific exceptions as-is for proper handling upstream
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to add the log entry to the database.", ex);
        }
    }
    
    /// <inheritdoc />
    /// <remarks>
    /// This implementation uses a sophisticated approach to update log entries while avoiding
    /// Entity Framework relationship conflicts. It first clears the Vehicle navigation property,
    /// then locates the existing entity in the database, and uses SetValues to copy all
    /// scalar properties from the input entity to the tracked entity.
    /// 
    /// The method explicitly marks the entity as modified to ensure change detection works
    /// correctly, which is particularly important when dealing with entities that might
    /// have been detached from the change tracker.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the entry parameter is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no log entry exists with the specified Id, when the database context
    /// has been disposed, or when there are connectivity issues.
    /// </exception>
    /// <exception cref="DbUpdateException">
    /// Thrown when there are database constraint violations during the update operation.
    /// </exception>
    /// <exception cref="DbUpdateConcurrencyException">
    /// Thrown when the entry has been modified by another process since it was loaded.
    /// </exception>
    async Task ILogEntryService.UpdateAsync(LogEntry entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        if (entry.Id <= 0)
            throw new ArgumentException("Reminder ID must be a positive integer.", nameof(entry.Id));

        try
        {
            // Clear navigation property to avoid relationship conflicts
            entry.Vehicle = null;

            // Find the existing entry from the database
            var existingEntry = await _context.LogEntries.FindAsync(entry.Id);
            if (existingEntry == null)
            {
                throw new InvalidOperationException($"Log entry with ID {entry.Id} not found in the database.");
            }

            // Update the existing entry properties
            _context.Entry(existingEntry).CurrentValues.SetValues(entry);
            
            // Explicitly mark as modified to ensure changes are detected
            _context.Entry(existingEntry).State = EntityState.Modified;
            
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Re-throw database-specific exceptions as-is for proper handling upstream
            throw;
        }
        catch (InvalidOperationException)
        {
            // Re-throw our specific exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to update the log entry in the database.", ex);
        }
    }
    
    /// <inheritdoc />
    /// <remarks>
    /// This implementation first attempts to find the log entry by ID, and only removes it if found.
    /// This approach prevents exceptions when trying to delete non-existent entities and makes
    /// the method idempotent. The operation will succeed silently if the log entry doesn't exist.
    /// 
    /// Database foreign key constraints may prevent deletion if the log entry is referenced
    /// by other entities, which will result in a DbUpdateException.
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
    async Task ILogEntryService.DeleteAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Log entry ID must be a positive integer.", nameof(id));
            
        try
        {
            var entry = await _context.LogEntries.FindAsync(id);
            if (entry != null)
            {
                _context.LogEntries.Remove(entry);
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
            throw new InvalidOperationException($"Failed to delete log entry with ID {id} from the database.", ex);
        }
    }
}
