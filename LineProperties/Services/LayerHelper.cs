using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;

namespace LineProperties.Services;

public static class LayerHelper
{
    public const string JoistsLayer = "JOISTS";
    public const string DeckLayer = "DECK";
    public const string BeamsLayer = "BEAMS";
    public const string PlatesLayer = "PLATES";
    public const string WallsLayer = "WALLS";
    public const string ColumnsLayer = "COLUMNS";

    /// <summary>Ensures a layer exists; creates it if not. colorIndex = ACI (1=red, 3=green).</summary>
    public static void EnsureLayer(Database db, Transaction tr, string layerName, short? colorIndex = null)
    {
        if (string.IsNullOrEmpty(layerName)) return;
        var lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
        if (lt.Has(layerName)) return;
        lt.UpgradeOpen();
        using var ltr = new LayerTableRecord();
        ltr.Name = layerName;
        if (colorIndex.HasValue)
            ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, colorIndex.Value);
        lt.Add(ltr);
        tr.AddNewlyCreatedDBObject(ltr, true);
    }

    public static void EnsureJoistsLayer(Database db, Transaction tr) => EnsureLayer(db, tr, JoistsLayer, 1);
    public static void EnsureDeckLayer(Database db, Transaction tr) => EnsureLayer(db, tr, DeckLayer, 3);
    public static void EnsureBeamsLayer(Database db, Transaction tr) => EnsureLayer(db, tr, BeamsLayer, 4); // cyan
    public static void EnsurePlatesLayer(Database db, Transaction tr) => EnsureLayer(db, tr, PlatesLayer, 2); // yellow
}
