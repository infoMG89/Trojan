using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using LineProperties.Data;
using LineProperties.Services;
using LineProperties.UI;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(LineProperties.Commands.SmartAssignCommand))]

namespace LineProperties.Commands;

/// <summary>
/// Assign MORAVIO_SMART properties to one or more lines/polylines. Select → dialog → apply.
/// </summary>
public class SmartAssignCommand
{
    [CommandMethod("SMARTASSIGN", CommandFlags.Modal)]
    public void SmartAssign()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var filter = new SelectionFilter(new[]
        {
            new TypedValue((int)DxfCode.Operator, "<or"),
            new TypedValue((int)DxfCode.Start, "LINE"),
            new TypedValue((int)DxfCode.Start, "LWPOLYLINE"),
            new TypedValue((int)DxfCode.Operator, "or>")
        });

        var pso = new PromptSelectionOptions();
        pso.MessageForAdding = "\nSelect lines or plates to assign: ";

        var psr = ed.GetSelection(pso, filter);
        if (psr.Status != PromptStatus.OK) return;

        var ids = psr.Value.GetObjectIds();
        if (ids.Length == 0) return;

        using var tr = db.TransactionManager.StartTransaction();
        var first = tr.GetObject(ids[0], OpenMode.ForRead) as Entity;
        var isPolyline = first is Polyline pl && pl.Closed;
        tr.Commit();

        var dialog = new SmartAssignDialog(isPolyline);
        if (Application.ShowModalWindow(dialog) != true) return;

        using var tr2 = db.TransactionManager.StartTransaction();
        MoravioSmartXDataService.EnsureRegApp(db, tr2);

        var applied = 0;
        var useAutoMark = string.IsNullOrEmpty(dialog.Mark);
        foreach (var id in ids)
        {
            var ent = tr2.GetObject(id, OpenMode.ForRead) as Entity;
            if (ent == null) continue;
            if (isPolyline && ent is not Polyline) continue;
            if (isPolyline && ent is Polyline poly && !poly.Closed) continue;
            if (!isPolyline && ent is Polyline) continue;

            var data = BuildMoravioData(ent, dialog, db, tr2, useAutoMark);
            if (data == null) continue;

            ent.UpgradeOpen();
            MoravioSmartXDataService.Write(ent, data, db, tr2);

            switch (data.MemberType)
            {
                case MoravioSmartXDataService.MemberTypeJoist: LayerHelper.EnsureJoistsLayer(db, tr2); ent.Layer = LayerHelper.JoistsLayer; break;
                case MoravioSmartXDataService.MemberTypeBeam: LayerHelper.EnsureBeamsLayer(db, tr2); ent.Layer = LayerHelper.BeamsLayer; break;
                case MoravioSmartXDataService.MemberTypeDeck: LayerHelper.EnsureDeckLayer(db, tr2); ent.Layer = LayerHelper.DeckLayer; break;
                case MoravioSmartXDataService.MemberTypePlate: LayerHelper.EnsurePlatesLayer(db, tr2); ent.Layer = LayerHelper.PlatesLayer; break;
            }
            applied++;
        }

        tr2.Commit();
        ed.Regen();
        ed.WriteMessage($"\nPřiřazeno {applied} entit(y/ies).");
    }

    private static MoravioSmartData? BuildMoravioData(Entity ent, SmartAssignDialog dialog, Database db, Transaction tr, bool useAutoMark)
    {
        var mark = useAutoMark ? dialog.MemberType switch
        {
            MoravioSmartXDataService.MemberTypeJoist => MoravioSmartXDataService.GetNextJoistMark(db, tr),
            MoravioSmartXDataService.MemberTypeBeam => MoravioSmartXDataService.GetNextBeamMark(db, tr),
            MoravioSmartXDataService.MemberTypeDeck => MoravioSmartXDataService.GetNextDeckMark(db, tr),
            MoravioSmartXDataService.MemberTypePlate => MoravioSmartXDataService.GetNextPlateMark(db, tr),
            _ => MoravioSmartXDataService.GetNextJoistMark(db, tr)
        } : dialog.Mark;

        if (ent is Line line)
        {
            var len = line.StartPoint.DistanceTo(line.EndPoint);
            var isBeam = dialog.MemberType == MoravioSmartXDataService.MemberTypeBeam;
            var (depth, weight) = GetDepthWeight(dialog.Designation, dialog.MemberType);

            return new MoravioSmartData
            {
                MemberType = dialog.MemberType,
                Mark = mark,
                Designation = dialog.Designation,
                SpanLength = len,
                Depth = depth,
                Weight = weight
            };
        }

        if (ent is Polyline pline && pline.Closed)
        {
            var (minX, maxX, minY, maxY) = (double.MaxValue, double.MinValue, double.MaxValue, double.MinValue);
            for (int i = 0; i < pline.NumberOfVertices; i++)
            {
                var p = pline.GetPoint2dAt(i);
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }
            var length = Math.Max(maxX - minX, maxY - minY);
            var width = Math.Min(maxX - minX, maxY - minY);
            var plateRec = PlateLibrary.GetByDesignation(dialog.Designation);
            var thickness = plateRec?.ThicknessMm ?? 10;

            return new MoravioSmartData
            {
                MemberType = MoravioSmartXDataService.MemberTypePlate,
                Mark = mark,
                Designation = dialog.Designation,
                SpanLength = length,
                ExtensionRight = width,
                Depth = thickness
            };
        }

        return null;
    }

    private static (double depth, double weight) GetDepthWeight(string designation, string memberType)
    {
        if (memberType == MoravioSmartXDataService.MemberTypeJoist)
        {
            var rec = SjiJoistLibrary.GetByDesignation(designation);
            return rec != null ? (rec.DepthInches * 25.4, rec.WeightPlf) : (0, 0);
        }
        if (memberType == MoravioSmartXDataService.MemberTypeBeam)
        {
            var rec = AiscShapeLibrary.GetByDesignation(designation);
            return rec != null ? (rec.DepthInches * 25.4, rec.WeightPlf) : (0, 0);
        }
        return (0, 0);
    }
}
