using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using LineProperties.Models;

namespace LineProperties.Services;

public enum ConnectionType { A, B, C } // A=deck-deck/plate-plate/plate-deck, B=joist-deck/joist-plate, C=joist-joist

/// <summary>Connection info. When ConnectionPointEnd equals ConnectionPoint (or is default), contact is a point; otherwise a segment.</summary>
public record ConnectionInfo(ObjectId Id1, ObjectId Id2, ConnectionType Type, Point3d ConnectionPoint, Point3d ConnectionPointEnd);

public static class ConnectionDetectionService
{
    private const double Tol = 1e-10;

    public static List<ConnectionInfo> FindConnections(Database db, Transaction tr)
    {
        var joists = new List<(ObjectId Id, Line Line)>();
        var beams = new List<(ObjectId Id, Line Line)>();
        var decks = new List<(ObjectId Id, Line Line)>();
        var plates = new List<(ObjectId Id, Polyline Polyline)>();

        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

        foreach (ObjectId id in btr)
        {
            if (!id.IsValid) continue;

            if (tr.GetObject(id, OpenMode.ForRead) is Line line)
            {
                if (!MoravioSmartXDataService.HasMoravioSmartData(line)) continue;
                var data = MoravioSmartXDataService.Read(line);
                if (data == null) continue;
                if (data.MemberType == MoravioSmartXDataService.MemberTypeJoist)
                    joists.Add((id, line));
                else if (data.MemberType == MoravioSmartXDataService.MemberTypeBeam)
                    beams.Add((id, line));
                else if (data.MemberType == MoravioSmartXDataService.MemberTypeDeck)
                    decks.Add((id, line));
                continue;
            }

            if (tr.GetObject(id, OpenMode.ForRead) is Polyline pline && pline.Closed)
            {
                if (!MoravioSmartXDataService.HasMoravioSmartData(pline)) continue;
                var data = MoravioSmartXDataService.Read(pline);
                if (data == null || data.MemberType != MoravioSmartXDataService.MemberTypePlate) continue;
                plates.Add((id, pline));
            }
        }

        var result = new List<ConnectionInfo>();
        double tolerance = ElementIdentificationOptions.ConnectionTolerance;

        for (int i = 0; i < joists.Count; i++)
        for (int j = i + 1; j < joists.Count; j++)
        {
            if (TryLineLine(joists[i].Line, joists[j].Line, tolerance, out var pt1, out var pt2))
                result.Add(new ConnectionInfo(joists[i].Id, joists[j].Id, ConnectionType.C, pt1, pt2));
        }

        for (int i = 0; i < joists.Count; i++)
        foreach (var (beamId, beam) in beams)
        {
            if (TryLineLine(joists[i].Line, beam, tolerance, out var pt1, out var pt2))
                result.Add(new ConnectionInfo(joists[i].Id, beamId, ConnectionType.B, pt1, pt2));
        }

        for (int i = 0; i < joists.Count; i++)
        for (int j = 0; j < decks.Count; j++)
        {
            if (TryLineLine(joists[i].Line, decks[j].Line, tolerance, out var pt1, out var pt2))
                result.Add(new ConnectionInfo(joists[i].Id, decks[j].Id, ConnectionType.B, pt1, pt2));
        }

        foreach (var (beamId, beam) in beams)
        for (int j = 0; j < decks.Count; j++)
        {
            if (TryLineLine(beam, decks[j].Line, tolerance, out var pt1, out var pt2))
                result.Add(new ConnectionInfo(beamId, decks[j].Id, ConnectionType.B, pt1, pt2));
        }

        for (int i = 0; i < joists.Count; i++)
        foreach (var (plateId, plate) in plates)
        {
            if (TryLinePolyline(joists[i].Line, plate, tolerance, out var pt1, out var pt2))
                result.Add(new ConnectionInfo(joists[i].Id, plateId, ConnectionType.B, pt1, pt2));
        }

        foreach (var (beamId, beam) in beams)
        foreach (var (plateId, plate) in plates)
        {
            if (TryLinePolyline(beam, plate, tolerance, out var pt1, out var pt2))
                result.Add(new ConnectionInfo(beamId, plateId, ConnectionType.B, pt1, pt2));
        }

        for (int i = 0; i < decks.Count; i++)
        foreach (var (plateId, plate) in plates)
        {
            if (TryLinePolyline(decks[i].Line, plate, tolerance, out var pt1, out var pt2))
                result.Add(new ConnectionInfo(decks[i].Id, plateId, ConnectionType.A, pt1, pt2));
        }

        for (int i = 0; i < decks.Count; i++)
        for (int j = i + 1; j < decks.Count; j++)
        {
            if (TryLineLine(decks[i].Line, decks[j].Line, tolerance, out var pt1, out var pt2))
                result.Add(new ConnectionInfo(decks[i].Id, decks[j].Id, ConnectionType.A, pt1, pt2));
        }

        for (int i = 0; i < plates.Count; i++)
        for (int j = i + 1; j < plates.Count; j++)
        {
            if (TryPolylinePolyline(plates[i].Polyline, plates[j].Polyline, tolerance, out var pt1, out var pt2))
                result.Add(new ConnectionInfo(plates[i].Id, plates[j].Id, ConnectionType.A, pt1, pt2));
        }

        return result;
    }

