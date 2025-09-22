using Microsoft.EntityFrameworkCore;
using GreaseMonkeyJournal.Api.Components.Models;
using GreaseMonkeyJournal.Api.Components.DbContext;

namespace GreaseMonkeyJournal.Api.Components.Services;

public class VehicleService
{
    private readonly VehicleLogDbContext _context;
    public VehicleService(VehicleLogDbContext context)
    {
        _context = context;
    }

    public async Task<List<Vehicle>> GetAllAsync() => await _context.Vehicles.ToListAsync();
    public async Task<Vehicle?> GetByIdAsync(int id) => await _context.Vehicles.FindAsync(id);
    public async Task AddAsync(Vehicle vehicle)
    {
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateAsync(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
        await _context.SaveChangesAsync();
    }
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
