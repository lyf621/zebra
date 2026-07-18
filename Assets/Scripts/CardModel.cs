using System;

public enum LocationType
{
    Economy,
    Military,
    Administration,
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
}
