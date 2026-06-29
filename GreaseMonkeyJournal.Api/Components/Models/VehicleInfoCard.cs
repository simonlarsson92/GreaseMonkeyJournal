using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreaseMonkeyJournal.Api.Components.Models;

/// <summary>
/// A free-form information card attached to a <see cref="Vehicle"/>. Each card captures a
/// titled note about the vehicle or machine, such as a tedder's rake article number or the
/// engine oil specification for a car.
/// </summary>
public class VehicleInfoCard
{
    /// <summary>Database identifier for the card.</summary>
    public int Id { get; set; }

    /// <summary>Identifier of the owning vehicle.</summary>
    [Required]
    public int VehicleId { get; set; }

    /// <summary>Short heading shown on the card (e.g. "Rake", "Engine oil").</summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>Longer free-form notes for the card (e.g. "Article #12345", "5W-30, 4.5L").</summary>
    public string? Details { get; set; }

    /// <summary>Navigation property for the related vehicle.</summary>
    [ForeignKey(nameof(VehicleId))]
    public Vehicle? Vehicle { get; set; }
}
