using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RoyalPolicyCardSetup
{
    private const string CardFolder = "Assets/Scripts/Cards/RoyalCards";
    private const string MainMapPath = "Assets/Scenes/MainMap.unity";

    private readonly struct Definition
    {
        public readonly string FileName;
        public readonly string EnglishName;
        public readonly string ChineseName;
        public readonly string EnglishDescription;
        public readonly string ChineseDescription;
        public readonly LocationType Location;
        public readonly PermanentCardEffectType Effect;
        public readonly int MajestyGain;
        public readonly int FightGain;

        public Definition(string fileName, string englishName, string chineseName, string englishDescription, string chineseDescription, LocationType location, PermanentCardEffectType effect, int majestyGain, int fightGain)
        {
            FileName = fileName;
            EnglishName = englishName;
            ChineseName = chineseName;
            EnglishDescription = englishDescription;
            ChineseDescription = chineseDescription;
            Location = location;
            Effect = effect;
            MajestyGain = majestyGain;
            FightGain = fightGain;
        }
    }

    private static readonly Definition[] Definitions =
    {
        new Definition("EconomicSpecialZone", "Economic Special Zone", "经济特区", "PO +1. Consume.", "使一个地块永久获得民意 +1。\n消耗。", LocationType.Economy, PermanentCardEffectType.EconomyPublicOpinion, 1, 0),
        new Definition("MilitaryExpansion", "Military Expansion", "军备扩张", "MS +1. Consume.", "使一个地块永久获得军力 +1。\n消耗。", LocationType.Military, PermanentCardEffectType.MilitaryStrength, 0, 2),
        new Definition("AdministrativeReform", "Administrative Reform", "行政改制", "AL +1. Consume.", "使一个地块永久获得权威 +1。\n消耗。", LocationType.Administration, PermanentCardEffectType.AdministrationAuthority, 1, 0),
        new Definition("RoyalDelegation", "Royal Delegation", "王室使团", "King reputation +1. Royal Grace only. Consume.", "使地块永久获得王室声望 +1。\n仅可打出至皇室恩典。消耗。", LocationType.Diplomacy, PermanentCardEffectType.DiplomacyKingReputation, 1, 0),
        new Definition("ChurchDelegation", "Church Delegation", "教会使团", "Church reputation +1. Donation only. Consume.", "使地块永久获得教会声望 +1。\n仅可打出至慷慨捐赠。消耗。", LocationType.Diplomacy, PermanentCardEffectType.DiplomacyChurchReputation, 1, 0),
        new Definition("AristocratDelegation", "Aristocrat Delegation", "贵族使团", "Aristocrat reputation +1. Alliance only. Consume.", "使地块永久获得大贵族声望 +1。\n仅可打出至结盟。\n消耗。", LocationType.Diplomacy, PermanentCardEffectType.DiplomacyAristocratReputation, 1, 0)
    };

    [MenuItem("Zebra/Create or Update Royal Policy Cards")]
    public static void Apply()
    {
        List<CardSO> cards = new List<CardSO>();
        foreach (Definition definition in Definitions)
        {
            cards.Add(CreateOrUpdate(definition));
        }

        AddCardsToMainMapMarket(cards);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Zebra: royal policy cards created and added to MainMap market.");
    }

    private static CardSO CreateOrUpdate(Definition definition)
    {
        string path = CardFolder + "/" + definition.FileName + ".asset";
        CardSO card = AssetDatabase.LoadAssetAtPath<CardSO>(path);
        if (card == null)
        {
            card = ScriptableObject.CreateInstance<CardSO>();
            AssetDatabase.CreateAsset(card, path);
        }

        card.NameEnglish = definition.EnglishName;
        card.NameChinese = definition.ChineseName;
        card.DescriptionEnglish = definition.EnglishDescription;
        card.DescriptionChinese = definition.ChineseDescription;
        card.Location = definition.Location;
        card.RetainEffect = RetainEffectType.None;
        card.PermanentEffect = definition.Effect;
        card.IsRoyal = true;
        card.MajestyCost = 6;
        card.MajestyGain = definition.MajestyGain;
        card.FightGain = definition.FightGain;
        EditorUtility.SetDirty(card);
        return card;
    }

    private static void AddCardsToMainMapMarket(List<CardSO> cards)
    {
        Scene scene = EditorSceneManager.OpenScene(MainMapPath, OpenSceneMode.Single);
        ZebraGameController controller = Object.FindAnyObjectByType<ZebraGameController>();
        if (controller == null)
        {
            throw new System.InvalidOperationException("ZebraGameController was not found in MainMap.");
        }

        SerializedObject serializedController = new SerializedObject(controller);
        SerializedProperty marketCards = serializedController.FindProperty("marketCards");
        foreach (CardSO card in cards)
        {
            if (Contains(marketCards, card))
            {
                continue;
            }

            int index = marketCards.arraySize;
            marketCards.InsertArrayElementAtIndex(index);
            marketCards.GetArrayElementAtIndex(index).objectReferenceValue = card;
        }

        serializedController.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static bool Contains(SerializedProperty cards, CardSO candidate)
    {
        for (int i = 0; i < cards.arraySize; i++)
        {
            if (cards.GetArrayElementAtIndex(i).objectReferenceValue == candidate)
            {
                return true;
            }
        }
        return false;
    }
}
