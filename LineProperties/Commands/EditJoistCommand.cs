using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using LineProperties.Models;
using LineProperties.Services;
using LineProperties.UI;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(LineProperties.Commands.EditJoistCommand))]

namespace LineProperties.Commands;

public class EditJoistCommand
{
    [CommandMethod("EDITJOIST", CommandFlags.Modal)]
    public void EditJoist()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var filter = new SelectionFilter(new[] { new TypedValue((int)DxfCode.Start, "LINE") });
        var pso = new PromptSelectionOptions();
        pso.MessageForAdding = "\nSelect joist line(s): ";

        var psr = ed.GetSelection(pso, filter);
        if (psr.Status != PromptStatus.OK) return;

        var ids = psr.Value.GetObjectIds();
        if (ids.Length == 0) return;

        using var tr = db.TransactionManager.StartTransaction();

        var firstLine = (Line)tr.GetObject(ids[0], OpenMode.ForRead);
        var existing = MoravioSmartXDataService.Read(firstLine);
        var spanLength = firstLine.StartPoint.DistanceTo(firstLine.EndPoint);

        JoistPropertyModel model;
        if (existing != null)
            model = EditJoistDialog.FromMoravioSmartData(existing);
        else
            model = new JoistPropertyModel { Designation = Data.SjiJoistLibrary.DefaultDesignation };

        var dialog = new EditJoistDialog(model);
        if (Application.ShowModalWindow(dialog) != true)
        {
            tr.Commit();
            return;
        }

        var data = EditJoistDialog.ToMoravioSmartData(dialog.Model, MoravioSmartXDataService.MemberTypeJoist, spanLength);

        foreach (var id in ids)
        {
            var line = (Line)tr.GetObject(id, OpenMode.ForRead);
            var len = line.StartPoint.DistanceTo(line.EndPoint);
            var d = EditJoistDialog.ToMoravioSmartData(dialog.Model, MoravioSmartXDataService.MemberTypeJoist, len);
            line.UpgradeOpen();
            MoravioSmartXDataService.EnsureRegApp(db, tr);
            MoravioSmartXDataService.Write(line, d, db, tr);
        }

        tr.Commit();
        ed.Regen();
    }
}
