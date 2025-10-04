using GreaseMonkeyJournal.Api.Components.Models;

namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Provides contract for vehicle management operations including CRUD functionality
/// for vehicle records in the Grease Monkey Journal application.
/// </summary>
/// <remarks>
/// This service interface defines the core operations for managing vehicle entities,
/// supporting the main vehicle tracking functionality of the application.
/// All methods are asynchronous to support scalable database operations.
/// </remarks>
/// <example>
/// Basic usage example:
/// <code>
/// // Dependency injection in a Blazor component
/// [Inject] public IVehicleService VehicleService { get; set; } = default!;
/// 
/// // Getting all vehicles
/// var vehicles = await VehicleService.GetAllAsync();
/// 
/// // Adding a new vehicle
/// var newVehicle = new Vehicle { Make = "Toyota", Model = "Camry", Year = 2022 };
/// await VehicleService.AddAsync(newVehicle);
/// </code>
/// </example>
public interface IVehicleService
{
    /// <summary>
    /// Retrieves all vehicles from the database with their complete information.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a list of all <see cref="Vehicle"/> entities in the database. Returns an empty
    /// list if no vehicles are found.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context is not properly initialized or when there are
    /// database connectivity issues.
    /// </exception>
    /// <example>
    /// <code>
    /// var allVehicles = await vehicleService.GetAllAsync();
    /// foreach (var vehicle in allVehicles)
    /// {
    ///     Console.WriteLine($"{vehicle.Year} {vehicle.Make} {vehicle.Model}");
    /// }
    /// </code>
    /// </example>
    Task<List<Vehicle>> GetAllAsync();

    /// <summary>
    /// Retrieves a specific vehicle by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the vehicle to retrieve. Must be a positive integer.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// the <see cref="Vehicle"/> entity if found, or null if no vehicle exists with the specified ID.
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
    /// var vehicle = await vehicleService.GetByIdAsync(123);
    /// if (vehicle != null)
    /// {
    ///     Console.WriteLine($"Found: {vehicle.Make} {vehicle.Model}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("Vehicle not found");
    /// }
    /// </code>
    /// </example>
    Task<Vehicle?> GetByIdAsync(int id);

    /// <summary>
    /// Adds a new vehicle to the database with validation and auto-generated timestamps.
    /// </summary>
    /// <param name="vehicle">
    /// The <see cref="Vehicle"/> entity to add. Must contain all required fields:
    /// Make, Model, Year, and VIN. The Id field will be auto-generated.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The vehicle will be persisted
    /// to the database and the entity will have its Id property populated upon successful completion.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the vehicle parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when required vehicle properties (Make, Model, Year, VIN) are missing or invalid.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the database context is not properly initialized or when there are
    /// database connectivity issues.
    /// </exception>
    /// <exception cref="DbUpdateException">
    /// Thrown when there's a database constraint violation (e.g., duplicate VIN).
    /// </exception>
    /// <example>
    /// <code>
    /// var newVehicle = new Vehicle
    /// {
    ///     Make = "Honda",
    ///     Model = "Civic",
    ///     Year = 2023,
    ///     VIN = "1HGBH41JXMN109186",
    ///     LicensePlate = "ABC123"
    /// };
    /// 
    /// await vehicleService.AddAsync(newVehicle);
    /// // newVehicle.Id now contains the database-generated ID
    /// </code>
    /// </example>
    Task AddAsync(Vehicle vehicle);

    /// <summary>
    /// Updates an existing vehicle in the database with the provided information.
    /// </summary>
    /// <param name="vehicle">
    /// The <see cref="Vehicle"/> entity with updated information. Must have a valid Id
    /// that exists in the database. All properties will be updated to match the provided entity.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The vehicle record in the database
    /// will be updated with the provided information upon successful completion.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the vehicle parameter is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the vehicle Id is invalid (less than or equal to zero) or when
    /// required properties are missing or invalid.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no vehicle exists with the specified Id, or when the database context
    /// is not properly initialized, or when there are database connectivity issues.
    /// </exception>
    /// <exception cref="DbUpdateException">
    /// Thrown when there's a database constraint violation during the update operation.
    /// </exception>
    /// <example>
    /// <code>
    /// var existingVehicle = await vehicleService.GetByIdAsync(123);
    /// if (existingVehicle != null)
    /// {
    ///     existingVehicle.Mileage = 50000;
    ///     existingVehicle.Notes = "Updated mileage after service";
    ///     await vehicleService.UpdateAsync(existingVehicle);
    /// }
    /// </code>
    /// </example>
    Task UpdateAsync(Vehicle vehicle);

    /// <summary>
    /// Removes a vehicle and all associated records from the database permanently.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the vehicle to delete. Must be a positive integer
    /// representing an existing vehicle in the database.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The vehicle and all related
    /// data (log entries, reminders) will be permanently removed from the database
    /// upon successful completion. No error is thrown if the vehicle doesn't exist.
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
    /// This operation is destructive and cannot be undone. Consider implementing
    /// soft deletes or archival functionality for production use cases where
    /// data recovery might be necessary.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Delete a vehicle by ID
    /// await vehicleService.DeleteAsync(123);
    /// 
    /// // Verify deletion
    /// var deletedVehicle = await vehicleService.GetByIdAsync(123);
    /// // deletedVehicle will be null if deletion was successful
    /// </code>
    /// </example>
    Task DeleteAsync(int id);
}