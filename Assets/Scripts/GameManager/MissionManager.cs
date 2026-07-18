using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionManager : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private MissionPanelUI missionPanel;
    [SerializeField] private StatManager stats;

    private MissionSO currentActiveMission;

    // True while resolution buttons are shown and waiting for the player to react (Turn Phase 2).
    private bool awaitingResolution = false;
    public bool IsAwaitingChoice() { return awaitingResolution; }
    public bool HasMission() { return currentActiveMission != null; }

    private void Awake()
    {
        if (stats == null) stats = FindAnyObjectByType<StatManager>();
    }

    /// <summary>Called by EventManager after an event option with a linked mission is chosen.</summary>
    public void SetCurrentMission(MissionSO mission)
    {
        currentActiveMission = mission;
        awaitingResolution = false;
        if (missionPanel != null) missionPanel.Show(mission);   // display-only mode
    }

    /// <summary>Reveal the resolution buttons. Called on the Turn Phase 2 button.</summary>
    public void BeginResolution()
    {
        if (currentActiveMission == null) return;   // nothing to resolve -> button may pass through
        awaitingResolution = true;
        if (missionPanel != null) missionPanel.ShowResolutions(currentActiveMission, this);
    }

    /// <summary>Called by MissionPanelUI when the player clicks a resolution button.</summary>
    public void OnResolutionSelected(int resolutionIndex)
    {
        if (currentActiveMission == null) return;
        if (resolutionIndex < 0 || resolutionIndex >= currentActiveMission.possibleResolutions.Count) return;

        MissionResolution res = currentActiveMission.possibleResolutions[resolutionIndex];

        // Apply the outcome and show the result.
        if (stats != null) res.resolutionEffect.ApplyTo(stats);
        if (missionPanel != null) missionPanel.ShowResult(res);

        awaitingResolution = false;
    }

    /// <summary>Clear the mission and hide the panel (called at end of turn).</summary>
    public void EndMission()
    {
        currentActiveMission = null;
        awaitingResolution = false;
        if (missionPanel != null) missionPanel.Hide();
    }

    public void ClearCurrentMission()
    {
        currentActiveMission = null;
    }

    public MissionSO GetCurrentMission()
    {
        return currentActiveMission;
    }
}
