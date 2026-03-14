using System.Collections.Generic;
using System.Linq;

namespace LineProperties.Data;

/// <summary>
/// Steel plate record: designation, thickness (mm).
/// </summary>
public record PlateRecord(string Designation, double ThicknessMm);

/// <summary>
/// Steel plate catalog for SMARTDECK (obdélníkové plechy).
/// </summary>
public static class PlateLibrary
{
    private static readonly PlateRecord[] Plates =
    {
        new("PL 6", 6),
        new("PL 8", 8),
        new("PL 10", 10),
        new("PL 12", 12),
        new("PL 15", 15),
        new("PL 20", 20),
        new("PL 25", 25),
        new("PL 30", 30),
    };

    public const string DefaultPlateType = "PL 10";

    public static IEnumerable<string> GetPlateTypes() => Plates.Select(p => p.Designation);

    public static PlateRecord? GetByDesignation(string? designation)
    {
        if (string.IsNullOrEmpty(designation)) return null;
        return Plates.FirstOrDefault(p =>
            string.Equals(p.Designation, designation, System.StringComparison.OrdinalIgnoreCase));
    }
}
