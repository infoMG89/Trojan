using System.Globalization;
using System.Text.RegularExpressions;

namespace LineProperties.Services;

/// <summary>
/// Parses fractional inch notation (e.g. 4", 1/4", 3-1/2") to decimal inches.
/// </summary>
public static class FractionalInchParser
{
    private static readonly Regex MixedRegex = new(
        @"^(?<whole>-?\d+)\s*-\s*(?<num>\d+)/(?<den>\d+)\s*""?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex FractionOnlyRegex = new(
        @"^(?<num>\d+)/(?<den>\d+)\s*""?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>Parses a fractional inch string to decimal. Returns null if parsing fails.</summary>
    public static double? Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return 0.0;
        input = input.Trim().TrimEnd('"');

        if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var direct))
            return direct;

        var m = MixedRegex.Match(input);
        if (m.Success)
        {
            int whole = int.Parse(m.Groups["whole"].Value, CultureInfo.InvariantCulture);
            int num = int.Parse(m.Groups["num"].Value, CultureInfo.InvariantCulture);
            int den = int.Parse(m.Groups["den"].Value, CultureInfo.InvariantCulture);
            if (den <= 0) return null;
            return whole + (num / (double)den);
        }

        m = FractionOnlyRegex.Match(input);
        if (m.Success)
        {
            int num = int.Parse(m.Groups["num"].Value, CultureInfo.InvariantCulture);
            int den = int.Parse(m.Groups["den"].Value, CultureInfo.InvariantCulture);
            if (den <= 0) return null;
            return num / (double)den;
        }

        if (Regex.IsMatch(input, @"^-?\d+\s*""?$"))
            return int.Parse(Regex.Match(input, @"-?\d+").Value, CultureInfo.InvariantCulture);

        return null;
    }

    /// <summary>Parses or returns defaultValue if parsing fails.</summary>
    public static double ParseOr(string? input, double defaultValue)
    {
        var r = Parse(input);
        return r ?? defaultValue;
    }

    /// <summary>Formats a decimal value to fractional inch string (e.g. 4-1/4" for 4.25).</summary>
    public static string Format(double value)
    {
        if (Math.Abs(value) < 1e-9) return "0\"";
        int whole = (int)Math.Truncate(value);
        double frac = Math.Abs(value - whole);
        if (frac < 1e-9) return $"{whole}\"";
        int den = 16;
        int num = (int)Math.Round(frac * den);
        if (num == den) { whole += Math.Sign(value); num = 0; }
        if (num == 0) return $"{whole}\"";
        while (num > 0 && num % 2 == 0 && den % 2 == 0) { num /= 2; den /= 2; }
        return whole != 0 ? $"{whole}-{num}/{den}\"" : $"{num}/{den}\"";
    }
}
