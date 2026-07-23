using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class TurnController : MonoBehaviour
{
    [SerializeField] private int TurnCount = 0;
    [SerializeField] private int MaxTurns = 10;
    [SerializeField] private int MaxMinisters = 2;
    /*[SerializeField] private TMP_Text ShowTurn;
    [SerializeField] private TMP_Text ShowWorker;*/
    [SerializeField] private TMP_Text currentTurnPhase;
    [SerializeField] private StatManager Stats;
    [SerializeField] private GameEndingController Ending;

    private int TurnPhase = 0;
    private int MinistersLeft;
    private bool GameEnded = false;
    private ZebraGameController Cards;
    public ClickOnLocation[] LocationList;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Stats == null) Stats = FindAnyObjectByType<StatManager>();
        if (Ending == null) Ending = GameEndingController.EnsureExists();
        MainMapUIController.EnsureExists();
        DecisionReviewController.EnsureExists();
        Cards = FindAnyObjectByType<ZebraGameController>();
        RegisterSceneLocations();
        if (!GameSessionSettings.HasSelectedDifficulty && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMap")
        {
            gameObject.AddComponent<DifficultySelectionBootstrap>();
        }
        TurnCount = 1;
        MinistersLeft = MaxMinisters;
        // LocationArtController.EnsureExists();   // 已停用地点美术功能
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
    public int GetMaxTurnCount() { return MaxTurns; }
    public bool IsGameOver() { return GameEnded; }
    public void SetLocations(ClickOnLocation[] locations)
    {
        LocationList = locations;
        if (Cards == null) Cards = FindAnyObjectByType<ZebraGameController>();
        if (Cards != null) Cards.SetLocations(LocationList);
    }

    // 自动收集队友在场景中摆放的地块，不改变它们的位置、数量或外观。
    private void RegisterSceneLocations()
    {
        LocationList = FindObjectsByType<ClickOnLocation>(FindObjectsSortMode.None);
        if (Cards != null) Cards.SetLocations(LocationList);
    }

    // 结束当前回合；第十回合直接结算胜负，其他回合继续执行原有重置流程。
    public void EndTurn()
    {
        if (GameEnded) return;
        if (TurnCount >= MaxTurns)
        {
            GameEnded = true;
            if (Ending == null) Ending = GameEndingController.EnsureExists();
            Ending.ShowEnding(Stats);
            return;
        }

        Stats.ReturnToBalance(GameSessionSettings.BalanceLowerBound, GameSessionSettings.BalanceUpperBound);
        Stats.ClearTurnStat();
        RestoreMinister();
        TurnCount++;
        if (LocationList != null) foreach (var tile in LocationList) if (tile != null) tile.ResetObject();
        TurnPhase = 0;
    }

    public void MovesRunOut()
    {
        if (LocationList != null) foreach (var tile in LocationList) if (tile != null) tile.DisableObject();
    }

    public void NextTurnPhase() { TurnPhase ++; }
    public int CheckTurnPhase() { return TurnPhase; }

    public string UpdateTurnPhase()
    {
        bool chinese = Cards != null && Cards.UseChinese;
        if (GameEnded) return chinese ? "游戏结束" : "Game Over";
        if(TurnPhase == 0) return chinese ? "开始事件" : "Begin Event";
        if(TurnPhase == 1) return chinese ? "结束出牌并揭示" : "Reveal Hand";
        if(TurnPhase == 2) return chinese ? "结束买牌并处理任务" : "Resolve Mission";
        if(TurnPhase == 3) return chinese ? "结束回合" : "End Turn";
        return "???";
    }
}
