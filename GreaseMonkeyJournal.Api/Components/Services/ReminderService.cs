using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GreaseMonkeyJournal.Api.Components.DbContext;
using GreaseMonkeyJournal.Api.Components.Models;

namespace GreaseMonkeyJournal.Api.Components.Services
{
    /// <summary>
    /// Service for reminder operations
    /// </summary>
    public class ReminderService : IReminderService
    {
        private readonly VehicleLogDbContext _context;
        private readonly ILogEntryService _logEntryService;

        /// <summary>
        /// Initializes a new instance of the ReminderService
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="logEntryService">Log entry service dependency</param>
        public ReminderService(VehicleLogDbContext context, ILogEntryService logEntryService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logEntryService = logEntryService ?? throw new ArgumentNullException(nameof(logEntryService));
        }

        /// <inheritdoc />
        public async Task<List<Reminder>> GetRemindersForVehicleAsync(int vehicleId)
        {
            return await _context.Reminders.Where(r => r.VehicleId == vehicleId).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Reminder?> GetReminderByIdAsync(int id)
        {
            return await _context.Reminders.FindAsync(id);
        }

        /// <inheritdoc />
        public async Task AddReminderAsync(Reminder reminder)
        {
            if (reminder == null)
                throw new ArgumentNullException(nameof(reminder));
                
            _context.Reminders.Add(reminder);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task UpdateReminderAsync(Reminder reminder)
        {
            if (reminder == null)
                throw new ArgumentNullException(nameof(reminder));
                
            _context.Reminders.Update(reminder);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteReminderAsync(int id)
        {
            var reminder = await _context.Reminders.FindAsync(id);
            if (reminder != null)
            {
                _context.Reminders.Remove(reminder);
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public async Task CompleteReminderAsync(int reminderId, string logDescription, DateTime logDate, bool recreate, DateTime? newDueDate = null)
        {
            if (string.IsNullOrWhiteSpace(logDescription))
                throw new ArgumentException("Log description cannot be null or empty", nameof(logDescription));
                
            var reminder = await _context.Reminders.Include(r => r.Vehicle).FirstOrDefaultAsync(r => r.Id == reminderId);
            if (reminder == null) return;

            reminder.IsCompleted = true;
            await _context.SaveChangesAsync();

            // Create log entry
            var logEntry = new LogEntry
            {
                VehicleId = reminder.VehicleId,
                Description = logDescription,
                Date = logDate
            };
            await _logEntryService.AddAsync(logEntry);

            // Optionally recreate reminder
            if (recreate && newDueDate.HasValue)
            {
                var newReminder = new Reminder
                {
                    VehicleId = reminder.VehicleId,
                    Description = reminder.Description,
                    DueDate = newDueDate.Value,
                    IsCompleted = false
                };
                _context.Reminders.Add(newReminder);
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public async Task<List<Reminder>> GetAllRemindersWithVehicleAsync()
        {
            return await _context.Reminders.Include(r => r.Vehicle).ToListAsync();
        }
    }
}
