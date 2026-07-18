using UnityEngine;

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

    private void Awake()
    {
        if (Events == null) Events = FindAnyObjectByType<EventManager>();
        if (Missions == null) Missions = FindAnyObjectByType<MissionManager>();
        if (Cards == null) Cards = FindAnyObjectByType<ZebraGameController>();
    }

    public void HandleTurnPhase()
    {
        // Force the player to finish the current decision before the phase can move on.
        if (Events != null && Events.IsAwaitingChoice()) return;
        if (Missions != null && Missions.IsAwaitingChoice()) return;

        int phase = Turns.CheckTurnPhase();

        if (phase == 0)                 // Event -> get a mission, then start card play
        {
            if (Events != null) Events.TriggerRandomEvent();    // opens the event panel (awaiting the option)
            Turns.NextTurnPhase();      // 0 -> 1
            // If no event popped up (empty pool / no EventManager), start card play now;
            // otherwise EventManager starts it after the player picks an option.
            if (Cards != null && (Events == null || !Events.IsAwaitingChoice()))
                Cards.EnableCardPlay();
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
        }
        else if (phase == 3)            // Mission handled -> end turn and draw the next hand
        {
            if (Missions != null) Missions.EndMission();
            Turns.EndTurn();            // resets phase to 0, refills ministers
            if (Cards != null) Cards.StartTurnHand();   // draw next turn's hand right away
        }
    }
}
