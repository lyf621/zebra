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
    GoldUp,
    ReputationUp
}

// Effects that fire the moment a card is played on a location, regardless of which one.
// Distinct from PermanentCardEffectType, which stations a lasting policy at that district.
// Serialized by index in the CardSO assets, so new members must be APPENDED.
public enum PlayEffectType
{
    None,
    Draw2Cards
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
    public PlayEffectType PlayEffect;
    public PermanentCardEffectType PermanentEffect;
    public bool IsRoyal;

    // Temporary reveal-phase economy (copied from CardSO).
    public int MajestyCost;    // Majesty needed to buy this card
    public int MajestyGain;    // Majesty granted when revealed (left unplayed)
    public int FightGain;      // Fight granted when revealed (left unplayed)
}
