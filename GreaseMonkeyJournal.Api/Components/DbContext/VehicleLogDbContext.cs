using Microsoft.EntityFrameworkCore;
using GreaseMonkeyJournal.Api.Components.Models;

namespace GreaseMonkeyJournal.Api.Components.DbContext;

public class VehicleLogDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public VehicleLogDbContext(DbContextOptions<VehicleLogDbContext> options) : base(options) { }
    public DbSet<Vehicle> Vehicles { get; set; } = default!;
    public DbSet<LogEntry> LogEntries { get; set; } = default!;
    public DbSet<Reminder> Reminders { get; set; } = default!;
    public DbSet<NotificationSettings> NotificationSettings { get; set; } = default!;
    public DbSet<VehicleInfoCard> VehicleInfoCards { get; set; } = default!;
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
        
        // NotificationSettings configuration
        modelBuilder.Entity<NotificationSettings>().HasKey(n => n.Id);
        modelBuilder.Entity<NotificationSettings>()
            .HasIndex(n => n.UserEmail)
            .IsUnique();

        // VehicleInfoCard relationship
        modelBuilder.Entity<VehicleInfoCard>().HasKey(c => c.Id);
        modelBuilder.Entity<VehicleInfoCard>()
            .HasOne(c => c.Vehicle)
            .WithMany(v => v.InfoCards)
            .HasForeignKey(c => c.VehicleId);
    }
}
