namespace GreaseMonkeyJournal.Api.Components.Models;

/// <summary>
/// Categorizes a vehicle or machine so the UI can adapt (for example, hiding the
/// registration field for implements that have no registration of their own).
/// </summary>
public enum VehicleType
{
    Car = 0,
    Truck = 1,
    Tractor = 2,
    TractorImplement = 3,
    Lawnmower = 4,
    Trailer = 5,
    Atv = 6,
    Implement = 7,
    Other = 8
}

/// <summary>
/// Helper methods for presenting <see cref="VehicleType"/> values in the UI.
/// </summary>
public static class VehicleTypeExtensions
{
    /// <summary>
    /// Returns a human-friendly display name for a <see cref="VehicleType"/> value.
    /// </summary>
    public static string ToDisplayName(this VehicleType type) => type switch
    {
        VehicleType.TractorImplement => "Tractor Implement",
        VehicleType.Atv => "ATV / UTV",
        VehicleType.Implement => "Farm Implement",
        _ => type.ToString()
    };

    /// <summary>
    /// Returns true for vehicle types identified by serial number rather than registration plate.
    /// </summary>
    public static bool UsesSerialNumber(this VehicleType type) => type switch
    {
        VehicleType.TractorImplement => true,
        VehicleType.Implement => true,
        VehicleType.Lawnmower => true,
        _ => false
    };
}
