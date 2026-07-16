using UnityEngine;

public class VillageEffect : MonoBehaviour
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

    public void VisitVillage()
    {
        Turns.AssignMinister();
        Stats.UpdateGold(1);
        Stats.UpdateResource(1,0,0);
    }
}
