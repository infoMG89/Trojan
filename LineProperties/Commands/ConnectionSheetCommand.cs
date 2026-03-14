using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using LineProperties.Services;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(LineProperties.Commands.ConnectionSheetCommand))]

namespace LineProperties.Commands;

public class ConnectionSheetCommand
{
    [CommandMethod("CONNECTION_SHEET", CommandFlags.Modal)]
    public void ConnectionSheet()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var markers = ConnectionMarkerListService.CollectMarkers(db);
        if (markers.Count == 0)
        {
            ed.WriteMessage("\nŽádné connection markery nenalezeny. Spusťte nejdříve CONNECTIONS.");
            return;
        }

        using var tr = db.TransactionManager.StartTransaction();
        int created = 0;
        foreach (var m in markers)
        {
            var id1 = ConnectionMarkerService.GetObjectIdByHandleString(db, m.Handle1);
            var id2 = ConnectionMarkerService.GetObjectIdByHandleString(db, m.Handle2);
            var layoutId = ConnectionLayoutService.CreateLayoutFromTemplate(db, tr, m.Type, m.ConnectionId, m.ChosenCode, id1, id2);
            if (layoutId.HasValue) created++;
        }
        tr.Commit();

        ed.WriteMessage($"\nVytvořeno {created} sheetů ze šablon.");
        ed.Regen();
    }
}
