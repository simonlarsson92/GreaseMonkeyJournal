using GreaseMonkeyJournal.Api.Components.Models;

namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Provides contract for managing <see cref="VehicleInfoCard"/> records, which capture free-form
/// titled information (component article numbers, fluid specifications, etc.) attached to a vehicle.
/// </summary>
/// <remarks>
/// All operations are asynchronous and align with the Entity Framework Core change tracking model
/// used throughout the application. Cards are always scoped to a single owning vehicle.
/// </remarks>
public interface IVehicleInfoCardService
{
    /// <summary>
    /// Retrieves all information cards belonging to the specified vehicle.
    /// </summary>
    /// <param name="vehicleId">The owning vehicle's identifier. Must be a positive integer.</param>
    /// <returns>A list of <see cref="VehicleInfoCard"/> entities; empty when none exist.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="vehicleId"/> is less than or equal to zero.</exception>
    Task<List<VehicleInfoCard>> GetByVehicleIdAsync(int vehicleId);

    /// <summary>
    /// Retrieves a single information card by its unique identifier.
    /// </summary>
    /// <param name="id">The card identifier. Must be a positive integer.</param>
    /// <returns>The matching <see cref="VehicleInfoCard"/>, or null when not found.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is less than or equal to zero.</exception>
    Task<VehicleInfoCard?> GetByIdAsync(int id);

    /// <summary>
    /// Adds a new information card to the database.
    /// </summary>
    /// <param name="card">The card to persist. Its <see cref="VehicleInfoCard.Id"/> is populated on success.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="card"/> is null.</exception>
    Task AddAsync(VehicleInfoCard card);

    /// <summary>
    /// Updates an existing information card.
    /// </summary>
    /// <param name="card">The card with updated values; must have a valid <see cref="VehicleInfoCard.Id"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="card"/> is null.</exception>
    Task UpdateAsync(VehicleInfoCard card);

    /// <summary>
    /// Removes an information card by its identifier. Succeeds silently when the card does not exist.
    /// </summary>
    /// <param name="id">The card identifier. Must be a positive integer.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is less than or equal to zero.</exception>
    Task DeleteAsync(int id);
}
