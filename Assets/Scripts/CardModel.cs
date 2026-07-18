using System;

public enum LocationType
{
    Economy,
    Military,
    Administration,
    Diplomacy,
    Any
}

public enum RetainEffectType
{
    None,
    PublicOpinionUp,
    MilitaryStrengthDown
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
    public bool IsRoyal;

    // Temporary reveal-phase economy (copied from CardSO).
    public int MajestyCost;    // Majesty needed to buy this card
    public int MajestyGain;    // Majesty granted when revealed (left unplayed)
    public int FightGain;      // Fight granted when revealed (left unplayed)
}
