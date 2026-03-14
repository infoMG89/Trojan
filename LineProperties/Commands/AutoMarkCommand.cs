using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using LineProperties.Services;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(LineProperties.Commands.AutoMarkCommand))]

namespace LineProperties.Commands;

public class AutoMarkCommand
{
    [CommandMethod("AUTOMARK", CommandFlags.Modal)]
    public void AutoMark()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        using var tr = db.TransactionManager.StartTransaction();

        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

        var joists = new List<(ObjectId Id, MoravioSmartData Data, double Span)>();
        var tieJoists = new List<(ObjectId Id, MoravioSmartData Data, double Span)>();

        foreach (ObjectId id in btr)
        {
            if (!id.IsValid) continue;
            var ent = tr.GetObject(id, OpenMode.ForRead) as Line;
            if (ent == null) continue;
            if (!MoravioSmartXDataService.HasMoravioSmartData(ent)) continue;

            var data = MoravioSmartXDataService.Read(ent);
            if (data == null || data.MemberType != MoravioSmartXDataService.MemberTypeJoist) continue;

            var span = ent.StartPoint.DistanceTo(ent.EndPoint);
            if (data.IsTieJoist)
                tieJoists.Add((id, data, span));
            else
                joists.Add((id, data, span));
        }

        AssignMarks(tr, btr, joists, false);
        AssignMarks(tr, btr, tieJoists, true);

        tr.Commit();
        ed.Regen();
        ed.WriteMessage($"\nAUTOMARK: Assigned marks and length labels to {joists.Count + tieJoists.Count} joists.");
    }

    private static void AssignMarks(
        Transaction tr,
        BlockTableRecord btr,
        List<(ObjectId Id, MoravioSmartData Data, double Span)> items,
        bool tieJoist)
    {
        var groups = new Dictionary<(string Designation, double ExtL, double ExtR, double CL, double CR, double Span), List<ObjectId>>();
        foreach (var (id, data, span) in items)
        {
            var k = (data.Designation ?? "", data.ExtensionLeft, data.ExtensionRight, data.CantileverLeft, data.CantileverRight, span);
            if (!groups.TryGetValue(k, out var list))
            {
                list = new List<ObjectId>();
                groups[k] = list;
            }
            list.Add(id);
        }

        int markNum = 1;
        string prefix = tieJoist ? "TJ" : "J";
        foreach (var (_, list) in groups)
        {
            string mark = $"{prefix}{markNum++}";
            foreach (var id in list)
            {
                var line = (Line)tr.GetObject(id, OpenMode.ForWrite);
                var data = MoravioSmartXDataService.Read(line)!;
                var span = line.StartPoint.DistanceTo(line.EndPoint);
                var updated = data with { Mark = mark };
                MoravioSmartXDataService.Write(line, updated, line.Database, tr);

                AddLengthLabel(btr, tr, line, span, line.Layer);
            }
        }
    }

    private static void AddLengthLabel(BlockTableRecord btr, Transaction tr, Line line, double lengthInches, string layer)
    {
        var mid = line.StartPoint + (line.EndPoint - line.StartPoint) / 2.0;
        var angle = Math.Atan2(line.EndPoint.Y - line.StartPoint.Y, line.EndPoint.X - line.StartPoint.X);
        var lengthStr = FormatLengthFeetInches(lengthInches);

        using var dbText = new DBText();
        dbText.Position = mid;
        dbText.Height = 2.5;
        dbText.TextString = lengthStr;
        dbText.Rotation = angle;
        dbText.Layer = layer;
        dbText.HorizontalMode = TextHorizontalMode.TextCenter;
        dbText.VerticalMode = TextVerticalMode.TextVerticalMid;
        dbText.AlignmentPoint = mid;

        btr.AppendEntity(dbText);
        tr.AddNewlyCreatedDBObject(dbText, true);
    }

    private static string FormatLengthFeetInches(double inches)
    {
        int totalInches = (int)Math.Round(inches);
        int feet = totalInches / 12;
        int remInches = totalInches % 12;
        return remInches == 0 ? $"{feet}'-0\"" : $"{feet}'-{remInches}\"";
    }
}
