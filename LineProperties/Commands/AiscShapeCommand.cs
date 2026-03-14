using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using LineProperties.Data;
using LineProperties.Services;
using LineProperties.UI;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(LineProperties.Commands.AiscShapeCommand))]

namespace LineProperties.Commands;

public class AiscShapeCommand
{
    [CommandMethod("AISC_SHAPE", CommandFlags.Modal)]
    public void AiscShape()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var dialog = new AiscShapeDialog();
        if (Application.ShowModalWindow(dialog) != true) return;

        var designation = dialog.SelectedDesignation;
        var mark = dialog.Mark;
        var rec = AiscShapeLibrary.GetByDesignation(designation);
        if (rec == null)
        {
            ed.WriteMessage($"\nNeznámá designace: {designation}");
            return;
        }

        var ppo = new PromptPointOptions("\nPick start point of beam: ");
        var ppr1 = ed.GetPoint(ppo);
        if (ppr1.Status != PromptStatus.OK) return;

        ppo.Message = "\nPick end point of beam: ";
        ppo.UseBasePoint = true;
        ppo.BasePoint = ppr1.Value;
        var ppr2 = ed.GetPoint(ppo);
        if (ppr2.Status != PromptStatus.OK) return;

        var start = ppr1.Value;
        var end = ppr2.Value;
        var spanLength = start.DistanceTo(end);

        using var tr = db.TransactionManager.StartTransaction();
        LayerHelper.EnsureBeamsLayer(db, tr);

        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

        var line = new Line(start, end);
        line.Layer = LayerHelper.BeamsLayer;
        line.ColorIndex = 4; // cyan

        btr.AppendEntity(line);
        tr.AddNewlyCreatedDBObject(line, true);

        var data = new MoravioSmartData
        {
            MemberType = MoravioSmartXDataService.MemberTypeBeam,
            Mark = mark,
            Designation = designation,
            SpanLength = spanLength,
            Depth = rec.DepthInches,
            Weight = rec.WeightPlf
        };
        MoravioSmartXDataService.EnsureRegApp(db, tr);
        MoravioSmartXDataService.Write(line, data, db, tr);

        tr.Commit();
        ed.Regen();
        ed.WriteMessage($"\nBeam {designation} (Mark: {mark}) drawn.");
    }
}
