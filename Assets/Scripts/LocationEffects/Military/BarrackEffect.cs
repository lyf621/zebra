 using UnityEngine;

public class BarrackEffect : MonoBehaviour, EntryCostCheck
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
        Stats.UpdateGold(0);
        Stats.UpdateResource(0,2,-1);
        Stats.UpdateReputation(0,0,0);
    }
}