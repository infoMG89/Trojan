using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using LineProperties.Data;
using LineProperties.Services;
using LineProperties.UI;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(LineProperties.Commands.ConnectionsCommand))]

namespace LineProperties.Commands;

public class ConnectionsCommand
{
    [CommandMethod("CONNECTIONS", CommandFlags.Modal)]
    public void Connections()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        using var tr = db.TransactionManager.StartTransaction();
        var connections = ConnectionDetectionService.FindConnections(db, tr);
        if (connections.Count == 0)
        {
            ed.WriteMessage("\nŽádné spoje nenalezeny (joists/deck s MORAVIO_SMART).");
            tr.Commit();
            return;
        }

        ConnectionDrawingService.DrawConnections(connections, db, tr);
        tr.Commit();
        ed.Regen();
        ed.WriteMessage($"\nNalezeno {connections.Count} spojů. Klikněte na marker pro výběr kódu (příkaz CONNECTION_MARKERS).");

        PickAndAssignCode(doc, db, ed);
    }

    private static void PickAndAssignCode(Document doc, Database db, Editor ed)
    {
        var filter = new SelectionFilter(new[]
        {
            new TypedValue((int)DxfCode.Operator, "<or"),
            new TypedValue((int)DxfCode.Start, "CIRCLE"),
            new TypedValue((int)DxfCode.Start, "LINE"),
            new TypedValue((int)DxfCode.Operator, "or>")
        });
        var pso = new PromptSelectionOptions();
        pso.MessageForAdding = "\nVyberte marker spoje pro přiřazení kódu (Enter = konec): ";

        while (true)
        {
            var psr = ed.GetSelection(pso, filter);
            if (psr.Status != PromptStatus.OK) break;

            var ids = psr.Value.GetObjectIds();
            if (ids.Length == 0) break;

            using var tr = db.TransactionManager.StartTransaction();
            var marker = tr.GetObject(ids[0], OpenMode.ForRead) as Entity;
            if (marker == null) { tr.Commit(); continue; }

            var data = ConnectionMarkerService.GetMarkerData(marker);
            if (data == null) { tr.Commit(); continue; }

            var type = data.Value.Type;
            var codes = ConnectionLibrary.GetCodesForType(type);
            var items = new List<ConnectionCodeDialog.CodeItem>();
            foreach (var c in codes)
                items.Add(new ConnectionCodeDialog.CodeItem(c, ConnectionLibrary.GetDescription(c)));

            tr.Commit();

            var dialog = new ConnectionCodeDialog(data.Value.ConnectionId, items);
            if (Application.ShowModalWindow(dialog) != true) continue;

            var chosen = dialog.SelectedCode;
            if (string.IsNullOrEmpty(chosen)) continue;

            using (var tr2 = db.TransactionManager.StartTransaction())
            {
                marker = tr2.GetObject(ids[0], OpenMode.ForWrite) as Entity;
                if (marker != null)
                {
                    ConnectionMarkerService.SetChosenCode(marker, chosen, db, tr2);
                    ConnectionDrawingService.AddAnnotationForMarker(db, tr2, marker, chosen, ConnectionLibrary.GetDescription(chosen));
                }
                tr2.Commit();
            }
            doc.Editor.Regen();
        }
    }

    [CommandMethod("CONNECTION_MARKERS", CommandFlags.Modal)]
    public void ConnectionMarkers()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        PickAndAssignCode(doc, db, ed);
    }
}
