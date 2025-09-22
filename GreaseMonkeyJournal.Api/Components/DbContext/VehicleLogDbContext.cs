using Microsoft.EntityFrameworkCore;
using GreaseMonkeyJournal.Api.Components.Models;

namespace GreaseMonkeyJournal.Api.Components.DbContext;

public class VehicleLogDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public VehicleLogDbContext(DbContextOptions<VehicleLogDbContext> options) : base(options) { }
    public DbSet<Vehicle> Vehicles { get; set; } = default!;
    public DbSet<LogEntry> LogEntries { get; set; } = default!;
    public DbSet<Reminder> Reminders { get; set; } = default!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehicle>().HasKey(v => v.Id);
        modelBuilder.Entity<LogEntry>().HasKey(l => l.Id);
        modelBuilder.Entity<LogEntry>()
            .HasOne(l => l.Vehicle)
            .WithMany()
            .HasForeignKey(l => l.VehicleId);
        // Reminder relationship
        modelBuilder.Entity<Reminder>().HasKey(r => r.Id);
        modelBuilder.Entity<Reminder>()
            .HasOne(r => r.Vehicle)
            .WithMany(v => v.Reminders)
            .HasForeignKey(r => r.VehicleId);
    }
}
