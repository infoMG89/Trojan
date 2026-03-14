using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace LineProperties.Services;

public static class ConnectionDrawingService
{
    private const double MarkerRadius = 2.5;
    private const double AnnotationOffset = 6;
    private const double AnnotationHeight = 2.5;

    public static void DrawConnections(List<ConnectionInfo> connections, Database db, Transaction tr)
    {
        if (connections.Count == 0) return;

        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

        foreach (var conn in connections)
        {
            var connectionId = ConnectionIdService.GetNextConnectionId(db, tr);
            var ent1 = tr.GetObject(conn.Id1, OpenMode.ForRead) as Entity;
            var ent2 = tr.GetObject(conn.Id2, OpenMode.ForRead) as Entity;
            if (ent1 == null || ent2 == null) continue;

            var circle = new Circle(conn.ConnectionPoint, Vector3d.ZAxis, MarkerRadius);
            circle.ColorIndex = 5;
            btr.AppendEntity(circle);
            tr.AddNewlyCreatedDBObject(circle, true);
            ConnectionMarkerService.SetMarkerData(circle, ent1.Handle.ToString(), ent2.Handle.ToString(), conn.Type, connectionId, db, tr);
        }
    }

    public static void AddAnnotationForMarker(Database db, Transaction tr, Entity marker, string code, string? description = null)
    {
        Point3d textPos;
        if (marker is Circle circle)
            textPos = new Point3d(circle.Center.X + AnnotationOffset, circle.Center.Y, circle.Center.Z);
        else if (marker is Line line)
        {
            var mid = new Point3d((line.StartPoint.X + line.EndPoint.X) / 2, (line.StartPoint.Y + line.EndPoint.Y) / 2, line.StartPoint.Z);
            textPos = new Point3d(mid.X + AnnotationOffset, mid.Y, mid.Z);
        }
        else
            return;

        var textStr = string.IsNullOrEmpty(description) ? code : $"{code} – {description}";

        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
        using var dbText = new DBText();
        dbText.Position = textPos;
        dbText.Height = AnnotationHeight;
        dbText.TextString = textStr;
        dbText.ColorIndex = 5;
        dbText.HorizontalMode = TextHorizontalMode.TextLeft;
        dbText.VerticalMode = TextVerticalMode.TextVerticalMid;
        dbText.AlignmentPoint = textPos;
        btr.AppendEntity(dbText);
        tr.AddNewlyCreatedDBObject(dbText, true);
    }
}
