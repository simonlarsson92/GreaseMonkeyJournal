using GreaseMonkeyJournal.Api.Components.DbContext;
using GreaseMonkeyJournal.Api.Components.Models;
using Microsoft.EntityFrameworkCore;

namespace GreaseMonkeyJournal.Api.Components.Services;

/// <summary>
/// Service for managing notification settings.
/// </summary>
public class NotificationSettingsService : INotificationSettingsService
{
    private readonly VehicleLogDbContext _context;

    public NotificationSettingsService(VehicleLogDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationSettings?> GetByEmailAsync(string email)
    {
        return await _context.NotificationSettings
            .FirstOrDefaultAsync(s => s.UserEmail == email);
    }

    public async Task<List<NotificationSettings>> GetAllEnabledAsync()
    {
        return await _context.NotificationSettings
            .Where(s => s.IsEnabled)
            .ToListAsync();
    }

    public async Task<NotificationSettings> SaveAsync(NotificationSettings settings)
    {
        var existing = await GetByEmailAsync(settings.UserEmail);
        
        if (existing != null)
        {
            // Update existing
            existing.IsEnabled = settings.IsEnabled;
            existing.Send7DayReminders = settings.Send7DayReminders;
            existing.Send1DayReminders = settings.Send1DayReminders;
            existing.SendTodayReminders = settings.SendTodayReminders;
            existing.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return existing;
        }
        else
        {
            // Create new
            settings.CreatedAt = DateTime.UtcNow;
            settings.UpdatedAt = DateTime.UtcNow;
            _context.NotificationSettings.Add(settings);
            await _context.SaveChangesAsync();
            return settings;
        }
    }

    public async Task<NotificationSettings> GetOrCreateDefaultAsync()
    {
        var existing = await _context.NotificationSettings.FirstOrDefaultAsync();
        
        if (existing != null)
        {
            return existing;
        }

        // Create default settings
        var defaultSettings = new NotificationSettings
        {
            UserEmail = "",
            IsEnabled = false, // Disabled by default until user configures
            Send7DayReminders = true,
            Send1DayReminders = true,
            SendTodayReminders = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.NotificationSettings.Add(defaultSettings);
        await _context.SaveChangesAsync();
        
        return defaultSettings;
    }
}
