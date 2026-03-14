using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using LineProperties.Services;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(LineProperties.Commands.TemplateCheckCommand))]

namespace LineProperties.Commands;

public class TemplateCheckCommand
{
    [CommandMethod("TEMPLATE_CHECK", CommandFlags.Modal)]
    public void TemplateCheck()
    {
        var ed = Application.DocumentManager.MdiActiveDocument.Editor;

        foreach (var type in new[] { ConnectionType.A, ConnectionType.B, ConnectionType.C })
        {
            var name = type switch { ConnectionType.A => "A", ConnectionType.B => "B", ConnectionType.C => "C", _ => "?" };
            var fileName = type switch
            {
                ConnectionType.A => "Connection_A_Template.dwg",
                ConnectionType.B => "Connection_B_Template.dwg",
                ConnectionType.C => "Connection_C_Template.dwg",
                _ => ""
            };
            ed.WriteMessage($"\n--- {name} ({fileName}) ---");

            var result = ConnectionLayoutService.CheckTemplate(type);
            if (result == null)
            {
                ed.WriteMessage("\n  Soubor nenalezen nebo chyba při čtení.");
                continue;
            }

            ed.WriteMessage($"\n  Layout: '{result.Value.LayoutName}'");
            ed.WriteMessage($"\n  Počet entit v layoutu: {result.Value.EntityCount}");
            ed.WriteMessage($"\n  Bloky: {string.Join(", ", result.Value.BlockNames)}");
        }
        ed.WriteMessage("\n");
    }
}
