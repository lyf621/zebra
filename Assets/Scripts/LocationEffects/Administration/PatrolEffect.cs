 using UnityEngine;

public class PatrolEffect : MonoBehaviour, EntryCostCheck
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

    public bool CanEnter() => Stats != null && Stats.GetMS() >= 0;

    public void VisitTest()
    {
        Turns.AssignMinister();
        Stats.UpdateGold(0);
        Stats.UpdateResource(-1,-1,3);
        Stats.UpdateReputation(0,0,0);
    }
}