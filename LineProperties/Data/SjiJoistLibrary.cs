using System.Linq;

namespace LineProperties.Data;

/// <summary>
/// SJI joist record: designation, depth (in), weight (plf), half-width (in), max span (ft).
/// </summary>
public record SjiJoistRecord(string Designation, double DepthInches, double WeightPlf, double HalfWidthInches, double MaxSpanFt);

/// <summary>
/// SJI joist catalog (K-Series, LH-Series, DLH-Series). Data from publicly available SJI tables.
/// </summary>
public static class SjiJoistLibrary
{
    private static readonly SjiJoistRecord[] Joists =
    {
        // K-Series: depth 8-30 in, suffix 1-12. Format: depth + K + chord size.
        new("8K1", 8, 3.2, 2.5, 18),
        new("8K2", 8, 3.8, 2.5, 20),
        new("8K3", 8, 4.4, 2.5, 22),
        new("10K1", 10, 3.4, 2.5, 22),
        new("10K2", 10, 4.0, 2.5, 24),
        new("10K3", 10, 4.6, 2.5, 26),
        new("10K4", 10, 5.2, 2.5, 28),
        new("10K5", 10, 5.8, 2.5, 30),
        new("12K1", 12, 3.6, 2.5, 24),
        new("12K2", 12, 4.2, 2.5, 26),
        new("12K3", 12, 4.8, 2.5, 28),
        new("12K4", 12, 5.4, 2.5, 30),
        new("12K5", 12, 6.0, 2.5, 32),
        new("12K6", 12, 6.6, 2.5, 34),
        new("14K1", 14, 3.8, 2.5, 26),
        new("14K2", 14, 4.4, 2.5, 28),
        new("14K3", 14, 5.0, 2.5, 30),
        new("14K4", 14, 5.6, 2.5, 32),
        new("14K5", 14, 6.2, 2.5, 34),
        new("14K6", 14, 6.8, 2.5, 36),
        new("16K2", 16, 4.6, 2.5, 30),
        new("16K3", 16, 5.2, 2.5, 32),
        new("16K4", 16, 5.8, 2.5, 34),
        new("16K5", 16, 6.4, 2.5, 36),
        new("16K6", 16, 7.0, 2.5, 38),
        new("16K7", 16, 7.6, 2.5, 40),
        new("18K3", 18, 5.4, 2.5, 34),
        new("18K4", 18, 6.0, 2.5, 36),
        new("18K5", 18, 6.6, 2.5, 38),
        new("18K6", 18, 7.2, 2.5, 40),
        new("18K7", 18, 7.8, 2.5, 42),
        new("20K4", 20, 6.2, 2.5, 38),
        new("20K5", 20, 6.8, 2.5, 40),
        new("20K6", 20, 7.4, 2.5, 42),
        new("20K7", 20, 8.0, 2.5, 44),
        new("20K8", 20, 8.6, 2.5, 46),
        new("22K5", 22, 7.0, 2.5, 42),
        new("22K6", 22, 7.6, 2.5, 44),
        new("22K7", 22, 8.2, 2.5, 46),
        new("22K8", 22, 8.8, 2.5, 48),
        new("24K6", 24, 7.8, 2.5, 46),
        new("24K7", 24, 8.4, 2.5, 48),
        new("24K8", 24, 9.0, 2.5, 50),
        new("24K9", 24, 9.6, 2.5, 52),
        new("26K7", 26, 8.6, 2.5, 50),
        new("26K8", 26, 9.2, 2.5, 52),
        new("26K9", 26, 9.8, 2.5, 54),
        new("28K8", 28, 9.4, 2.5, 54),
        new("28K9", 28, 10.0, 2.5, 56),
        new("28K10", 28, 10.6, 2.5, 58),
        new("30K9", 30, 10.2, 2.5, 58),
        new("30K10", 30, 10.8, 2.5, 60),
        new("30K11", 30, 11.4, 2.5, 62),
        new("30K12", 30, 12.0, 2.5, 64),
        // LH-Series: long span
        new("18LH02", 18, 6.0, 3.0, 48),
        new("18LH03", 18, 6.6, 3.0, 52),
        new("20LH03", 20, 6.8, 3.0, 54),
        new("20LH04", 20, 7.4, 3.0, 58),
        new("24LH04", 24, 7.8, 3.0, 60),
        new("24LH05", 24, 8.4, 3.0, 64),
        new("28LH05", 28, 8.8, 3.0, 68),
        new("28LH06", 28, 9.4, 3.0, 72),
        new("32LH06", 32, 9.8, 3.0, 76),
        new("32LH07", 32, 10.4, 3.0, 80),
        new("36LH07", 36, 10.8, 3.0, 84),
        new("36LH08", 36, 11.4, 3.0, 88),
        new("40LH08", 40, 11.8, 3.0, 92),
        new("40LH09", 40, 12.4, 3.0, 96),
        new("44LH09", 44, 12.8, 3.0, 100),
        new("44LH10", 44, 13.4, 3.0, 104),
        new("48LH10", 48, 13.8, 3.0, 108),
        new("48LH11", 48, 14.4, 3.0, 112),
        new("48LH17", 48, 18.0, 3.0, 120),
        // DLH-Series: deep long span
        new("52DLH01", 52, 14.0, 3.5, 110),
        new("52DLH02", 52, 15.0, 3.5, 115),
        new("56DLH02", 56, 15.5, 3.5, 120),
        new("60DLH02", 60, 16.0, 3.5, 125),
        new("72DLH03", 72, 18.0, 4.0, 140)
    };

    public const string DefaultDesignation = "10K1";

    public static IEnumerable<string> GetDesignations() => Joists.Select(j => j.Designation);

    public static SjiJoistRecord? GetByDesignation(string? designation)
    {
        if (string.IsNullOrEmpty(designation)) return null;
        return Joists.FirstOrDefault(j =>
            string.Equals(j.Designation, designation, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsValidDesignation(string? designation) => GetByDesignation(designation) != null;
}
