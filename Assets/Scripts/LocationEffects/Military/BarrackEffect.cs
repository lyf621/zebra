 using UnityEngine;

public class BarrackEffect : MonoBehaviour, EntryCostCheck, ILocationEffectPreview
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

    public bool CanEnter() => Stats != null && Stats.GetAL() >= 0;

    public void VisitTest()
    {
        Turns.AssignMinister();
        GetPreviewEffect().ApplyTo(Stats);
    }

    public StatModifier GetPreviewEffect() => new StatModifier { ms = 2, al = -1 };
}
