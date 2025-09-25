using GreaseMonkeyJournal.Api.Components.Models;

namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Interface for vehicle service operations
/// </summary>
public interface IVehicleService
{
    /// <summary>
    /// Get all vehicles
    /// </summary>
    /// <returns>List of all vehicles</returns>
    Task<List<Vehicle>> GetAllAsync();

    /// <summary>
    /// Get a vehicle by its ID
    /// </summary>
    /// <param name="id">Vehicle ID</param>
    /// <returns>Vehicle if found, null otherwise</returns>
    Task<Vehicle?> GetByIdAsync(int id);

    /// <summary>
    /// Add a new vehicle
    /// </summary>
    /// <param name="vehicle">Vehicle to add</param>
    Task AddAsync(Vehicle vehicle);

    /// <summary>
    /// Update an existing vehicle
    /// </summary>
    /// <param name="vehicle">Vehicle to update</param>
    Task UpdateAsync(Vehicle vehicle);

    /// <summary>
    /// Delete a vehicle by ID
    /// </summary>
    /// <param name="id">Vehicle ID to delete</param>
    Task DeleteAsync(int id);
}