    private static bool TryLinePolyline(Line line, Polyline pline, double tolerance, out Point3d pt1, out Point3d pt2)
    {
        pt1 = pt2 = default;
        var p1 = line.StartPoint;
        var p2 = line.EndPoint;
        int n = pline.NumberOfVertices;
        for (int i = 0; i < n; i++)
        {
            var j = (i + 1) % n;
            var p3 = pline.GetPoint3dAt(i);
            var p4 = pline.GetPoint3dAt(j);
            if (TryLineLine(p1, p2, p3, p4, tolerance, out pt1, out pt2))
                return true;
        }
        return false;
    }

    private static bool TryPolylinePolyline(Polyline a, Polyline b, double tolerance, out Point3d pt1, out Point3d pt2)
    {
        pt1 = pt2 = default;
        int na = a.NumberOfVertices;
        int nb = b.NumberOfVertices;
        for (int i = 0; i < na; i++)
        {
            var ia = (i + 1) % na;
            var p1 = a.GetPoint3dAt(i);
            var p2 = a.GetPoint3dAt(ia);
            for (int j = 0; j < nb; j++)
            {
                var jb = (j + 1) % nb;
                var p3 = b.GetPoint3dAt(j);
                var p4 = b.GetPoint3dAt(jb);
                if (TryLineLine(p1, p2, p3, p4, tolerance, out pt1, out pt2))
                    return true;
            }
        }
        return false;
    }

    private static bool TryLineLine(Point3d p1, Point3d p2, Point3d p3, Point3d p4, double tolerance, out Point3d pt1, out Point3d pt2)
    {
        var la = new Line(p1, p2);
        var lb = new Line(p3, p4);
        return TryLineLine(la, lb, tolerance, out pt1, out pt2);
    }

    private static bool TryLineLine(Line a, Line b, double tolerance, out Point3d pt1, out Point3d pt2)
    {
        pt1 = pt2 = default;
        var p1 = a.StartPoint;
        var p2 = a.EndPoint;
        var p3 = b.StartPoint;
        var p4 = b.EndPoint;

        if (SegmentSegmentOverlap(p1, p2, p3, p4, out var overlapStart, out var overlapEnd))
        {
            pt1 = overlapStart;
            pt2 = overlapEnd;
            return true;
        }

        if (SegmentSegmentIntersect(p1, p2, p3, p4, out var pt))
        {
            pt1 = pt2 = pt;
            return true;
        }

        double d = SegmentToSegmentDistance(p1, p2, p3, p4, out var pa, out var pb);
        if (d <= tolerance)
        {
            double z = (p1.Z + p2.Z) / 2;
            pt1 = pt2 = new Point3d((pa.X + pb.X) / 2, (pa.Y + pb.Y) / 2, z);
            return true;
        }
        return false;
    }

    /// <summary>When segments are collinear and overlap, returns the overlap segment. Otherwise false.</summary>
    private static bool SegmentSegmentOverlap(Point3d p1, Point3d p2, Point3d p3, Point3d p4, out Point3d start, out Point3d end)
    {
        start = end = default;
        var d1 = p2 - p1;
        var d2 = p4 - p3;
        if (d1.Length < Tol || d2.Length < Tol) return false;
        if (d1.CrossProduct(d2).Length > Tol) return false;

        var lenSq = d1.Length * d1.Length;
        double t3 = (p3 - p1).DotProduct(d1) / lenSq;
        double t4 = (p4 - p1).DotProduct(d1) / lenSq;
        double tMin = Math.Min(t3, t4);
        double tMax = Math.Max(t3, t4);
        double overlapStart = Math.Max(0, tMin);
        double overlapEnd = Math.Min(1, tMax);
        if (overlapStart >= overlapEnd - Tol) return false;

        start = p1 + overlapStart * d1;
        end = p1 + overlapEnd * d1;
        return true;
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
