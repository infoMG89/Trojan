using System.Linq;

namespace LineProperties.Data;

/// <summary>
/// Metal deck record: type, gauge, profile depth (in), rib spacing (in) for drawing.
/// </summary>
public record DeckRecord(string DeckType, int Gauge, double ProfileDepthInches, double RibSpacingInches);

/// <summary>
/// Metal deck catalog: B-Deck, C-Deck, N-Deck; gauges 18, 20, 22; profiles 1"-3".
/// </summary>
public static class DeckLibrary
{
    private static readonly DeckRecord[] Decks =
    {
        new("B-Deck 1.5\" 18ga", 18, 1.5, 6),
        new("B-Deck 1.5\" 20ga", 20, 1.5, 6),
        new("B-Deck 1.5\" 22ga", 22, 1.5, 6),
        new("B-Deck 2\" 18ga", 18, 2.0, 6),
        new("B-Deck 2\" 20ga", 20, 2.0, 6),
        new("B-Deck 2\" 22ga", 22, 2.0, 6),
        new("B-Deck 3\" 18ga", 18, 3.0, 6),
        new("B-Deck 3\" 20ga", 20, 3.0, 6),
        new("B-Deck 3\" 22ga", 22, 3.0, 6),
        new("C-Deck 1.5\" 18ga", 18, 1.5, 12),
        new("C-Deck 1.5\" 20ga", 20, 1.5, 12),
        new("C-Deck 1.5\" 22ga", 22, 1.5, 12),
        new("C-Deck 2\" 18ga", 18, 2.0, 12),
        new("C-Deck 2\" 20ga", 20, 2.0, 12),
        new("C-Deck 2\" 22ga", 22, 2.0, 12),
        new("C-Deck 3\" 18ga", 18, 3.0, 12),
        new("C-Deck 3\" 20ga", 20, 3.0, 12),
        new("C-Deck 3\" 22ga", 22, 3.0, 12),
        new("N-Deck 1\" 18ga", 18, 1.0, 6),
        new("N-Deck 1\" 20ga", 20, 1.0, 6),
        new("N-Deck 1\" 22ga", 22, 1.0, 6),
        new("N-Deck 1.5\" 18ga", 18, 1.5, 6),
        new("N-Deck 1.5\" 20ga", 20, 1.5, 6),
        new("N-Deck 1.5\" 22ga", 22, 1.5, 6),
        new("N-Deck 2\" 18ga", 18, 2.0, 6),
        new("N-Deck 2\" 20ga", 20, 2.0, 6),
        new("N-Deck 2\" 22ga", 22, 2.0, 6)
    };

    public const string DefaultDeckType = "B-Deck 1.5\" 18ga";

    public static IEnumerable<string> GetDeckTypes() => Decks.Select(d => d.DeckType);

    public static DeckRecord? GetByDeckType(string? deckType)
    {
        if (string.IsNullOrEmpty(deckType)) return null;
        return Decks.FirstOrDefault(d =>
            string.Equals(d.DeckType, deckType, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Rib spacing in drawing units (inches) - used for SMARTPATTERN line spacing.</summary>
    public static double GetRibSpacing(string? deckType) => GetByDeckType(deckType)?.RibSpacingInches ?? 6.0;
}
