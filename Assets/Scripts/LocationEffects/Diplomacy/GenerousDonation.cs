 using UnityEngine;

public class GenerousDonation : MonoBehaviour, EntryCostCheck
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

    public bool CanEnter() => Stats != null && Stats.GetGold() >= 3;

    public void VisitTest()
    {
        Turns.AssignMinister();
        Stats.UpdateGold(-3);
        Stats.UpdateResource(1,0,0);
        Stats.UpdateReputation(0,2,0);
    }
}