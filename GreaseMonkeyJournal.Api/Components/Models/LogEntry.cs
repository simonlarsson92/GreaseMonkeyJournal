using System.ComponentModel.DataAnnotations.Schema;

namespace GreaseMonkeyJournal.Api.Components.Models;

public class LogEntry
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public DateTime? Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string Type { get; set; } = string.Empty; // e.g., Repair, Maintenance
    public string? Notes { get; set; }
    public decimal? SpeedometerReading { get; set; } // Reading in KM or Hours based on vehicle's speedometer type
    public Vehicle? Vehicle { get; set; } // Navigation property for related vehicle
}

