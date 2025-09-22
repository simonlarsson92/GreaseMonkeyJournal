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

    // Navigation property for reminders
    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
}
