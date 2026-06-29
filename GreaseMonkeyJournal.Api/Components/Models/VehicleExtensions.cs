namespace GreaseMonkeyJournal.Api.Components.Models;

/// <summary>
/// Display helpers for <see cref="Vehicle"/> that centralize the "which identifier applies"
/// rule: Tractor Implements are identified by their serial number, all other vehicle types by
/// their registration. Keeping this in one place avoids inconsistent rendering across pages.
/// </summary>
public static class VehicleExtensions
{
    /// <summary>
    /// Returns the identifier relevant to the vehicle's type: the serial number for
    /// <see cref="VehicleType.TractorImplement"/>, otherwise the registration. May be null/empty.
    /// </summary>
    public static string? GetIdentifier(this Vehicle vehicle) =>
        vehicle.VehicleType == VehicleType.TractorImplement ? vehicle.SerialNumber : vehicle.Registration;

    /// <summary>
    /// Returns a display string for the vehicle's identifier, or "N/A" when none is set.
    /// Serial numbers are prefixed with "S/N " to distinguish them from registrations.
    /// </summary>
    public static string GetIdentifierDisplay(this Vehicle vehicle)
    {
        var identifier = vehicle.GetIdentifier();
        if (string.IsNullOrWhiteSpace(identifier))
            return "N/A";

        return vehicle.VehicleType == VehicleType.TractorImplement ? $"S/N {identifier}" : identifier;
    }

    /// <summary>
    /// Returns the identifier wrapped in parentheses for inline display next to a vehicle name
    /// (e.g. "Toyota Corolla (ABC123)"), or an empty string when no identifier is set.
    /// </summary>
    public static string GetIdentifierParenthetical(this Vehicle vehicle)
    {
        var identifier = vehicle.GetIdentifier();
        return string.IsNullOrWhiteSpace(identifier) ? string.Empty : $"({identifier})";
    }
}
