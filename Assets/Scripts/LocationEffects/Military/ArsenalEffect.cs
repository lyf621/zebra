 using UnityEngine;

public class ArsenalEffect : MonoBehaviour, EntryCostCheck, ILocationEffectPreview
{
    private ClickOnLocation Clicks; 
    private StatManager Stats;
    private TurnController Turns;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Clicks = transform.GetComponent<ClickOnLocation>();
        Stats = Clicks.VisitStats();
        Turns = Clicks.VisitTurns();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool CanEnter() => Stats != null && Stats.GetGold() >= 4;

    public void VisitTest()
    {
        Turns.AssignMinister();
        GetPreviewEffect().ApplyTo(Stats);
    }

    public StatModifier GetPreviewEffect() => new StatModifier { gold = -4, ms = 3 };
}
