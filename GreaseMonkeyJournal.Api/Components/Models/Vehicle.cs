using System.Collections.Generic;

namespace GreaseMonkeyJournal.Api.Components.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Registration { get; set; } = string.Empty;
    public SpeedometerType SpeedometerType { get; set; } = SpeedometerType.None;

    // Categorizes the vehicle/machine; drives conditional UI (e.g. registration vs. serial number)
    public VehicleType VehicleType { get; set; } = VehicleType.Car;

    // Optional free-text description of the vehicle/machine
    public string? Description { get; set; }

    // Optional serial number, used for Tractor Implements which have no registration
    public string? SerialNumber { get; set; }

    // Navigation property for reminders
    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();

    // Navigation property for information cards
    public ICollection<VehicleInfoCard> InfoCards { get; set; } = new List<VehicleInfoCard>();
}
