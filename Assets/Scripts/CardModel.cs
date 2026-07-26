using System;

public enum LocationType
{
    Economy,
    Military,
    Administration,
    Diplomacy,
    Any
}

// Serialized by index in the CardSO assets, so new members must be APPENDED — inserting one
// would silently reassign the retain effect of every card authored after it.
public enum RetainEffectType
{
    None,
    PublicOpinionUp,
    MilitaryStrengthDown,
    GoldUp
}

// One-shot policies activated only when the card is played on a matching district.
public enum PermanentCardEffectType
{
    None,
    EconomyPublicOpinion,
    MilitaryStrength,
    AdministrationAuthority,
    DiplomacyKingReputation,
    DiplomacyChurchReputation,
    DiplomacyAristocratReputation
}

[Serializable]
public class CardModel
{
    public int InstanceId;
    public string NameEnglish;
    public string NameChinese;
    public string DescriptionEnglish;
    public string DescriptionChinese;
    public LocationType Location;
    public RetainEffectType RetainEffect;
    public PermanentCardEffectType PermanentEffect;
    public bool IsRoyal;

    // Temporary reveal-phase economy (copied from CardSO).
    public int MajestyCost;    // Majesty needed to buy this card
    public int MajestyGain;    // Majesty granted when revealed (left unplayed)
    public int FightGain;      // Fight granted when revealed (left unplayed)
}
