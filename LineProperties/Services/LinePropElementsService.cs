using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace LineProperties.Services;

/// <summary>Element with MORAVIO_SMART XData for display in JOISTLIST.</summary>
public record SmartElement(string Mark, string MemberType, string Designation, string Layer, string Dimensions, string Handle);

/// <summary>Collects entities with MORAVIO_SMART XData.</summary>
public static class LinePropElementsService
{
    public static List<SmartElement> CollectElements(Database db, Transaction tr)
    {
        var result = new List<SmartElement>();

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

            result.Add(new SmartElement(
                data.Mark ?? "",
                data.MemberType ?? "",
                data.Designation ?? "",
                ent.Layer ?? "",
                dims,
                ent.Handle.ToString()));
        }

        result.Sort((a, b) =>
        {
            int tc = string.Compare(a.MemberType, b.MemberType, StringComparison.OrdinalIgnoreCase);
            if (tc != 0) return tc;
            return string.Compare(a.Mark, b.Mark, StringComparison.OrdinalIgnoreCase);
        });
        return result;
    }
}
