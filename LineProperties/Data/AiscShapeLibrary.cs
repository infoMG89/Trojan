using System.Collections.Generic;
using System.Linq;

namespace LineProperties.Data;

/// <summary>
/// AISC 14th Edition steel shape record: designation, type, dimensions (in), weight (lb/ft).
/// Data from AISC Steel Construction Manual 14th/15th Edition.
/// </summary>
public record AiscShapeRecord(string Designation, string ShapeType, double DepthInches, double FlangeWidthInches,
    double WebThicknessInches, double FlangeThicknessInches, double WeightPlf);

/// <summary>
/// AISC 14th Edition steel shapes: W-shapes, C-channels. Subset of common sizes.
/// </summary>
public static class AiscShapeLibrary
{
    private static readonly AiscShapeRecord[] Shapes =
    {
        // W-shapes (Wide Flange) - common sizes
        new("W44X335", "W", 44, 15.9, 1.03, 1.77, 335),
        new("W40X324", "W", 40.2, 15.9, 1.0, 1.81, 324),
        new("W36X330", "W", 37.7, 16.6, 1.02, 1.85, 330),
        new("W36X256", "W", 37.4, 12.2, 0.96, 1.73, 256),
        new("W36X135", "W", 35.6, 12, 0.6, 0.79, 135),
        new("W33X201", "W", 33.7, 15.7, 0.715, 1.15, 201),
        new("W33X130", "W", 33.1, 11.5, 0.58, 0.855, 130),
        new("W30X191", "W", 30.7, 15, 0.71, 1.19, 191),
        new("W30X124", "W", 30.2, 10.5, 0.585, 0.93, 124),
        new("W27X146", "W", 27.6, 14, 0.605, 1.02, 146),
        new("W27X94", "W", 26.9, 10, 0.49, 0.745, 94),
        new("W24X104", "W", 24.1, 12.8, 0.5, 0.96, 104),
        new("W24X68", "W", 23.7, 8.97, 0.415, 0.585, 68),
        new("W21X62", "W", 21, 8.24, 0.4, 0.615, 62),
        new("W21X44", "W", 20.66, 6.5, 0.35, 0.45, 44),
        new("W18X76", "W", 18.2, 11, 0.425, 0.68, 76),
        new("W18X35", "W", 17.7, 6, 0.3, 0.425, 35),
        new("W16X67", "W", 16.3, 10.2, 0.395, 0.665, 67),
        new("W16X26", "W", 15.69, 5.5, 0.25, 0.345, 26),
        new("W14X90", "W", 14, 14.5, 0.44, 0.71, 90),
        new("W14X38", "W", 14.1, 6.77, 0.31, 0.515, 38),
        new("W14X22", "W", 13.74, 5, 0.23, 0.335, 22),
        new("W12X65", "W", 12.1, 12, 0.39, 0.605, 65),
        new("W12X26", "W", 12.22, 6.49, 0.23, 0.38, 26),
        new("W10X49", "W", 9.98, 10, 0.34, 0.56, 49),
        new("W10X22", "W", 10.17, 5.75, 0.24, 0.36, 22),
        new("W8X31", "W", 8, 7.95, 0.285, 0.435, 31),
        new("W8X18", "W", 8.14, 5.25, 0.23, 0.33, 18),
        new("W6X25", "W", 6.38, 6.08, 0.32, 0.455, 25),
        new("W6X15", "W", 5.99, 5.99, 0.23, 0.26, 15),
        new("W5X19", "W", 5.03, 5, 0.27, 0.395, 19),
        new("W4X13", "W", 4.16, 4.06, 0.28, 0.345, 13),
        // C-channels (American Standard Channels)
        new("C15X50", "C", 15, 3.72, 0.716, 0.65, 50),
        new("C12X30", "C", 12, 3.17, 0.51, 0.501, 30),
        new("C12X25", "C", 12, 3.05, 0.387, 0.501, 25),
        new("C12X20.7", "C", 12, 2.94, 0.282, 0.501, 20.7),
        new("C10X30", "C", 10, 3.03, 0.436, 0.436, 30),
        new("C10X20", "C", 10, 2.74, 0.379, 0.436, 20),
        new("C10X15.3", "C", 10, 2.6, 0.24, 0.436, 15.3),
        new("C9X20", "C", 9, 2.65, 0.448, 0.413, 20),
        new("C9X15", "C", 9, 2.49, 0.285, 0.413, 15),
        new("C8X18.75", "C", 8, 2.53, 0.487, 0.39, 18.75),
        new("C8X13.75", "C", 8, 2.34, 0.303, 0.39, 13.75),
        new("C8X11.5", "C", 8, 2.26, 0.22, 0.39, 11.5),
        new("C7X14.75", "C", 7, 2.3, 0.366, 0.366, 14.75),
        new("C7X12.25", "C", 7, 2.19, 0.282, 0.366, 12.25),
        new("C6X13", "C", 6, 2.16, 0.437, 0.343, 13),
        new("C6X10.5", "C", 6, 2.03, 0.314, 0.343, 10.5),
        new("C6X8.2", "C", 6, 1.92, 0.2, 0.343, 8.2),
        new("C5X9", "C", 5, 1.89, 0.325, 0.32, 9),
        new("C5X6.7", "C", 5, 1.75, 0.19, 0.32, 6.7),
        new("C4X7.25", "C", 4, 1.72, 0.321, 0.296, 7.25),
        new("C4X5.4", "C", 4, 1.58, 0.184, 0.296, 5.4),
        new("C3X6", "C", 3, 1.6, 0.356, 0.273, 6),
        new("C3X5", "C", 3, 1.49, 0.258, 0.273, 5),
    };

    public const string DefaultWShape = "W14X22";
    public const string DefaultCShape = "C12X25";

    public static IEnumerable<string> GetShapeTypes() => new[] { "W", "C" };

    public static IEnumerable<string> GetDesignations(string? shapeType)
    {
        if (string.IsNullOrEmpty(shapeType))
            return Shapes.Select(s => s.Designation);
        return Shapes.Where(s => string.Equals(s.ShapeType, shapeType, System.StringComparison.OrdinalIgnoreCase))
            .Select(s => s.Designation);
    }

    public static AiscShapeRecord? GetByDesignation(string? designation)
    {
        if (string.IsNullOrEmpty(designation)) return null;
        return Shapes.FirstOrDefault(s =>
            string.Equals(s.Designation, designation, System.StringComparison.OrdinalIgnoreCase));
    }

    public static string GetDefaultDesignation(string? shapeType) =>
        shapeType?.ToUpperInvariant() == "C" ? DefaultCShape : DefaultWShape;
}
