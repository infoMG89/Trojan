using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using LineProperties.Services;
using LineProperties.UI;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(LineProperties.Commands.SmartJoistCommand))]

namespace LineProperties.Commands;

public class SmartJoistCommand
{
    [CommandMethod("SMARTJOIST", CommandFlags.Modal)]
    public void SmartJoist()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var ppo = new PromptPointOptions("\nPick start point of joist: ");
        var ppr1 = ed.GetPoint(ppo);
        if (ppr1.Status != PromptStatus.OK) return;

        ppo.Message = "\nPick end point of joist: ";
        ppo.UseBasePoint = true;
        ppo.BasePoint = ppr1.Value;
        var ppr2 = ed.GetPoint(ppo);
        if (ppr2.Status != PromptStatus.OK) return;

        var start = ppr1.Value;
        var end = ppr2.Value;
        var spanLength = start.DistanceTo(end);

        using var tr = db.TransactionManager.StartTransaction();
        LayerHelper.EnsureJoistsLayer(db, tr);

        var line = new Line(start, end);
        line.Layer = LayerHelper.JoistsLayer;
        line.ColorIndex = 1;

        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
        btr.AppendEntity(line);
        tr.AddNewlyCreatedDBObject(line, true);

        var dialog = new EditJoistDialog();
        if (Application.ShowModalWindow(dialog) != true)
        {
            line.Erase();
            tr.Commit();
            return;
        }

        var mark = dialog.Model.IsTieJoist
            ? MoravioSmartXDataService.GetNextTieJoistMark(db, tr)
            : MoravioSmartXDataService.GetNextJoistMark(db, tr);

        dialog.Model.Mark = mark;
        var data = EditJoistDialog.ToMoravioSmartData(dialog.Model, MoravioSmartXDataService.MemberTypeJoist, spanLength);
        MoravioSmartXDataService.EnsureRegApp(db, tr);
        MoravioSmartXDataService.Write(line, data, db, tr);

        tr.Commit();
        ed.Regen();
    }
}
