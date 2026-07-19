 using UnityEngine;

public class RoyalGrace : MonoBehaviour, EntryCostCheck
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

    public bool CanEnter() => Stats != null;

    public void VisitTest()
    {
        Turns.AssignMinister();
        Stats.UpdateGold(1);
        Stats.UpdateResource(0,0,-1);
        Stats.UpdateReputation(2,0,0);
    }
}