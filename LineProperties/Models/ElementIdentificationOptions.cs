namespace LineProperties.Models;

/// <summary>
/// Shared options for identifying joists, deck, walls, columns by layer name.
/// Used by BOM and commands.
/// </summary>
public static class ElementIdentificationOptions
{
    /// <summary>Layer name must contain this substring (case-insensitive) to be treated as joist. Entity must be Line.</summary>
    public const string JoistLayerSubstring = "JOISTS";

    /// <summary>Layer name must contain this substring (case-insensitive) to be treated as deck.</summary>
    public const string DeckLayerSubstring = "DECK";

    /// <summary>Layer for walls (white).</summary>
    public const string WallLayerSubstring = "WALLS";

    /// <summary>Layer for columns (yellow).</summary>
    public const string ColumnLayerSubstring = "COLUMNS";

    /// <summary>Legacy: NOSNIK treated as joist for backward compatibility.</summary>
    public const string BeamLayerSubstring = "NOSNIK";

    /// <summary>Legacy: PLECH treated as deck for backward compatibility.</summary>
    public const string PlateLayerSubstring = "PLECH";

    /// <summary>Distance tolerance for "touch" when detecting connections (in drawing units).</summary>
    public const double ConnectionTolerance = 0.01;

    public static bool IsJoistLayer(string layerName) =>
        !string.IsNullOrEmpty(layerName) &&
        (layerName.Contains(JoistLayerSubstring, StringComparison.OrdinalIgnoreCase) ||
         layerName.Contains(BeamLayerSubstring, StringComparison.OrdinalIgnoreCase));

    public static bool IsDeckLayer(string layerName) =>
        !string.IsNullOrEmpty(layerName) &&
        (layerName.Contains(DeckLayerSubstring, StringComparison.OrdinalIgnoreCase) ||
         layerName.Contains(PlateLayerSubstring, StringComparison.OrdinalIgnoreCase));

    [Obsolete("Use IsJoistLayer")]
    public static bool IsBeamLayer(string layerName) => IsJoistLayer(layerName);

    [Obsolete("Use IsDeckLayer")]
    public static bool IsPlateLayer(string layerName) => IsDeckLayer(layerName);
}
