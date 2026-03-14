using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using LineProperties.Models;

namespace LineProperties.Services;

public enum ConnectionType { A, B, C } // A=deck-deck, B=joist-deck, C=joist-joist

public record ConnectionInfo(ObjectId Id1, ObjectId Id2, ConnectionType Type, Point3d ConnectionPoint);

public static class ConnectionDetectionService
{
    private const double Tol = 1e-10;

    public static List<ConnectionInfo> FindConnections(Database db, Transaction tr)
    {
        var joists = new List<(ObjectId Id, Line Line)>();
        var decks = new List<(ObjectId Id, Line Line)>();

        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

        foreach (ObjectId id in btr)
        {
            if (!id.IsValid) continue;
            var ent = tr.GetObject(id, OpenMode.ForRead) as Line;
            if (ent == null) continue;

            if (!MoravioSmartXDataService.HasMoravioSmartData(ent)) continue;
            var data = MoravioSmartXDataService.Read(ent);
            if (data == null) continue;

            if (data.MemberType == MoravioSmartXDataService.MemberTypeJoist)
                joists.Add((id, ent));
            else if (data.MemberType == MoravioSmartXDataService.MemberTypeDeck)
                decks.Add((id, ent));
        }

        var result = new List<ConnectionInfo>();
        double tolerance = ElementIdentificationOptions.ConnectionTolerance;

        for (int i = 0; i < joists.Count; i++)
        for (int j = i + 1; j < joists.Count; j++)
        {
            if (TryLineLine(joists[i].Line, joists[j].Line, tolerance, out var pt))
                result.Add(new ConnectionInfo(joists[i].Id, joists[j].Id, ConnectionType.C, pt));
        }

        for (int i = 0; i < joists.Count; i++)
        for (int j = 0; j < decks.Count; j++)
        {
            if (TryLineLine(joists[i].Line, decks[j].Line, tolerance, out var pt))
                result.Add(new ConnectionInfo(joists[i].Id, decks[j].Id, ConnectionType.B, pt));
        }

        for (int i = 0; i < decks.Count; i++)
        for (int j = i + 1; j < decks.Count; j++)
        {
            if (TryLineLine(decks[i].Line, decks[j].Line, tolerance, out var pt))
                result.Add(new ConnectionInfo(decks[i].Id, decks[j].Id, ConnectionType.A, pt));
        }

        return result;
    }

    private static bool TryLineLine(Line a, Line b, double tolerance, out Point3d connectionPoint)
    {
        connectionPoint = default;
        var p1 = a.StartPoint;
        var p2 = a.EndPoint;
        var p3 = b.StartPoint;
        var p4 = b.EndPoint;

        if (SegmentSegmentIntersect(p1, p2, p3, p4, out var pt))
        {
            connectionPoint = pt;
            return true;
        }

        double d = SegmentToSegmentDistance(p1, p2, p3, p4, out var pa, out var pb);
        if (d <= tolerance)
        {
            double z = (p1.Z + p2.Z) / 2;
            connectionPoint = new Point3d((pa.X + pb.X) / 2, (pa.Y + pb.Y) / 2, z);
            return true;
        }
        return false;
    }

    private static bool SegmentSegmentIntersect(Point3d p1, Point3d p2, Point3d p3, Point3d p4, out Point3d result)
    {
        result = default;
        double ua = (p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X);
        double ub = (p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X);
        double denom = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);
        if (Math.Abs(denom) < Tol) return false;
        ua /= denom;
        ub /= denom;
        if (ua < 0 || ua > 1 || ub < 0 || ub > 1) return false;
        result = new Point3d(p1.X + ua * (p2.X - p1.X), p1.Y + ua * (p2.Y - p1.Y), p1.Z + ua * (p2.Z - p1.Z));
        return true;
    }

    private static double SegmentToSegmentDistance(Point3d p1, Point3d p2, Point3d p3, Point3d p4, out Point2d pa, out Point2d pb)
    {
        var a = new Point2d(p1.X, p1.Y);
        var b = new Point2d(p2.X, p2.Y);
        var c = new Point2d(p3.X, p3.Y);
        var d = new Point2d(p4.X, p4.Y);

        double minD = double.MaxValue;
        pa = a;
        pb = c;

        PointToSegment(a, c, d, out var p, out double d1);
        if (d1 < minD) { minD = d1; pa = a; pb = p; }
        PointToSegment(b, c, d, out p, out d1);
        if (d1 < minD) { minD = d1; pa = b; pb = p; }
        PointToSegment(c, a, b, out p, out d1);
        if (d1 < minD) { minD = d1; pa = p; pb = c; }
        PointToSegment(d, a, b, out p, out d1);
        if (d1 < minD) { minD = d1; pa = p; pb = d; }

        return minD;
    }

    private static void PointToSegment(Point2d pt, Point2d s0, Point2d s1, out Point2d closest, out double distance)
    {
        var v = s1 - s0;
        var w = pt - s0;
        double c1 = v.X * w.X + v.Y * w.Y;
        if (c1 <= 0) { closest = s0; distance = pt.GetDistanceTo(s0); return; }
        double c2 = v.X * v.X + v.Y * v.Y;
        if (c2 <= c1) { closest = s1; distance = pt.GetDistanceTo(s1); return; }
        double t = c1 / c2;
        closest = new Point2d(s0.X + t * v.X, s0.Y + t * v.Y);
        distance = pt.GetDistanceTo(closest);
    }
}
