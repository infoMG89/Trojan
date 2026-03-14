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

        var deckType = optionsDialog.SelectedDeckType;
        var spacing = DeckLibrary.GetRibSpacing(deckType);

        var ppo = new PromptPointOptions("\nPick start point of deck span: ");
        var ppr1 = ed.GetPoint(ppo);
        if (ppr1.Status != PromptStatus.OK) return;

        ppo.Message = "\nPick end point of deck span: ";
        ppo.UseBasePoint = true;
        ppo.BasePoint = ppr1.Value;
        var ppr2 = ed.GetPoint(ppo);
        if (ppr2.Status != PromptStatus.OK) return;

        var start = ppr1.Value;
        var end = ppr2.Value;
        var spanLength = start.DistanceTo(end);
        var dir = end - start;
        var length = dir.Length;
        if (length < 1e-10) return;

        dir = dir.GetNormal();
        var perp = new Vector3d(-dir.Y, dir.X, 0);

        using var tr = db.TransactionManager.StartTransaction();
        LayerHelper.EnsureDeckLayer(db, tr);

        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

        double offset = 0;
        int ribIndex = 0;
        var deckRec = DeckLibrary.GetByDeckType(deckType);
        while (offset <= length + 1e-6)
        {
            var p1 = start + perp * offset;
            var p2 = end + perp * offset;
            var line = new Line(p1, p2);
            line.Layer = LayerHelper.DeckLayer;
            line.ColorIndex = 3;

            btr.AppendEntity(line);
            tr.AddNewlyCreatedDBObject(line, true);

            var data = new MoravioSmartData
            {
                MemberType = MoravioSmartXDataService.MemberTypeDeck,
                Mark = $"D{ribIndex + 1}",
                Designation = deckType,
                SpanLength = spanLength,
                Depth = deckRec?.ProfileDepthInches ?? 1.5
            };
            MoravioSmartXDataService.EnsureRegApp(db, tr);
            MoravioSmartXDataService.Write(line, data, db, tr);

            offset += spacing;
            ribIndex++;
        }

        tr.Commit();
        ed.Regen();
    }
}
