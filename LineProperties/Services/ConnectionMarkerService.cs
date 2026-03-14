using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace LineProperties.Services;

public static class ConnectionMarkerService
{
    public const string RegAppName = "CONNECTION";
    private const int DxfString = 1000;
    private const int DxfLong = 1071;

    public static void EnsureRegApp(Database db, Transaction tr)
    {
        var rat = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead);
        if (rat.Has(RegAppName)) return;
        rat.UpgradeOpen();
        using var rar = new RegAppTableRecord();
        rar.Name = RegAppName;
        rat.Add(rar);
        tr.AddNewlyCreatedDBObject(rar, true);
    }

    public static void SetMarkerData(Entity entity, string handle1, string handle2, ConnectionType type, string connectionId, Database db, Transaction tr)
    {
        EnsureRegApp(db, tr);
        using var rb = new ResultBuffer(
            new TypedValue((int)DxfCode.ExtendedDataRegAppName, RegAppName),
            new TypedValue(DxfString, handle1),
            new TypedValue(DxfString, handle2),
            new TypedValue(DxfString, type.ToString()),
            new TypedValue(DxfString, connectionId));
        entity.XData = rb;
    }

    public static (string Handle1, string Handle2, ConnectionType Type, string ConnectionId)? GetMarkerData(Entity entity)
    {
        var data = entity.XData;
        if (data == null) return null;
        var values = new List<string>();
        bool inGroup = false;
        foreach (TypedValue tv in data)
        {
            if (tv.TypeCode == (int)DxfCode.ExtendedDataRegAppName && tv.Value is string s && s == RegAppName)
            { inGroup = true; continue; }
            if (inGroup && tv.TypeCode == DxfString && tv.Value is string v)
                values.Add(v);
        }
        if (values.Count < 4) return null;
        if (!Enum.TryParse<ConnectionType>(values[2], out var type)) return null;
        return (values[0], values[1], type, values[3]);
    }

    public static string? GetChosenCode(Entity entity)
    {
        var data = entity.XData;
        if (data == null) return null;
        var values = new List<string>();
        bool inGroup = false;
        foreach (TypedValue tv in data)
        {
            if (tv.TypeCode == (int)DxfCode.ExtendedDataRegAppName && tv.Value is string s && s == RegAppName)
            { inGroup = true; continue; }
            if (inGroup && tv.TypeCode == DxfString && tv.Value is string v)
                values.Add(v);
        }
        return values.Count >= 5 ? values[4] : null;
    }

    public static void SetChosenCode(Entity entity, string code, Database db, Transaction tr)
    {
        var existing = GetMarkerData(entity);
        if (existing == null) return;
        EnsureRegApp(db, tr);
        using var rb = new ResultBuffer(
            new TypedValue((int)DxfCode.ExtendedDataRegAppName, RegAppName),
            new TypedValue(DxfString, existing.Value.Handle1),
            new TypedValue(DxfString, existing.Value.Handle2),
            new TypedValue(DxfString, existing.Value.Type.ToString()),
            new TypedValue(DxfString, existing.Value.ConnectionId),
            new TypedValue(DxfString, code ?? ""));
        entity.XData = rb;
    }

    public static ObjectId? GetObjectIdByHandleString(Database db, string handleStr)
    {
        if (string.IsNullOrEmpty(handleStr)) return null;
        if (!long.TryParse(handleStr, System.Globalization.NumberStyles.HexNumber, null, out long handleVal))
            return null;
        var handle = new Handle(handleVal);
        return db.GetObjectId(false, handle, 0);
    }
}
