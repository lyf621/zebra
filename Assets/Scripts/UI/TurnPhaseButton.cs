using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Single button that drives the turn phases.
///   Phase 0 -> the turn's hand is already drawn (visible); handle the event and get a mission.
///              Card play unlocks once the event is resolved.
///   Phase 1 -> play cards; click to reveal the "reveal" effects of leftover cards and go to Phase 2.
///   Phase 2 -> buy / add new cards to the deck; click to go to Phase 3.
///   Phase 3 -> handle the mission (resolution buttons); click when done to end the turn.
/// The button is blocked from advancing while the player still has an
/// event option or a mission resolution to choose.
/// </summary>
public class TurnPhaseButton : MonoBehaviour
{
    [SerializeField] private TurnController Turns;
    [SerializeField] private EventManager Events;
    [SerializeField] private MissionManager Missions;
    [SerializeField] private ZebraGameController Cards;
    private Button PhaseButton;

    private void Start()
    {
        // A game entered from the main menu already has a selected difficulty. When MainMap is
        // opened directly, wait until the bootstrap difficulty dialog has been answered. In both
        // cases the first event is opened without asking the player to press a separate button.
        StartCoroutine(BeginFirstEventAfterDifficultySelection());
    }

    private void Awake()
    {
        if (Turns == null) Turns = FindAnyObjectByType<TurnController>();
        if (Events == null) Events = FindAnyObjectByType<EventManager>();
        if (Missions == null) Missions = FindAnyObjectByType<MissionManager>();
        if (Cards == null) Cards = FindAnyObjectByType<ZebraGameController>();
        PhaseButton = GetComponent<Button>();
    }

    private void Update()
    {
        if (PhaseButton == null || Turns == null) return;
        bool decisionOpen = (Events != null && Events.IsAwaitingChoice()) || (Missions != null && Missions.IsAwaitingChoice());
        bool missionPanelOpen = Missions != null && Missions.IsPanelOpen();   // 任务面板打开时锁定阶段按钮
        bool cardsReady = Cards == null || Turns.CheckTurnPhase() == 3 || Cards.CanAdvanceTurnPhase();
        PhaseButton.interactable = !Turns.IsGameOver() && !decisionOpen && !missionPanelOpen && cardsReady;
    }

    public void HandleTurnPhase()
    {
        // Force the player to finish the current decision before the phase can move on.
        if (Events != null && Events.IsAwaitingChoice()) return;
        if (Missions != null && Missions.IsAwaitingChoice()) return;
        if (Missions != null && Missions.IsPanelOpen()) return;   // 任务面板打开时锁定：先关闭面板才能推进
        if (Cards != null && Turns.CheckTurnPhase() != 3 && !Cards.CanAdvanceTurnPhase()) return;

        int phase = Turns.CheckTurnPhase();

        if (phase == 0)                 // Event -> get a mission, then start card play
        {
            BeginEventForCurrentTurn();
        }
        else if (phase == 1)            // Card play done -> reveal leftover cards
        {
            if (Cards != null) Cards.RevealLeftoverCards();     // reveal effects of unplayed cards, enter buy mode
            Turns.NextTurnPhase();      // 1 -> 2
        }
        else if (phase == 2)            // Buying done -> handle the mission
        {
            if (Cards != null) Cards.EndCardTurn();             // hide the card board
            if (Missions != null) Missions.BeginResolution();   // opens resolution buttons if a mission exists
            Turns.NextTurnPhase();      // 2 -> 3  (now awaiting the resolution, if any)
            if (Missions == null || !Missions.HasMission())
                CompleteMissionAndBeginNextTurn();
        }
        else if (phase == 3)            // Mission handled -> end turn and draw the next hand
        {
            CompleteMissionAndBeginNextTurn();
        }
    }

    public void BeginEventForCurrentTurn()
    {
        if (Turns == null || Turns.IsGameOver() || Turns.CheckTurnPhase() != 0) return;

        if (Events != null) Events.TriggerRandomEvent();
        Turns.NextTurnPhase();
        if (Cards != null && (Events == null || !Events.IsAwaitingChoice()))
            Cards.EnableCardPlay();
    }

    public void CompleteMissionAndBeginNextTurn()
    {
        if (Turns == null || Turns.IsGameOver() || Turns.CheckTurnPhase() != 3) return;

        if (Missions != null) Missions.EndMission();
        Turns.EndTurn();
        if (!Turns.IsGameOver())
        {
            if (Cards != null) Cards.StartTurnHand();
            StartCoroutine(BeginEventNextFrame());
        }
    }

    private System.Collections.IEnumerator BeginFirstEventAfterDifficultySelection()
    {
        while (Turns != null && !Turns.IsGameOver() && !GameSessionSettings.HasSelectedDifficulty)
            yield return null;

        // All scene Start methods have run by this point, including ZebraGameController's initial
        // hand setup, so the event panel and the card interface are ready together.
        yield return null;
        BeginEventForCurrentTurn();
    }

    private System.Collections.IEnumerator BeginEventNextFrame()
    {
        yield return null;
        BeginEventForCurrentTurn();
    }
}
