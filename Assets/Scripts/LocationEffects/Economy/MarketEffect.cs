using UnityEngine;

public class MarketEffect : MonoBehaviour
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

    public void VisitTest()
    {
        Turns.AssignMinister();
        Stats.UpdateGold(6);
        Stats.UpdateResource(-1,0,-1);
        Stats.UpdateReputation(0,0,0);
    }
}