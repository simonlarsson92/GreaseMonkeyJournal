using Microsoft.EntityFrameworkCore;
using GreaseMonkeyJournal.Api.Components.Models;
using GreaseMonkeyJournal.Api.Components.DbContext;

namespace GreaseMonkeyJournal.Api.Components.Services;

public class LogEntryService
{
    private readonly VehicleLogDbContext _context;
    public LogEntryService(VehicleLogDbContext context)
    {
        _context = context;
    }

    public async Task<List<LogEntry>> GetAllAsync() => await _context.LogEntries.Include(le => le.Vehicle).ToListAsync();
    public async Task<LogEntry?> GetByIdAsync(int id) => await _context.LogEntries.Include(le => le.Vehicle).FirstOrDefaultAsync(le => le.Id == id);
    public async Task<List<LogEntry>> GetByVehicleIdAsync(int vehicleId) => await _context.LogEntries.Include(le => le.Vehicle).Where(le => le.VehicleId == vehicleId).ToListAsync();
    public async Task AddAsync(LogEntry entry)
    {
        entry.Vehicle = null; // Ensure navigation property is not set for add
        _context.LogEntries.Add(entry);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateAsync(LogEntry entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        // Clear navigation property to avoid relationship conflicts
        entry.Vehicle = null;

        // Find the existing entry from the database
        var existingEntry = await _context.LogEntries.FindAsync(entry.Id);
        if (existingEntry != null)
        {
            // Update the existing entry properties
            _context.Entry(existingEntry).CurrentValues.SetValues(entry);
            
            // Explicitly mark as modified to ensure changes are detected
            _context.Entry(existingEntry).State = EntityState.Modified;
            
            await _context.SaveChangesAsync();
        }
    }
    public async Task DeleteAsync(int id)
    {
        var entry = await _context.LogEntries.FindAsync(id);
        if (entry != null)
        {
            _context.LogEntries.Remove(entry);
            await _context.SaveChangesAsync();
        }
    }
}
