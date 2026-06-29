using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GreaseMonkeyJournal.Api.Components.Models;
using GreaseMonkeyJournal.Api.Components.DbContext;

namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Concrete implementation of <see cref="IVehicleInfoCardService"/> backed by Entity Framework Core.
/// </summary>
/// <remarks>
/// Mirrors the patterns used by the other entity services: constructor dependency validation,
/// structured logging, defensive error handling, and clearing of the <see cref="VehicleInfoCard.Vehicle"/>
/// navigation property before write operations to avoid relationship-tracking conflicts.
/// </remarks>
public class VehicleInfoCardService : IVehicleInfoCardService
{
    private readonly VehicleLogDbContext _context;
    private readonly ILogger<VehicleInfoCardService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleInfoCardService"/> class.
    /// </summary>
    /// <param name="context">The Entity Framework database context.</param>
    /// <param name="logger">The logger used for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when any dependency is null.</exception>
    public VehicleInfoCardService(VehicleLogDbContext context, ILogger<VehicleInfoCardService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    async Task<List<VehicleInfoCard>> IVehicleInfoCardService.GetByVehicleIdAsync(int vehicleId)
    {
        if (vehicleId <= 0)
        {
            throw new ArgumentException("Vehicle ID must be a positive integer.", nameof(vehicleId));
        }

        try
        {
            var cards = await _context.VehicleInfoCards
                .Where(c => c.VehicleId == vehicleId)
                .OrderBy(c => c.Id)
                .ToListAsync();
            return cards;
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            _logger.LogError(ex, "Failed to retrieve information cards for vehicle ID {VehicleId}", vehicleId);
            throw new InvalidOperationException($"Failed to retrieve information cards for vehicle ID {vehicleId} from the database.", ex);
        }
    }

    /// <inheritdoc />
    async Task<VehicleInfoCard?> IVehicleInfoCardService.GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Information card ID must be a positive integer.", nameof(id));
        }

        try
        {
            return await _context.VehicleInfoCards.FindAsync(id);
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            _logger.LogError(ex, "Failed to retrieve information card with ID {CardId}", id);
            throw new InvalidOperationException($"Failed to retrieve information card with ID {id} from the database.", ex);
        }
    }

    /// <inheritdoc />
    async Task IVehicleInfoCardService.AddAsync(VehicleInfoCard card)
    {
        if (card == null)
        {
            throw new ArgumentNullException(nameof(card));
        }

        _logger.LogInformation("Adding information card '{Title}' for vehicle ID {VehicleId}", card.Title, card.VehicleId);
        try
        {
            card.Vehicle = null; // avoid relationship tracking conflicts
            _context.VehicleInfoCards.Add(card);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully added information card with ID {CardId}", card.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database constraint violation while adding information card for vehicle ID {VehicleId}", card.VehicleId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add information card for vehicle ID {VehicleId}", card.VehicleId);
            throw new InvalidOperationException("Failed to add the information card to the database.", ex);
        }
    }

    /// <inheritdoc />
    async Task IVehicleInfoCardService.UpdateAsync(VehicleInfoCard card)
    {
        if (card == null)
        {
            throw new ArgumentNullException(nameof(card));
        }

        _logger.LogInformation("Updating information card with ID {CardId}", card.Id);
        try
        {
            // Load-and-copy rather than DbSet.Update: the relationship to Vehicle is required and
            // bidirectional, so attaching a detached graph (or nulling the navigation) can orphan the
            // card or conflict with an already-tracked instance in the request-scoped context.
            var existing = await _context.VehicleInfoCards.FindAsync(card.Id);
            if (existing == null)
            {
                _logger.LogWarning("Attempted to update non-existent information card with ID {CardId}", card.Id);
                return;
            }

            existing.VehicleId = card.VehicleId;
            existing.Title = card.Title;
            existing.Details = card.Details;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully updated information card with ID {CardId}", card.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database constraint violation while updating information card with ID {CardId}", card.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update information card with ID {CardId}", card.Id);
            throw new InvalidOperationException("Failed to update the information card in the database.", ex);
        }
    }

    /// <inheritdoc />
    async Task IVehicleInfoCardService.DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Information card ID must be a positive integer.", nameof(id));
        }

        _logger.LogInformation("Attempting to delete information card with ID {CardId}", id);
        try
        {
            var card = await _context.VehicleInfoCards.FindAsync(id);
            if (card != null)
            {
                _context.VehicleInfoCards.Remove(card);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted information card with ID {CardId}", id);
            }
            else
            {
                _logger.LogWarning("Attempted to delete non-existent information card with ID {CardId}", id);
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database constraint violation while deleting information card with ID {CardId}", id);
            throw;
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            _logger.LogError(ex, "Failed to delete information card with ID {CardId}", id);
            throw new InvalidOperationException($"Failed to delete information card with ID {id} from the database.", ex);
        }
    }
}
