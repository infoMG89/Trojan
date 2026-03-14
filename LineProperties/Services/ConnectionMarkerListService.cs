using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace LineProperties.Services;

public record ConnectionMarkerInfo(string Handle1, string Handle2, ConnectionType Type, string ConnectionId, string? ChosenCode);

/// <summary>Connection info for display in CONNECTION_LIST popup (with element Mark/Designation).</summary>
public record ConnectionListDisplayItem(string ConnectionId, string Type, string Code, string Element1, string Element2);

public static class ConnectionMarkerListService
{
    public static List<ConnectionMarkerInfo> CollectMarkers(Database db)
    {
        var result = new List<ConnectionMarkerInfo>();
        using var tr = db.TransactionManager.StartTransaction();
        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

        foreach (ObjectId id in btr)
        {
            if (tr.GetObject(id, OpenMode.ForRead) is not Entity ent) continue;
            var data = ConnectionMarkerService.GetMarkerData(ent);
            if (data == null) continue;

            var code = ConnectionMarkerService.GetChosenCode(ent);
            result.Add(new ConnectionMarkerInfo(
                data.Value.Handle1,
                data.Value.Handle2,
                data.Value.Type,
                data.Value.ConnectionId,
                code));
        }
        tr.Commit();
        return result;
    }

    public static List<ConnectionListDisplayItem> CollectMarkersWithEntityInfo(Database db)
    {
        var markers = CollectMarkers(db);
        var result = new List<ConnectionListDisplayItem>();
        using var tr = db.TransactionManager.StartTransaction();
        foreach (var m in markers)
        {
            var info1 = GetEntityDisplayInfo(db, tr, m.Handle1);
            var info2 = GetEntityDisplayInfo(db, tr, m.Handle2);
            result.Add(new ConnectionListDisplayItem(
                m.ConnectionId,
                m.Type.ToString(),
                m.ChosenCode ?? "",
                info1,
                info2));
        }
        tr.Commit();
        return result;
    }

    private static string GetEntityDisplayInfo(Database db, Transaction tr, string handleStr)
    {
        var id = ConnectionMarkerService.GetObjectIdByHandleString(db, handleStr);
        if (!id.HasValue || !id.Value.IsValid) return $"(handle {handleStr})";
        var ent = tr.GetObject(id.Value, OpenMode.ForRead) as Entity;
        if (ent == null) return $"(handle {handleStr})";
        var data = MoravioSmartXDataService.Read(ent);
        if (data == null) return $"{ent.Layer}";
        var mark = string.IsNullOrEmpty(data.Mark) ? "-" : data.Mark;
        var des = string.IsNullOrEmpty(data.Designation) ? "" : $" {data.Designation}";
        return $"{mark}{des}";
    }
}
