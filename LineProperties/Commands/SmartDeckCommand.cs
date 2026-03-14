using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using LineProperties.Data;
using LineProperties.Services;
using LineProperties.UI;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(LineProperties.Commands.SmartDeckCommand))]

namespace LineProperties.Commands;

public class SmartDeckCommand
{
    [CommandMethod("SMARTDECK", CommandFlags.Modal)]
    public void SmartDeck()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var optionsDialog = new SmartDeckOptionsDialog();
        if (Application.ShowModalWindow(optionsDialog) != true) return;

        var plateType = optionsDialog.SelectedPlateType;
        var mark = optionsDialog.Mark;
        var rec = PlateLibrary.GetByDesignation(plateType);
        var thicknessMm = rec?.ThicknessMm ?? 10;

        var ppo = new PromptPointOptions("\nPick first corner of plate: ");
        var ppr1 = ed.GetPoint(ppo);
        if (ppr1.Status != PromptStatus.OK) return;

        ppo.Message = "\nPick opposite corner of plate: ";
        ppo.UseBasePoint = true;
        ppo.BasePoint = ppr1.Value;
        var ppr2 = ed.GetPoint(ppo);
        if (ppr2.Status != PromptStatus.OK) return;

        var p1 = ppr1.Value;
        var p2 = ppr2.Value;
        var minX = Math.Min(p1.X, p2.X);
        var maxX = Math.Max(p1.X, p2.X);
        var minY = Math.Min(p1.Y, p2.Y);
        var maxY = Math.Max(p1.Y, p2.Y);
        var z = (p1.Z + p2.Z) / 2;
        var length = maxX - minX;
        var width = maxY - minY;

        if (length < 1e-6 || width < 1e-6)
        {
            ed.WriteMessage("\nPlech musí mít nenulové rozměry.");
            return;
        }

        using var tr = db.TransactionManager.StartTransaction();
        LayerHelper.EnsurePlatesLayer(db, tr);
        if (string.IsNullOrEmpty(mark)) mark = MoravioSmartXDataService.GetNextPlateMark(db, tr);

        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

        var pline = new Polyline();
        pline.AddVertexAt(0, new Point2d(minX, minY), 0, 0, 0);
        pline.AddVertexAt(1, new Point2d(maxX, minY), 0, 0, 0);
        pline.AddVertexAt(2, new Point2d(maxX, maxY), 0, 0, 0);
        pline.AddVertexAt(3, new Point2d(minX, maxY), 0, 0, 0);
        pline.Closed = true;
        pline.Layer = LayerHelper.PlatesLayer;
        pline.ColorIndex = 2;

        btr.AppendEntity(pline);
        tr.AddNewlyCreatedDBObject(pline, true);

        var data = new MoravioSmartData
        {
            MemberType = MoravioSmartXDataService.MemberTypePlate,
            Mark = mark,
            Designation = plateType,
            SpanLength = Math.Max(length, width),
            ExtensionRight = Math.Min(length, width),
            Depth = thicknessMm
        };
        MoravioSmartXDataService.EnsureRegApp(db, tr);
        MoravioSmartXDataService.Write(pline, data, db, tr);

        tr.Commit();
        ed.Regen();
        ed.WriteMessage($"\nPlech {plateType} (Mark: {mark}) nakreslen.");
    }
}
