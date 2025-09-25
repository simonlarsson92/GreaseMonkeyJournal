using Microsoft.EntityFrameworkCore;
using GreaseMonkeyJournal.Api.Components.Models;
using GreaseMonkeyJournal.Api.Components.DbContext;

namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Service for vehicle operations
/// </summary>
public class VehicleService : IVehicleService
{
    private readonly VehicleLogDbContext _context;
    
    /// <summary>
    /// Initializes a new instance of the VehicleService
    /// </summary>
    /// <param name="context">Database context</param>
    public VehicleService(VehicleLogDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<List<Vehicle>> GetAllAsync() => await _context.Vehicles.ToListAsync();
    
    /// <inheritdoc />
    public async Task<Vehicle?> GetByIdAsync(int id) => await _context.Vehicles.FindAsync(id);
    
    /// <inheritdoc />
    public async Task AddAsync(Vehicle vehicle)
    {
        if (vehicle == null)
            throw new ArgumentNullException(nameof(vehicle));
            
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();
    }
    
    /// <inheritdoc />
    public async Task UpdateAsync(Vehicle vehicle)
    {
        if (vehicle == null)
            throw new ArgumentNullException(nameof(vehicle));
            
        _context.Vehicles.Update(vehicle);
        await _context.SaveChangesAsync();
    }
    
    /// <inheritdoc />
    public async Task DeleteAsync(int id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle != null)
        {
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
        }
    }
}
