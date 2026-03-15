using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace LineProperties.Helpers;

public static class LogoHelper
{
    /// <summary>Returns Trojan Steel logo as BitmapImage, or null if not found.</summary>
    public static BitmapImage? GetTrojanLogoSource()
    {
        return LoadLogo("trojan_steel_llc_logo.jpg");
    }

    /// <summary>Returns HMR logo as BitmapImage, or null if not found.</summary>
    public static BitmapImage? GetHmrLogoSource()
    {
        return LoadLogo("1656059722254.jpg");
    }

    /// <summary>Legacy: returns Trojan logo (for backward compatibility).</summary>
    public static BitmapImage? GetLogoSource() => GetTrojanLogoSource();

    private static BitmapImage? LoadLogo(string fileName)
    {
        var path = FindLogoPath(fileName);
        if (path == null) return null;
        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(path, UriKind.Absolute);
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch { return null; }
    }

    private static string? FindLogoPath(string fileName)
    {
        var dirs = new[]
        {
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            AppDomain.CurrentDomain.BaseDirectory
        };
        foreach (var dir in dirs)
        {
            if (string.IsNullOrEmpty(dir)) continue;
            var path = Path.Combine(dir, "Logo", fileName);
            if (File.Exists(path)) return path;
        }
        return null;
    }
}
