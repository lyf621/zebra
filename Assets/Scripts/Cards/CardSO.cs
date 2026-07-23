using UnityEngine;

// Authoring asset for a card. Create via Assets > Create > Game > Card (like EventSO / MissionSO).
// At runtime ZebraGameController copies these into CardModel instances (so duplicates of the
// same card each get their own InstanceId and can move between piles independently).
[CreateAssetMenu(fileName = "NewCard", menuName = "Game/Card")]
public class CardSO : ScriptableObject
{
    [Header("Names")]
    public string NameEnglish;
    public string NameChinese;

    [Header("Descriptions")]
    [TextArea] public string DescriptionEnglish;
    [TextArea] public string DescriptionChinese;

    [Header("Rules")]
    public LocationType Location = LocationType.Any;                 // which locations this card may be played on
    public RetainEffectType RetainEffect = RetainEffectType.None;    // effect applied if left unplayed (revealed)
    public PermanentCardEffectType PermanentEffect = PermanentCardEffectType.None; // one-shot policy when played
    public bool IsRoyal;                                             // royal styling / market card

    [Header("Majesty / Fight (temporary reveal-phase resources)")]
    public int MajestyCost;    // Majesty needed to buy this card in Phase 2 (royal / market cards)
    public int MajestyGain;    // Majesty granted if this card is left unplayed (revealed)
    public int FightGain;      // Fight granted if this card is left unplayed (revealed)
}
