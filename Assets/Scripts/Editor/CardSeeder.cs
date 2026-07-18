#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

// One-click generator for the project's original card set as CardSO assets.
// Run from the top menu: Game > Create Default Cards. Assets are written to Assets/Cards/.
// Existing assets with the same name are left untouched, so it is safe to re-run.
public static class CardSeeder
{
    private const string FolderPath = "Assets/Cards";

    [MenuItem("Game/Create Default Cards")]
    public static void CreateDefaultCards()
    {
        if (!AssetDatabase.IsValidFolder(FolderPath))
            AssetDatabase.CreateFolder("Assets", "Cards");

        // ---- Starting deck ----
        Make("VillageCharter", "Village Charter", "村庄宪章", "Place a minister in Economy.", "在经济地点放置一名大臣。", LocationType.Economy, RetainEffectType.None, false);
        Make("HarvestPlan", "Harvest Plan", "收获计划", "Place a minister in Economy.", "在经济地点放置一名大臣。", LocationType.Economy, RetainEffectType.None, false);
        Make("BarracksOrder", "Barracks Order", "兵营命令", "Place a minister in Military.", "在军事地点放置一名大臣。", LocationType.Military, RetainEffectType.None, false);
        Make("DrillSchedule", "Drill Schedule", "训练日程", "Place a minister in Military.", "在军事地点放置一名大臣。", LocationType.Military, RetainEffectType.None, false);
        Make("RoyalSeal", "Royal Seal", "皇家印章", "Place a minister in Administration.", "在行政地点放置一名大臣。", LocationType.Administration, RetainEffectType.None, false);
        Make("ClerksReport", "Clerk's Report", "书记官报告", "Place a minister in Administration.", "在行政地点放置一名大臣。", LocationType.Administration, RetainEffectType.None, false);
        Make("PublicFeast", "Public Feast", "公众宴会", "If retained: PO +1.", "保留时：民意 +1。", LocationType.Economy, RetainEffectType.PublicOpinionUp, false);
        Make("ForcedLevy", "Forced Levy", "强制征募", "If retained: MS -1.", "保留时：军力 -1。", LocationType.Military, RetainEffectType.MilitaryStrengthDown, false);
        Make("TaxLedger", "Tax Ledger", "税务账册", "Place a minister in Economy.", "在经济地点放置一名大臣。", LocationType.Economy, RetainEffectType.None, false);
        Make("CouncilPetition", "Council Petition", "议会请愿", "Place a minister in Administration.", "在行政地点放置一名大臣。", LocationType.Administration, RetainEffectType.None, false);

        // ---- Market (royal) cards ----
        Make("RoyalDecree", "Royal Decree", "皇家法令", "May use any location.", "可以进入任意地点。", LocationType.Any, RetainEffectType.None, true);
        Make("CrownLevy", "Crown Levy", "王室征税", "Military order from the crown.", "来自王室的军事命令。", LocationType.Military, RetainEffectType.None, true);
        Make("RoyalPardon", "Royal Pardon", "皇家赦免", "Administrative order from the crown.", "来自王室的行政命令。", LocationType.Administration, RetainEffectType.None, true);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CardSeeder: default cards created in " + FolderPath +
                  ". Assign the starting cards to ZebraGameController.startingDeck and the royal cards to marketCards.");
    }

    private static void Make(string fileName, string en, string zh, string descEn, string descZh,
                             LocationType loc, RetainEffectType retain, bool royal)
    {
        string path = FolderPath + "/" + fileName + ".asset";
        if (AssetDatabase.LoadAssetAtPath<CardSO>(path) != null) return;   // don't overwrite existing edits

        CardSO card = ScriptableObject.CreateInstance<CardSO>();
        card.NameEnglish = en;
        card.NameChinese = zh;
        card.DescriptionEnglish = descEn;
        card.DescriptionChinese = descZh;
        card.Location = loc;
        card.RetainEffect = retain;
        card.IsRoyal = royal;
        AssetDatabase.CreateAsset(card, path);
    }
}
#endif
