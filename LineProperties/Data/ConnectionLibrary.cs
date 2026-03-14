using System.Collections.Generic;
using LineProperties.Services;

namespace LineProperties.Data;

public static class ConnectionLibrary
{
    public static IReadOnlyList<string> GetCodesForType(ConnectionType type)
    {
        return type switch
        {
            ConnectionType.A => new[] { "A1", "A2", "A15", "A3", "A4" },
            ConnectionType.B => new[] { "B1", "B2", "B3", "B7", "B4" },
            ConnectionType.C => new[] { "C1", "C2", "C3", "C7", "C4" },
            _ => new[] { "A1" }
        };
    }

    public static string GetDefaultCode(ConnectionType type)
    {
        return type switch
        {
            ConnectionType.A => "A1",
            ConnectionType.B => "B3",
            ConnectionType.C => "C1",
            _ => "A1"
        };
    }

    public static string? GetDescription(string code)
    {
        return code switch
        {
            "A1" => "Svařované tupo",
            "A2" => "Svařované na tupo s výztuhou",
            "A15" => "Šroubované M12",
            "A3" => "Lepené",
            "A4" => "Jiné (plech–plech)",
            "B1" => "Svařované koutové",
            "B2" => "Svařované čelně",
            "B3" => "Šroubované přes styčník",
            "B7" => "Nýtované",
            "B4" => "Jiné (nosník–plech)",
            "C1" => "Styčník šroubovaný",
            "C2" => "Čepový spoj",
            "C3" => "Překryv svařovaný",
            "C7" => "Svařovaný styčník",
            "C4" => "Jiné (nosník–nosník)",
            _ => null
        };
    }
}
