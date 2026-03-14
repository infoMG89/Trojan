using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using LineProperties.Models;

namespace LineProperties.Services;

public record BomRow(
    string Mark,
    string Type,
    string Designation,
    string Dimensions,
    double ExtensionLeft,
    double ExtensionRight,
    double Depth,
    double Weight,
    int Count);

public static class BomService
{
    /// <summary>Scans modelspace for entities with MORAVIO_SMART XData (joists, deck). Aggregates by type, designation, dimensions.</summary>
    public static List<BomRow> CollectBomRows(Database db, Transaction tr)
    {
        var aggregator = new Dictionary<(string Type, string Designation, string Dimensions, double ExtL, double ExtR, double Depth, double Weight), (string Marks, int Count)>();

        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

        foreach (ObjectId id in btr)
        {
            if (!id.IsValid) continue;
            var ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
            if (ent == null) continue;

            if (!MoravioSmartXDataService.HasMoravioSmartData(ent)) continue;

            var data = MoravioSmartXDataService.Read(ent);
            if (data == null) continue;

            string dims = ent is Line line ? line.Length.ToString("F2") : "0";
            if (data.SpanLength > 0) dims = data.SpanLength.ToString("F2");

            var key = (data.MemberType, data.Designation ?? "", dims, data.ExtensionLeft, data.ExtensionRight, data.Depth, data.Weight);
            if (!aggregator.TryGetValue(key, out var existing))
                aggregator[key] = (data.Mark ?? "", 1);
            else
                aggregator[key] = (existing.Marks + ", " + (data.Mark ?? ""), existing.Count + 1);
        }

        var rows = new List<BomRow>();
        foreach (var kv in aggregator)
        {
            var (type, designation, dimensions, extL, extR, depth, weight) = kv.Key;
            var (marks, count) = kv.Value;
            rows.Add(new BomRow(marks, type, designation, dimensions, extL, extR, depth, weight, count));
        }

        rows.Sort((a, b) =>
        {
            int tc = string.Compare(a.Type, b.Type, StringComparison.OrdinalIgnoreCase);
            if (tc != 0) return tc;
            int dc = string.Compare(a.Designation, b.Designation, StringComparison.OrdinalIgnoreCase);
            if (dc != 0) return dc;
            return string.Compare(a.Dimensions, b.Dimensions, StringComparison.Ordinal);
        });
        return rows;
    }
}
