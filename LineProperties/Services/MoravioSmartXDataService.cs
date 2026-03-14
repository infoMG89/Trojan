using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace LineProperties.Services;

/// <summary>
/// Reads and writes MORAVIO_SMART XData on entities (joists, beams, deck).
/// Schema: MemberType, Mark, Designation, ExtensionLeft/Right, CantileverLeft/Right,
/// ShoeLeft/Right, BridgingLeft/Right, IsTieJoist, ActiveLoads, SequenceZone,
/// SpanLength, Depth, Weight.
/// </summary>
public static class MoravioSmartXDataService
{
    public const string RegAppName = "MORAVIO_SMART";

    public const string MemberTypeJoist = "JOIST";
    public const string MemberTypeBeam = "BEAM";
    public const string MemberTypeDeck = "DECK";

    private const int DxfString = 1000;
    private const int DxfReal = 1040;
    private const int DxfLong = 1071;

    /// <summary>Registers the RegApp "MORAVIO_SMART" if not already present.</summary>
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

    /// <summary>Returns true if entity has MORAVIO_SMART XData.</summary>
    public static bool HasMoravioSmartData(Entity entity)
    {
        var data = entity.XData;
        if (data == null) return false;
        foreach (TypedValue tv in data)
        {
            if (tv.TypeCode == (int)DxfCode.ExtendedDataRegAppName && tv.Value is string s &&
                string.Equals(s, RegAppName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>Reads MORAVIO_SMART data from entity XData, or null if not present.</summary>
    public static MoravioSmartData? Read(Entity entity)
    {
        var data = entity.XData;
        if (data == null) return null;

        var values = new List<TypedValue>();
        bool inGroup = false;
        foreach (TypedValue tv in data)
        {
            if (tv.TypeCode == (int)DxfCode.ExtendedDataRegAppName && tv.Value is string s &&
                string.Equals(s, RegAppName, StringComparison.OrdinalIgnoreCase))
            {
                inGroup = true;
                continue;
            }
            if (inGroup) values.Add(tv);
        }

        if (values.Count < 17) return null;

        int idx = 0;
        string GetStr()
        {
            if (idx >= values.Count) return "";
            var tv = values[idx++];
            return tv.TypeCode == DxfString && tv.Value is string s ? s : "";
        }
        double GetReal()
        {
            if (idx >= values.Count) return 0.0;
            var tv = values[idx++];
            return tv.TypeCode == DxfReal && tv.Value is double d ? d : 0.0;
        }
        int GetLong()
        {
            if (idx >= values.Count) return 0;
            var tv = values[idx++];
            return tv.TypeCode == DxfLong && tv.Value is int n ? n : 0;
        }
        return new MoravioSmartData
        {
            MemberType = GetStr(),
            Mark = GetStr(),
            Designation = GetStr(),
            ExtensionLeft = GetReal(),
            ExtensionRight = GetReal(),
            CantileverLeft = GetReal(),
            CantileverRight = GetReal(),
            ShoeLeft = GetStr(),
            ShoeRight = GetStr(),
            BridgingLeft = GetStr(),
            BridgingRight = GetStr(),
            IsTieJoist = GetLong() != 0,
            ActiveLoads = GetReal(),
            SequenceZone = GetStr(),
            SpanLength = GetReal(),
            Depth = GetReal(),
            Weight = GetReal()
        };
    }

    /// <summary>Writes MORAVIO_SMART data to entity XData. Entity must be open for write.</summary>
    public static void Write(Entity entity, MoravioSmartData data, Database db, Transaction tr)
    {
        EnsureRegApp(db, tr);
        using var rb = new ResultBuffer(
            new TypedValue((int)DxfCode.ExtendedDataRegAppName, RegAppName),
            new TypedValue(DxfString, data.MemberType ?? ""),
            new TypedValue(DxfString, data.Mark ?? ""),
            new TypedValue(DxfString, data.Designation ?? ""),
            new TypedValue(DxfReal, data.ExtensionLeft),
            new TypedValue(DxfReal, data.ExtensionRight),
            new TypedValue(DxfReal, data.CantileverLeft),
            new TypedValue(DxfReal, data.CantileverRight),
            new TypedValue(DxfString, data.ShoeLeft ?? ""),
            new TypedValue(DxfString, data.ShoeRight ?? ""),
            new TypedValue(DxfString, data.BridgingLeft ?? ""),
            new TypedValue(DxfString, data.BridgingRight ?? ""),
            new TypedValue(DxfLong, data.IsTieJoist ? 1 : 0),
            new TypedValue(DxfReal, data.ActiveLoads),
            new TypedValue(DxfString, data.SequenceZone ?? ""),
            new TypedValue(DxfReal, data.SpanLength),
            new TypedValue(DxfReal, data.Depth),
            new TypedValue(DxfReal, data.Weight));
        entity.XData = rb;
    }

    private const string DictName = "MORAVIO_SMART";
    private const string NextJoistMarkRecord = "NextJoistMark";
    private const string NextTieJoistMarkRecord = "NextTieJoistMark";

    /// <summary>Returns the next joist mark (J1, J2, ...) and increments the counter.</summary>
    public static string GetNextJoistMark(Database db, Transaction tr)
    {
        int next = GetAndIncrementCounter(db, tr, NextJoistMarkRecord);
        return $"J{next}";
    }

    /// <summary>Returns the next tie joist mark (TJ1, TJ2, ...) and increments the counter.</summary>
    public static string GetNextTieJoistMark(Database db, Transaction tr)
    {
        int next = GetAndIncrementCounter(db, tr, NextTieJoistMarkRecord);
        return $"TJ{next}";
    }

    private static int GetAndIncrementCounter(Database db, Transaction tr, string recordName)
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
            dict.SetAt(recordName, xrec);
            tr.AddNewlyCreatedDBObject(xrec, true);
            return 1;
        }
        var dictObj = (DBDictionary)tr.GetObject(nod.GetAt(DictName), OpenMode.ForRead);
        dictObj.UpgradeOpen();
        if (!dictObj.Contains(recordName))
        {
            var xrec = new Xrecord();
            xrec.Data = new ResultBuffer(new TypedValue(DxfLong, 1));
            dictObj.SetAt(recordName, xrec);
            tr.AddNewlyCreatedDBObject(xrec, true);
            return 1;
        }
        var xrecObj = (Xrecord)tr.GetObject(dictObj.GetAt(recordName), OpenMode.ForRead);
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

/// <summary>Data structure for MORAVIO_SMART XData.</summary>
public record MoravioSmartData
{
    public string MemberType { get; init; } = MoravioSmartXDataService.MemberTypeJoist;
    public string Mark { get; init; } = "";
    public string Designation { get; init; } = "";
    public double ExtensionLeft { get; init; }
    public double ExtensionRight { get; init; }
    public double CantileverLeft { get; init; }
    public double CantileverRight { get; init; }
    public string ShoeLeft { get; init; } = "STD";
    public string ShoeRight { get; init; } = "STD";
    public string BridgingLeft { get; init; } = "";
    public string BridgingRight { get; init; } = "";
    public bool IsTieJoist { get; init; }
    public double ActiveLoads { get; init; }
    public string SequenceZone { get; init; } = "";
    public double SpanLength { get; init; }
    public double Depth { get; init; }
    public double Weight { get; init; }
}
