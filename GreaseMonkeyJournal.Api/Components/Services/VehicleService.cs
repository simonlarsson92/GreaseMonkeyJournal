using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GreaseMonkeyJournal.Api.Components.Models;
using GreaseMonkeyJournal.Api.Components.DbContext;

namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Concrete implementation of <see cref="IVehicleService"/> that provides vehicle management 
/// operations using Entity Framework Core with MariaDB backend.
/// </summary>
/// <remarks>
/// This service handles all CRUD operations for vehicle entities, including proper error handling,
/// validation, and database transaction management. The service is registered as scoped in the
/// dependency injection container to ensure proper DbContext lifecycle management.
/// 
/// The implementation uses Entity Framework Core's change tracking and ensures proper
/// disposal of database resources through the injected DbContext.
/// </remarks>
/// <example>
/// Service registration in Program.cs:
/// <code>
/// builder.Services.AddScoped&lt;IVehicleService, VehicleService&gt;();
/// </code>
/// 
/// Usage in a Blazor component:
/// <code>
/// @inject IVehicleService VehicleService
/// 
/// protected override async Task OnInitializedAsync()
/// {
///     vehicles = await VehicleService.GetAllAsync();
/// }
/// </code>
/// </example>
public class VehicleService : IVehicleService
{
    private readonly VehicleLogDbContext _context;
    private readonly ILogger<VehicleService> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleService"/> class with the required dependencies.
    /// </summary>
    /// <param name="context">
    /// The Entity Framework database context for vehicle operations. This context is used
    /// for all database interactions and must be properly configured with a valid connection string.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the context parameter is null, which would prevent any database operations.
    /// </exception>
    /// <remarks>
    /// The constructor validates that the required dependencies are provided and stores them
    /// for use throughout the service lifetime. The DbContext is managed by the DI container
    /// and will be disposed automatically when the service scope ends.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Typically handled by dependency injection, but manual instantiation would be:
    /// var dbContext = new VehicleLogDbContext(options);
    /// var vehicleService = new VehicleService(dbContext);
    /// </code>
    /// </example>
    public VehicleService(VehicleLogDbContext context, ILogger<VehicleService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    /// <remarks>
    /// This implementation retrieves all vehicle records from the database using Entity Framework's
    /// ToListAsync method, which executes the query asynchronously and materializes all results.
    /// For large datasets, consider implementing pagination or filtering mechanisms.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context has been disposed or when there are connectivity issues.
    /// </exception>
    /// <exception cref="SqlException">
    /// Thrown when there are database-specific errors such as connection timeouts or syntax errors.
    /// </exception>
    async Task<List<Vehicle>> IVehicleService.GetAllAsync()
    {
        _logger.LogDebug("Retrieving all vehicles from database");
        try
        {
            var vehicles = await _context.Vehicles.ToListAsync();
            _logger.LogInformation("Retrieved {VehicleCount} vehicles from database", vehicles.Count);
            return vehicles;
        }
        catch (Exception ex) when (!(ex is ArgumentNullException))
        {
            _logger.LogError(ex, "Failed to retrieve vehicles from database");
            throw new InvalidOperationException("Failed to retrieve vehicles from the database.", ex);
        }
    }
    
    /// <inheritdoc />
    /// <remarks>
    /// This implementation uses Entity Framework's FindAsync method which first checks the
    /// change tracker for the entity before querying the database, providing optimal performance
    /// for scenarios where the entity might already be loaded.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided ID is less than or equal to zero.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context has been disposed or when there are connectivity issues.
    /// </exception>
    async Task<Vehicle?> IVehicleService.GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Attempted to retrieve vehicle with invalid ID: {VehicleId}", id);
            throw new ArgumentException("Vehicle ID must be a positive integer.", nameof(id));
        }
            
