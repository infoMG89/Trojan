using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using LineProperties.Data;

namespace LineProperties.Services;

public static class ConnectionAttributeService
{
    private static readonly string[] ConnectionIdTags = { "CONNECTION_ID", "ODKAZ" };
    private static readonly string[] ConnectionCodeTags = { "CONNECTION_CODE", "DRUH_SPOJE", "CODE" };

    public static void FillAttributes(BlockTableRecord layoutBtr, Transaction tr, string connectionId, string? connectionCode,
        ObjectId? entityId1, ObjectId? entityId2, ConnectionType type)
    {
        var dimValues = BuildDimensionValues(tr, entityId1, entityId2, type);

        foreach (ObjectId id in layoutBtr)
        {
            if (tr.GetObject(id, OpenMode.ForRead) is not BlockReference br) continue;
            br.UpgradeOpen();

            var btr = (BlockTableRecord)tr.GetObject(br.BlockTableRecord, OpenMode.ForRead);
            var blockName = btr.Name;

            foreach (ObjectId attId in br.AttributeCollection)
            {
                var attRef = (AttributeReference)tr.GetObject(attId, OpenMode.ForWrite);
                var tag = attRef.Tag;

                if (MatchesTag(tag, ConnectionIdTags))
                    attRef.TextString = connectionId ?? "";
                else if (MatchesTag(tag, ConnectionCodeTags))
                    attRef.TextString = connectionCode ?? "";
                else if (tag == "VALUE")
                {
                    if (dimValues.TryGetValue(blockName, out var val))
                        attRef.TextString = val;
                    else
                    {
                        var baseName = blockName.Contains('$') ? blockName.Split('$').LastOrDefault() ?? blockName : blockName;
                        if (dimValues.TryGetValue(baseName, out val))
                            attRef.TextString = val;
                    }
                }
            }
        }
    }

    private static bool MatchesTag(string tag, string[] allowed)
    {
        foreach (var t in allowed)
            if (string.Equals(tag, t, System.StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    private static Dictionary<string, string> BuildDimensionValues(Transaction tr, ObjectId? id1, ObjectId? id2, ConnectionType type)
    {
        var result = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
        const double InchToMm = 25.4;

        string? GetJoistDim(ObjectId? id, string dimName)
        {
            if (!id.HasValue || !id.Value.IsValid) return null;
            var ent = tr.GetObject(id.Value, OpenMode.ForRead) as Entity;
            if (ent == null) return null;
            var data = MoravioSmartXDataService.Read(ent);
            if (data == null || data.MemberType != MoravioSmartXDataService.MemberTypeJoist) return null;
            var rec = SjiJoistLibrary.GetByDesignation(data.Designation);
            if (rec == null) return null;
            return dimName switch
            {
                "H" => ((int)(rec.DepthInches * InchToMm)).ToString(),
                "B" => ((int)(rec.HalfWidthInches * 2 * InchToMm)).ToString(),
                "TW" => "3",
                "TF" => "5",
                _ => null
            };
        }

        string? GetDeckDim(ObjectId? id, string dimName)
        {
            if (!id.HasValue || !id.Value.IsValid) return null;
            var ent = tr.GetObject(id.Value, OpenMode.ForRead) as Entity;
            if (ent == null) return null;
            var data = MoravioSmartXDataService.Read(ent);
            if (data == null || data.MemberType != MoravioSmartXDataService.MemberTypeDeck) return null;
            var rec = DeckLibrary.GetByDeckType(data.Designation);
            if (rec == null) return null;
            var tMm = rec.Gauge switch { 18 => 1.2, 20 => 0.9, 22 => 0.7, _ => 1.0 };
            return dimName switch
            {
                "L" => null,
                "T" => tMm.ToString("F1"),
                "B1" => ((int)(rec.ProfileDepthInches * InchToMm)).ToString(),
                "B2" => ((int)(rec.ProfileDepthInches * InchToMm)).ToString(),
                _ => null
            };
        }

        if (type == ConnectionType.C)
        {
            AddDim(result, "DIM_H1", GetJoistDim(id1, "H"));
            AddDim(result, "DIM_H2", GetJoistDim(id2, "H"));
            AddDim(result, "DIM_B1", GetJoistDim(id1, "B"));
            AddDim(result, "DIM_B2", GetJoistDim(id2, "B"));
            AddDim(result, "DIM_TW1", GetJoistDim(id1, "TW"));
            AddDim(result, "DIM_TW2", GetJoistDim(id2, "TW"));
            AddDim(result, "DIM_TF1", GetJoistDim(id1, "TF"));
            AddDim(result, "DIM_TF2", GetJoistDim(id2, "TF"));
        }
        else if (type == ConnectionType.B)
        {
            var ent1 = id1.HasValue ? tr.GetObject(id1.Value, OpenMode.ForRead) as Entity : null;
            var isFirstDeck = ent1 != null && MoravioSmartXDataService.Read(ent1)?.MemberType == MoravioSmartXDataService.MemberTypeDeck;
            var joistId = isFirstDeck ? id2 : id1;
            var deckId = isFirstDeck ? id1 : id2;
            AddDim(result, "DIM_H1", GetJoistDim(joistId, "H"));
            AddDim(result, "DIM_L1", GetDeckDim(deckId, "L"));
            AddDim(result, "DIM_B1_1", GetDeckDim(deckId, "B1"));
            AddDim(result, "DIM_B2_1", GetDeckDim(deckId, "B2"));
            AddDim(result, "DIM_T1", GetDeckDim(deckId, "T"));
        }
        else
        {
            AddDim(result, "DIM_L1", GetDeckDim(id1, "L"));
            AddDim(result, "DIM_L2", GetDeckDim(id2, "L"));
            AddDim(result, "DIM_B1_1", GetDeckDim(id1, "B1"));
            AddDim(result, "DIM_B1_2", GetDeckDim(id2, "B1"));
            AddDim(result, "DIM_B2_1", GetDeckDim(id1, "B2"));
            AddDim(result, "DIM_B2_2", GetDeckDim(id2, "B2"));
            AddDim(result, "DIM_T1", GetDeckDim(id1, "T"));
            AddDim(result, "DIM_T2", GetDeckDim(id2, "T"));
        }

        return result;
    }

    private static void AddDim(Dictionary<string, string> dict, string key, string? value)
    {
        if (!string.IsNullOrEmpty(value))
            dict[key] = value;
    }
}
