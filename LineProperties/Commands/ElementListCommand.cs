using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using LineProperties.Services;
using LineProperties.UI;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(LineProperties.Commands.ElementListCommand))]

namespace LineProperties.Commands;

public class ElementListCommand
{
    [CommandMethod("SMARTJOIST_LIST", CommandFlags.Modal)]
    public void ShowElementList()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;

        List<SmartElement> elements;
        using (var tr = db.TransactionManager.StartTransaction())
        {
            elements = LinePropElementsService.CollectElements(db, tr);
            tr.Commit();
        }

        if (elements.Count == 0)
        {
            doc.Editor.WriteMessage("\nNo MORAVIO_SMART entities in drawing.");
            return;
        }

        var dialog = new ElementListDialog(elements);
        Application.ShowModalWindow(dialog);
    }
}
