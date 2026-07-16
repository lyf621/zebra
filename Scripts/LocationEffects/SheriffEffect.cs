using UnityEngine;

public class SheriffEffect : MonoBehaviour
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

    public void VisitSheriff()
    {
        Turns.AssignMinister();
        Stats.UpdateResource(-1,0,3);
    }
}
