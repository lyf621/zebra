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
    // True while the mission panel is on screen — used to lock all other operations (modal).
    public bool IsPanelOpen() { return missionPanel != null && missionPanel.IsOpen(); }

    // 金币检查（仅针对金币）：若处理方式扣金币（gold<0）且玩家金币不足以支付全额，则不能选择。
    public bool CanAffordResolution(MissionResolution res)
    {
        if (res == null || stats == null) return true;
        int goldDelta = res.resolutionEffect.gold;
        return goldDelta >= 0 || stats.GetGold() + goldDelta >= 0;
    }

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

        // 金币不足以支付该处理方式的金币消耗时，不允许选择（仅针对金币，其它资源不检查）。
        if (!CanAffordResolution(res)) return;

        // Apply the outcome. Fight (a temporary reveal-phase resource) absorbs part of any
        // military-strength cost: a negative ms is reduced toward zero by the current Fight.
        if (stats != null)
        {
            StatModifier effect = res.resolutionEffect;
            if (effect.ms < 0)
                effect.ms = Mathf.Min(effect.ms + stats.GetFight(), 0);
            effect.ApplyTo(stats);
        }
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
