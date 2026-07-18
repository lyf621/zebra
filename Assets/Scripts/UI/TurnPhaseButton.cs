using UnityEngine;

/// <summary>
/// Single button that drives the turn phases.
///   Phase 0 -> trigger a random event (opens the event panel).
///   Phase 1 -> reveal step (existing gameplay), just advance.
///   Phase 2 -> react to the mission (opens resolution buttons).
///   Phase 3 -> end the turn (clears mission, hides panels, resets to phase 0).
/// The button is blocked from advancing while the player still has an
/// event option or a mission resolution to choose.
/// </summary>
public class TurnPhaseButton : MonoBehaviour
{
    [SerializeField] private TurnController Turns;
    [SerializeField] private EventManager Events;
    [SerializeField] private MissionManager Missions;

    private void Awake()
    {
        if (Events == null) Events = FindAnyObjectByType<EventManager>();
        if (Missions == null) Missions = FindAnyObjectByType<MissionManager>();
    }

    public void HandleTurnPhase()
    {
        // Force the player to finish the current decision before the phase can move on.
        if (Events != null && Events.IsAwaitingChoice()) return;
        if (Missions != null && Missions.IsAwaitingChoice()) return;

        int phase = Turns.CheckTurnPhase();

        if (phase == 0)                 // Event
        {
            if (Events != null) Events.TriggerRandomEvent();
            Turns.NextTurnPhase();      // 0 -> 1  (now awaiting the event option)
        }
        else if (phase == 1)            // Reveal
        {
            Turns.NextTurnPhase();      // 1 -> 2
        }
        else if (phase == 2)            // Mission
        {
            if (Missions != null) Missions.BeginResolution();   // opens resolution buttons if a mission exists
            Turns.NextTurnPhase();      // 2 -> 3  (now awaiting the resolution, if any)
        }
        else if (phase == 3)            // End turn
        {
            if (Missions != null) Missions.EndMission();
            Turns.EndTurn();            // resets phase to 0
        }
    }
}
