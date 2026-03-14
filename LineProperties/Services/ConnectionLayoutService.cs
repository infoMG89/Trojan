using System.Collections.Generic;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace LineProperties.Services;

public static class ConnectionLayoutService
{
    public static string TemplatesPath => ResolveTemplatesPath();

    private static string ResolveTemplatesPath()
    {
        var asmLoc = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly()?.Location) ?? "";
        var candidates = new[]
        {
            Path.Combine(asmLoc, "Templates"),
            Path.Combine(asmLoc, "..", "Templates"),
            Path.Combine(asmLoc, "..", "..", "..", "Templates"),
            @"C:\Users\marti\OneDrive\Documents_2\Cursor\AutoCAD\Templates"
        };
        foreach (var p in candidates)
        {
            var full = Path.GetFullPath(p);
            if (Directory.Exists(full)) return full;
        }
        return Path.GetFullPath(Path.Combine(asmLoc, "..", "Templates"));
    }

    public static string GetTemplatePath(ConnectionType type)
    {
        var fileName = type switch
        {
            ConnectionType.A => "Connection_A_Template.dwg",
            ConnectionType.B => "Connection_B_Template.dwg",
            ConnectionType.C => "Connection_C_Template.dwg",
            _ => "Connection_A_Template.dwg"
        };
        return Path.Combine(TemplatesPath, fileName);
    }

    public static ObjectId? CreateLayoutFromTemplate(Database targetDb, Transaction tr, ConnectionType type,
        string connectionId, string? connectionCode, ObjectId? entityId1, ObjectId? entityId2)
    {
        var templatePath = GetTemplatePath(type);
        if (!File.Exists(templatePath)) return null;

        using var sourceDb = new Database(false, true);
        sourceDb.ReadDwgFile(templatePath, FileOpenMode.OpenForReadAndAllShare, true, "");

        ObjectIdCollection idsToClone;
        using (var sourceTr = sourceDb.TransactionManager.StartTransaction())
        {
            var sourceBt = (BlockTable)sourceTr.GetObject(sourceDb.BlockTableId, OpenMode.ForRead);
            ObjectId? sourceLayoutId = null;
            foreach (ObjectId id in sourceBt)
            {
                if (id == sourceBt[BlockTableRecord.ModelSpace]) continue;
                var btr = (BlockTableRecord)sourceTr.GetObject(id, OpenMode.ForRead);
                if (btr.IsLayout && btr.Name != "Model")
                {
                    sourceLayoutId = id;
                    break;
                }
            }
            if (!sourceLayoutId.HasValue) return null;

            var sourceBtr = (BlockTableRecord)sourceTr.GetObject(sourceLayoutId.Value, OpenMode.ForRead);
            idsToClone = new ObjectIdCollection();
            foreach (ObjectId id in sourceBtr)
                idsToClone.Add(id);
            sourceTr.Commit();
        }

        if (idsToClone.Count == 0) return null;

        var layoutName = $"Connection_{connectionId.Replace("-", "_")}";
        var targetLm = (LayoutManager)LayoutManager.Current;
        try { targetLm.CreateLayout(layoutName); }
        catch { /* layout already exists */ }
        var layoutObjId = targetLm.GetLayoutId(layoutName);
        if (layoutObjId.IsNull) return null;
        var layout = (Layout)tr.GetObject(layoutObjId, OpenMode.ForRead);
        var targetBtr = (BlockTableRecord)tr.GetObject(layout.BlockTableRecordId, OpenMode.ForWrite);

        var idMap = new IdMapping();
        sourceDb.WblockCloneObjects(idsToClone, targetBtr.ObjectId, idMap, DuplicateRecordCloning.MangleName, false);

        ConnectionAttributeService.FillAttributes(targetBtr, tr, connectionId, connectionCode, entityId1, entityId2, type);
        return layout.BlockTableRecordId;
    }

    public static (string LayoutName, int EntityCount, List<string> BlockNames)? CheckTemplate(ConnectionType type)
    {
        var templatePath = GetTemplatePath(type);
        if (!File.Exists(templatePath)) return null;

        using var db = new Database(false, true);
        try { db.ReadDwgFile(templatePath, FileOpenMode.OpenForReadAndAllShare, true, ""); }
        catch { return null; }

        using var tr = db.TransactionManager.StartTransaction();
        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        BlockTableRecord? firstLayout = null;
        foreach (ObjectId id in bt)
        {
            if (id == bt[BlockTableRecord.ModelSpace]) continue;
            var btr = (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead);
            if (btr.IsLayout && btr.Name != "Model")
            {
                firstLayout = btr;
                break;
            }
        }
        if (firstLayout == null) return null;

        int count = 0;
        var blockNames = new List<string>();
        foreach (ObjectId eid in firstLayout)
        {
            count++;
            if (tr.GetObject(eid, OpenMode.ForRead) is BlockReference br)
            {
                var btr = (BlockTableRecord)tr.GetObject(br.BlockTableRecord, OpenMode.ForRead);
                if (!blockNames.Contains(btr.Name))
                    blockNames.Add(btr.Name);
            }
        }
        tr.Commit();
        return (firstLayout.Name, count, blockNames);
    }
}
