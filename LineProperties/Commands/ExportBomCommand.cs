using System.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using LineProperties.Services;
using Microsoft.Win32;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(LineProperties.Commands.ExportBomCommand))]

namespace LineProperties.Commands;

public class ExportBomCommand
{
    [CommandMethod("EXPORTBOM", CommandFlags.Modal)]
    public void ExportBom()
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;

        using var tr = db.TransactionManager.StartTransaction();
        var rows = BomService.CollectBomRows(db, tr);
        tr.Commit();

        if (rows.Count == 0)
        {
            ed.WriteMessage("\nNo MORAVIO_SMART entities found in drawing.");
            return;
        }

        var dlg = new SaveFileDialog
        {
            Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
            DefaultExt = ".xlsx",
            FileName = "BOM.xlsx"
        };

        if (dlg.ShowDialog() != true) return;

        try
        {
            BomExcelService.ExportToExcel(dlg.FileName, rows);
            ed.WriteMessage($"\nBOM exported to {dlg.FileName}");
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