        _logger.LogDebug("Retrieving vehicle with ID: {VehicleId}", id);
        try
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                _logger.LogDebug("Found vehicle {VehicleId}: {Make} {Model}", id, vehicle.Make, vehicle.Model);
            }
            else
            {
                _logger.LogDebug("Vehicle with ID {VehicleId} not found", id);
            }
            return vehicle;
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            _logger.LogError(ex, "Failed to retrieve vehicle with ID {VehicleId}", id);
            throw new InvalidOperationException($"Failed to retrieve vehicle with ID {id} from the database.", ex);
        }
    }
    
    /// <inheritdoc />
    /// <remarks>
    /// This implementation adds the vehicle to the change tracker and persists it to the database.
    /// The entity's ID will be populated with the database-generated value after SaveChangesAsync completes.
    /// All required validations should be performed by the Entity Framework model validations.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the vehicle parameter is null.
    /// </exception>
    /// <exception cref="DbUpdateException">
    /// Thrown when there are database constraint violations, such as duplicate VIN numbers.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context has been disposed or when there are connectivity issues.
    /// </exception>
    async Task IVehicleService.AddAsync(Vehicle vehicle)
    {
        if (vehicle == null)
        {
            _logger.LogWarning("Attempted to add null vehicle");
            throw new ArgumentNullException(nameof(vehicle));
        }
            
        _logger.LogInformation("Adding new vehicle: {Make} {Model} {Year}", vehicle.Make, vehicle.Model, vehicle.Year);
        try
        {
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully added vehicle with ID: {VehicleId}", vehicle.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database constraint violation while adding vehicle: {Make} {Model}", vehicle.Make, vehicle.Model);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add vehicle: {Make} {Model}", vehicle.Make, vehicle.Model);
            throw new InvalidOperationException("Failed to add the vehicle to the database.", ex);
        }
    }
    
    /// <inheritdoc />
    /// <remarks>
    /// This implementation updates the entire entity using Entity Framework's Update method,
    /// which marks all properties as modified. For partial updates, consider using
    /// Entry(entity).CurrentValues.SetValues() or attaching and selectively marking properties.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the vehicle parameter is null.
    /// </exception>
    /// <exception cref="DbUpdateException">
    /// Thrown when there are database constraint violations during the update operation.
    /// </exception>
    /// <exception cref="DbUpdateConcurrencyException">
    /// Thrown when the entity has been modified by another process since it was loaded.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context has been disposed or when there are connectivity issues.
    /// </exception>
    async Task IVehicleService.UpdateAsync(Vehicle vehicle)
    {
        if (vehicle == null)
        {
            _logger.LogWarning("Attempted to update null vehicle");
            throw new ArgumentNullException(nameof(vehicle));
        }
            
        _logger.LogInformation("Updating vehicle with ID: {VehicleId}", vehicle.Id);
        try
        {
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully updated vehicle with ID: {VehicleId}", vehicle.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database constraint violation while updating vehicle with ID: {VehicleId}", vehicle.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update vehicle with ID: {VehicleId}", vehicle.Id);
            throw new InvalidOperationException("Failed to update the vehicle in the database.", ex);
        }
    }
    
    /// <inheritdoc />
    /// <remarks>
    /// This implementation first attempts to find the vehicle by ID, and only removes it if found.
    /// This approach prevents exceptions when trying to delete non-existent entities.
    /// The operation will succeed silently if the vehicle doesn't exist.
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
    async Task IVehicleService.DeleteAsync(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Attempted to delete vehicle with invalid ID: {VehicleId}", id);
            throw new ArgumentException("Vehicle ID must be a positive integer.", nameof(id));
        }
            
        _logger.LogInformation("Attempting to delete vehicle with ID: {VehicleId}", id);
        try
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted vehicle with ID: {VehicleId}", id);
            }
            else
            {
                _logger.LogWarning("Attempted to delete non-existent vehicle with ID: {VehicleId}", id);
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database constraint violation while deleting vehicle with ID: {VehicleId}. Vehicle may have related records", id);
            throw;
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            _logger.LogError(ex, "Failed to delete vehicle with ID: {VehicleId}", id);
            throw new InvalidOperationException($"Failed to delete vehicle with ID {id} from the database.", ex);
        }
    }
}
