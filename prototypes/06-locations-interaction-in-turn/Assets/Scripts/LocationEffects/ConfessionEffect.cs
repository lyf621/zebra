using UnityEngine;

public class ConfessionEffect : MonoBehaviour
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

    public void VisitConfession()
    {
        if(Stats.GetGold() < 1) {
            transform.parent.GetComponent<OneClick2DObject>().ResetObject();
            return;
        }
        Turns.AssignMinister();
        Stats.UpdateGold(-1);
        Stats.UpdateReputation(0,2,0);
    }
}
