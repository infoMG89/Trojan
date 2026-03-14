using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace LineProperties.Services;

public static class ConnectionIdService
{
    private const string DictName = "CONNECTION";
    private const string NextIdRecord = "NextConnectionId";
    private const int DxfLong = 1071;

    public static string GetNextConnectionId(Database db, Transaction tr)
    {
        int next = GetAndIncrement(db, tr);
        return $"CONN-{next}";
    }

    public static int ParseConnectionIdNumber(string connectionId)
    {
        if (string.IsNullOrEmpty(connectionId)) return 0;
        var parts = connectionId.Split('-');
        if (parts.Length >= 2 && int.TryParse(parts[1], out int n))
            return n;
        return 0;
    }

    private static int GetAndIncrement(Database db, Transaction tr)
    {
        var nod = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);
        nod.UpgradeOpen();
        if (!nod.Contains(DictName))
        {
            var dict = new DBDictionary();
            nod.SetAt(DictName, dict);
            tr.AddNewlyCreatedDBObject(dict, true);
            var xrec = new Xrecord();
            xrec.Data = new ResultBuffer(new TypedValue(DxfLong, 1));
            dict.SetAt(NextIdRecord, xrec);
            tr.AddNewlyCreatedDBObject(xrec, true);
            return 1;
        }
        var dictObj = (DBDictionary)tr.GetObject(nod.GetAt(DictName), OpenMode.ForRead);
        dictObj.UpgradeOpen();
        if (!dictObj.Contains(NextIdRecord))
        {
            var xrec = new Xrecord();
            xrec.Data = new ResultBuffer(new TypedValue(DxfLong, 1));
            dictObj.SetAt(NextIdRecord, xrec);
            tr.AddNewlyCreatedDBObject(xrec, true);
            return 1;
        }
        var xrecObj = (Xrecord)tr.GetObject(dictObj.GetAt(NextIdRecord), OpenMode.ForRead);
        var data = xrecObj.Data;
        int val = 1;
        if (data != null)
            foreach (TypedValue tv in data)
                if (tv.TypeCode == DxfLong && tv.Value is int n) { val = n; break; }
        xrecObj.UpgradeOpen();
        xrecObj.Data = new ResultBuffer(new TypedValue(DxfLong, val + 1));
        return val;
    }
}
