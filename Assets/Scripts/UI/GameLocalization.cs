// Shared labels for stat changes and map locations. Runtime panels use this instead of
// each keeping a partially translated copy of the same strings.
public static class GameLocalization
{
    public static string FormatStatChanges(StatModifier effect, bool chinese, string indent = "")
    {
        if (chinese)
        {
            return indent + "金币 " + Signed(effect.gold)
                 + "   民意 " + Signed(effect.po)
                 + "   军力 " + Signed(effect.ms)
                 + "   权威 " + Signed(effect.al)
                 + "   王室 " + Signed(effect.kr)
                 + "   教会 " + Signed(effect.cr)
                 + "   大贵族 " + Signed(effect.ar);
        }

        return indent + "Gold " + Signed(effect.gold)
             + "   PO " + Signed(effect.po)
             + "   MS " + Signed(effect.ms)
             + "   AL " + Signed(effect.al)
             + "   KR " + Signed(effect.kr)
             + "   CR " + Signed(effect.cr)
             + "   AR " + Signed(effect.ar);
    }

    public static string GetLocationName(string english)
    {
        switch (english)
        {
            case "Royal Grace": return "皇室恩典";
            case "RoyalGrace": return "皇室恩典";
            case "Bureaucracy": return "官僚机构";
            case "Farm": return "农庄";
            case "Barrack": return "兵营";
            case "Generous Donation": return "慷慨捐赠";
            case "GenerousDonation": return "慷慨捐赠";
            case "Ceremony": return "仪式";
            case "Guild": return "行会";
            case "Arsenal": return "军械库";
            case "Alliance": return "结盟";
            case "Patrol": return "巡逻";
            case "Market": return "市场";
            case "Mobilization": return "动员";
            default: return english;
        }
    }

    public static string LocalizeStatTokens(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Replace("Gold", "金币")
                    .Replace("PO", "民意")
                    .Replace("MS", "军力")
                    .Replace("AL", "权威")
                    .Replace("KR", "王室")
                    .Replace("CR", "教会")
                    .Replace("AR", "大贵族");
    }

    private static string Signed(int value) => (value > 0 ? "+" : "") + value;
}
