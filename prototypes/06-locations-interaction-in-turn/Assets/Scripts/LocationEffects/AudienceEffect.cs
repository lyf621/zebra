using UnityEngine;

public class AudienceEffect : MonoBehaviour
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

    public void VisitAudience()
    {
        Turns.AssignMinister();
        Stats.UpdateReputation(1,0,1);
    }
}
