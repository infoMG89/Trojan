using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using LineProperties.Services;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(LineProperties.Commands.SmartMatchCommand))]

namespace LineProperties.Commands;

/// <summary>
/// Copy MORAVIO_SMART properties from one entity to others. Minimal steps: pick source → pick targets.
/// </summary>
public class SmartMatchCommand
{
    [CommandMethod("SMARTMATCH", CommandFlags.Modal)]
    public void SmartMatch()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var pso = new PromptEntityOptions("\nSelect source line or plate (with smart properties): ");
        pso.SetRejectMessage("\nSelect a Line or closed Polyline.");
        pso.AddAllowedClass(typeof(Line), true);
        pso.AddAllowedClass(typeof(Polyline), true);

        var per = ed.GetEntity(pso);
        if (per.Status != PromptStatus.OK) return;

        using var tr = db.TransactionManager.StartTransaction();
        var sourceEnt = tr.GetObject(per.ObjectId, OpenMode.ForRead) as Entity;
        if (sourceEnt == null) { tr.Commit(); return; }

        var sourceData = MoravioSmartXDataService.Read(sourceEnt);
        if (sourceData == null)
        {
            ed.WriteMessage("\nSource entity has no MORAVIO_SMART data.");
            tr.Commit();
            return;
        }

        var isLine = sourceEnt is Line;
        var filter = new SelectionFilter(new[]
        {
            new TypedValue((int)DxfCode.Operator, "<or"),
            new TypedValue((int)DxfCode.Start, "LINE"),
            new TypedValue((int)DxfCode.Start, "LWPOLYLINE"),
            new TypedValue((int)DxfCode.Operator, "or>")
        });

        var pss = new PromptSelectionOptions();
        pss.MessageForAdding = "\nSelect target lines or plates to apply: ";

        var psr = ed.GetSelection(pss, filter);
        if (psr.Status != PromptStatus.OK) { tr.Commit(); return; }

        var ids = psr.Value.GetObjectIds();
        var applied = 0;

        foreach (var id in ids)
        {
            if (id == per.ObjectId) continue;

            var targetEnt = tr.GetObject(id, OpenMode.ForRead) as Entity;
            if (targetEnt == null) continue;

            if (targetEnt is Line && !isLine) continue;
            if (targetEnt is Polyline pl && (!pl.Closed || isLine)) continue;

            var targetData = sourceData with { };
            if (targetEnt is Line line)
            {
                var len = line.StartPoint.DistanceTo(line.EndPoint);
                targetData = targetData with { SpanLength = len };
            }
            else if (targetEnt is Polyline pline)
            {
                double minX = double.MaxValue, maxX = double.MinValue, minY = double.MaxValue, maxY = double.MinValue;
                for (int i = 0; i < pline.NumberOfVertices; i++)
                {
                    var p = pline.GetPoint2dAt(i);
                    if (p.X < minX) minX = p.X;
                    if (p.X > maxX) maxX = p.X;
                    if (p.Y < minY) minY = p.Y;
                    if (p.Y > maxY) maxY = p.Y;
                }
                var w = maxX - minX;
                var h = maxY - minY;
                targetData = targetData with
                {
                    SpanLength = Math.Max(w, h),
                    ExtensionRight = Math.Min(w, h)
                };
            }

            targetEnt.UpgradeOpen();
            MoravioSmartXDataService.EnsureRegApp(db, tr);
            MoravioSmartXDataService.Write(targetEnt, targetData, db, tr);

            switch (sourceData.MemberType)
            {
                case MoravioSmartXDataService.MemberTypeJoist: LayerHelper.EnsureJoistsLayer(db, tr); targetEnt.Layer = LayerHelper.JoistsLayer; break;
                case MoravioSmartXDataService.MemberTypeBeam: LayerHelper.EnsureBeamsLayer(db, tr); targetEnt.Layer = LayerHelper.BeamsLayer; break;
                case MoravioSmartXDataService.MemberTypeDeck: LayerHelper.EnsureDeckLayer(db, tr); targetEnt.Layer = LayerHelper.DeckLayer; break;
                case MoravioSmartXDataService.MemberTypePlate: LayerHelper.EnsurePlatesLayer(db, tr); targetEnt.Layer = LayerHelper.PlatesLayer; break;
            }
            applied++;
        }

        tr.Commit();
        ed.Regen();
        ed.WriteMessage($"\nApplied to {applied} entit(y/ies).");
    }
}
