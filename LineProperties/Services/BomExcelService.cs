using ClosedXML.Excel;

namespace LineProperties.Services;

/// <summary>
/// Exports BOM rows to Excel (.xlsx).
/// </summary>
public static class BomExcelService
{
    /// <summary>Exports BOM rows to the specified file path.</summary>
    public static void ExportToExcel(string filePath, List<BomRow> rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("BOM");

        ws.Cell(1, 1).Value = "Mark";
        ws.Cell(1, 2).Value = "Type";
        ws.Cell(1, 3).Value = "Designation";
        ws.Cell(1, 4).Value = "Span";
        ws.Cell(1, 5).Value = "EXTL";
        ws.Cell(1, 6).Value = "EXTR";
        ws.Cell(1, 7).Value = "Depth";
        ws.Cell(1, 8).Value = "Weight";
        ws.Cell(1, 9).Value = "Count";

        var headerRange = ws.Range(1, 1, 1, 9);
        headerRange.Style.Font.Bold = true;

        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            int row = i + 2;
            ws.Cell(row, 1).Value = r.Mark;
            ws.Cell(row, 2).Value = r.Type;
            ws.Cell(row, 3).Value = r.Designation;
            ws.Cell(row, 4).Value = r.Dimensions;
            ws.Cell(row, 5).Value = r.ExtensionLeft;
            ws.Cell(row, 6).Value = r.ExtensionRight;
            ws.Cell(row, 7).Value = r.Depth;
            ws.Cell(row, 8).Value = r.Weight;
            ws.Cell(row, 9).Value = r.Count;
        }

        ws.Columns().AdjustToContents();
        wb.SaveAs(filePath);
    }
}
