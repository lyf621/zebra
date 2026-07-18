using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventManager : MonoBehaviour
{
    [Header("Event data")]
    [SerializeField] private EventSO[] eventPool;

    [Header("Wiring")]
    [SerializeField] private EventPanelUI eventPanel;
    [SerializeField] private MissionManager missionManager;
    [SerializeField] private StatManager stats;

    private EventSO currentActiveEvent;

    // True while the event panel is open and waiting for the player to pick an option.
    private bool awaitingChoice = false;
    public bool IsAwaitingChoice() { return awaitingChoice; }

    private void Awake()
    {
        // Fallbacks so the system still works if a reference was left unassigned.
        if (stats == null) stats = FindAnyObjectByType<StatManager>();
        if (missionManager == null) missionManager = FindAnyObjectByType<MissionManager>();
    }

    /// <summary>Pick a random event and open the event panel. Called on the Turn Phase 0 button.</summary>
    public void TriggerRandomEvent()
    {
        if (eventPool == null || eventPool.Length == 0)
        {
            Debug.LogWarning("EventManager: eventPool is empty, no event to trigger.");
            return;
        }

        int index = Random.Range(0, eventPool.Length);
        currentActiveEvent = eventPool[index];

        awaitingChoice = true;
        if (eventPanel != null) eventPanel.Show(currentActiveEvent, this);
    }

    /// <summary>Called by EventPanelUI when the player clicks one of the option buttons.</summary>
    public void OnOptionSelected(int optionIndex)
    {
        if (currentActiveEvent == null) return;
        if (optionIndex < 0 || optionIndex >= currentActiveEvent.availableOptions.Count) return;

        EventOption option = currentActiveEvent.availableOptions[optionIndex];

        // 1. Apply the immediate stat change.
        if (stats != null) option.immediateEffect.ApplyTo(stats);

        // 2. Close the event panel.
        awaitingChoice = false;
        if (eventPanel != null) eventPanel.Hide();

        // 3. Hand the linked mission (if any) to the MissionManager, which opens the mission panel.
        if (option.linkedMission != null && missionManager != null)
            missionManager.SetCurrentMission(option.linkedMission);

        currentActiveEvent = null;
    }
}
