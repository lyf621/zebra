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
    /*[SerializeField] private TMP_Text ShowTurn;
    [SerializeField] private TMP_Text ShowWorker;*/
    [SerializeField] private TMP_Text currentTurnPhase;
    [SerializeField] private StatManager Stats;

    private int TurnPhase = 0;
    private int MinistersLeft;
    public ClickOnLocation[] LocationList;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TurnCount = 1;
        MinistersLeft = MaxMinisters;
    }

    // Update is called once per frame
    void Update()
    {
        /*ShowTurn.text = "Turn: " + TurnCount;
        ShowWorker.text = "Available Ministers: " + MinistersLeft;*/
        currentTurnPhase.text = UpdateTurnPhase();

        if(MinistersLeft == 0) {
            MovesRunOut();
        }
    }

    public void AssignMinister() { MinistersLeft --; }
    public void RestoreMinister() { MinistersLeft = MaxMinisters; }

    // --- Getters so integrated systems (e.g. the card system) can defer to this controller
    //     instead of tracking their own minister / turn counters. ---
    public int GetMinistersLeft() { return MinistersLeft; }
    public int GetMaxMinisters() { return MaxMinisters; }
    public bool HasMinisterAvailable() { return MinistersLeft > 0; }
    public int GetTurnCount() { return TurnCount; }

    public void EndTurn()
    {
        Stats.ReturnToBalance();
        Stats.ClearTurnStat();
        RestoreMinister();
        TurnCount++;
        foreach (var tile in LocationList)
            tile.ResetObject();
        TurnPhase = 0;
    }

    public void MovesRunOut()
    {
        foreach (var tile in LocationList)
            tile.DisableObject();
    }

    public void NextTurnPhase() { TurnPhase ++; }
    public int CheckTurnPhase() { return TurnPhase; }

    public string UpdateTurnPhase()
    {
        if(TurnPhase == 0) return "Next:Event";
        if(TurnPhase == 1) return "Next:Reveal";
        if(TurnPhase == 2) return "Next:Mission";
        if(TurnPhase == 3) return "EndTurn";
        return "???";
    }
}
