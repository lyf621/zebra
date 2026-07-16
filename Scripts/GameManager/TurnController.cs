using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class TurnController : MonoBehaviour
{
    [SerializeField] private int TurnCount = 0;
    [SerializeField] private int MaxMinisters = 2;
    [SerializeField] private TMP_Text ShowTurn;
    [SerializeField] private TMP_Text ShowWorker;
    [SerializeField] private StatManager Stats;
    private int MinistersLeft;
    public OneClick2DObject[] allTiles;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TurnCount = 1;
        MinistersLeft = MaxMinisters;
    }

    // Update is called once per frame
    void Update()
    {
        ShowTurn.text = "Turn: " + TurnCount;
        ShowWorker.text = "Available Ministers: " + MinistersLeft;

        if(MinistersLeft == 0) {
            MovesRunOut();
        }
    }

    public void AssignMinister() { MinistersLeft --; }
    public void RestoreMinister() { MinistersLeft = MaxMinisters; }

    public void EndTurn()
    {
        Stats.ReturnToBalance();
        RestoreMinister();
        TurnCount++;
        foreach (var tile in allTiles)
            tile.ResetObject();
    }

    public void MovesRunOut()
    {
        foreach (var tile in allTiles)
            tile.DisableObject();
    }
}
