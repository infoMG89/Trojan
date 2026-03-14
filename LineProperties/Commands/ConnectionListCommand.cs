using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using LineProperties.Services;
using LineProperties.UI;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(LineProperties.Commands.ConnectionListCommand))]

namespace LineProperties.Commands;

public class ConnectionListCommand
{
    [CommandMethod("CONNECTION_LIST", CommandFlags.Modal)]
    public void ConnectionList()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        var items = ConnectionMarkerListService.CollectMarkersWithEntityInfo(db);
        if (items.Count == 0)
        {
            ed.WriteMessage("\nŽádné connection markery nenalezeny. Spusťte nejdříve CONNECTIONS.");
            return;
        }

        var dialog = new ConnectionListDialog(items);
        Application.ShowModalWindow(dialog);
    }
}
