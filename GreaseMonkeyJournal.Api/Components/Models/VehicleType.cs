namespace GreaseMonkeyJournal.Api.Components.Models;

/// <summary>
/// Categorizes a vehicle or machine so the UI can adapt (for example, hiding the
/// registration field for towed implements that have no registration of their own).
/// </summary>
public enum VehicleType
{
    Car = 0,
    Truck = 1,
    Tractor = 2,
    TractorImplement = 3
}

/// <summary>
/// Helper methods for presenting <see cref="VehicleType"/> values in the UI.
/// </summary>
public static class VehicleTypeExtensions
{
    /// <summary>
    /// Returns a human-friendly display name for a <see cref="VehicleType"/> value,
    /// inserting spaces for multi-word names such as "Tractor Implement".
    /// </summary>
    /// <param name="type">The vehicle type to convert.</param>
    /// <returns>A display-friendly name for the supplied type.</returns>
    public static string ToDisplayName(this VehicleType type) => type switch
    {
        VehicleType.TractorImplement => "Tractor Implement",
        _ => type.ToString()
    };
}
