using UnityEngine;

public class RaidEffect : MonoBehaviour
{
    [SerializeField] private TurnController Turns;
    [SerializeField] private StatManager Stats;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void VisitRaid()
    {
        if(Stats.GetMS() <= 6) {
            transform.parent.GetComponent<OneClick2DObject>().ResetObject();
            return;
        }
        Turns.AssignMinister();
        Stats.UpdateGold(5);
        Stats.UpdateReputation(0,0,-2);
    }
}
