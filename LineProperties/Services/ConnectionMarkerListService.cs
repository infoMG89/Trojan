using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace LineProperties.Services;

public record ConnectionMarkerInfo(string Handle1, string Handle2, ConnectionType Type, string ConnectionId, string? ChosenCode);

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
}
