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
    private MissionResolution pendingResolution;

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
        pendingResolution = null;
        awaitingResolution = false;
        // The event acknowledgement should return straight to card play. The player may still
        // inspect this pending mission through the Mission button; resolution opens it later.
    }

    /// <summary>Reveal the resolution buttons. Called on the Turn Phase 2 button.</summary>
    public void BeginResolution()
    {
        if (currentActiveMission == null) return;   // nothing to resolve -> button may pass through
        pendingResolution = null;

        // A mission with no affordable resolution would otherwise leave the player permanently
        // locked on a panel of disabled buttons.  This is the bankruptcy loss condition.
        if (!HasAffordableResolution())
        {
            awaitingResolution = true;
            ZebraGameController cards = FindAnyObjectByType<ZebraGameController>();
            bool chinese = cards != null && cards.UseChinese;
            if (missionPanel != null)
            {
                missionPanel.ShowBankruptcyResolution(
                    currentActiveMission,
                    chinese ? "接受和拒绝均无法承担，你破产了" : "Neither choice is affordable. Bankruptcy.",
                    ConfirmBankruptcy);
            }
            else
            {
                ConfirmBankruptcy();
            }
            return;
        }

        awaitingResolution = true;
        if (missionPanel != null) missionPanel.ShowResolutions(currentActiveMission, this);
    }

    /// <summary>Shows the linked mission as an event acknowledgement before card play resumes.</summary>
    public void ShowEventConfirmation(string confirmLabel, System.Action onConfirm)
    {
        if (currentActiveMission == null || missionPanel == null) return;
        missionPanel.ShowEventConfirmation(currentActiveMission, confirmLabel, onConfirm);
    }

    public void HideEventConfirmation()
    {
        if (missionPanel != null) missionPanel.SetVisibleForReview(false);
    }

    private void ConfirmBankruptcy()
    {
        awaitingResolution = false;
        GameEndingController.EnsureExists().ShowBankruptcyEnding();
    }

    private bool HasAffordableResolution()
    {
        if (currentActiveMission == null || currentActiveMission.possibleResolutions == null)
        {
            return false;
        }

        foreach (MissionResolution resolution in currentActiveMission.possibleResolutions)
        {
            if (CanAffordResolution(resolution))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Called by MissionPanelUI when the player clicks a resolution button.</summary>
    public void OnResolutionSelected(int resolutionIndex)
    {
        if (currentActiveMission == null) return;
        if (pendingResolution != null) return;
        if (resolutionIndex < 0 || resolutionIndex >= currentActiveMission.possibleResolutions.Count) return;

        MissionResolution res = currentActiveMission.possibleResolutions[resolutionIndex];

        // 金币不足以支付该处理方式的金币消耗时，不允许选择（仅针对金币，其它资源不检查）。
        if (!CanAffordResolution(res)) return;

        pendingResolution = res;
        if (missionPanel != null)
        {
            ZebraGameController cards = FindAnyObjectByType<ZebraGameController>();
            bool chinese = cards != null && cards.UseChinese;
            missionPanel.ShowSelectedResolutionConfirmation(
                res,
                chinese ? "确认并进入下一回合" : "Confirm and Begin Next Turn",
                ConfirmSelectedResolution);
        }
        else
        {
            ConfirmSelectedResolution();
        }
    }

    private void ConfirmSelectedResolution()
    {
        if (currentActiveMission == null || pendingResolution == null) return;

        MissionResolution res = pendingResolution;
        pendingResolution = null;

        // Apply the outcome. Fight (a temporary reveal-phase resource) absorbs part of any
        // military-strength cost: a negative ms is reduced toward zero by the current Fight.
        if (stats != null)
        {
            StatModifier effect = res.resolutionEffect;
            if (effect.ms < 0)
                effect.ms = Mathf.Min(effect.ms + stats.GetFight(), 0);
            effect.ApplyTo(stats);
        }
        awaitingResolution = false;

        TurnPhaseButton phaseButton = FindAnyObjectByType<TurnPhaseButton>();
        if (phaseButton != null)
        {
            phaseButton.CompleteMissionAndBeginNextTurn();
        }
        else
        {
            EndMission();
        }
    }

    /// <summary>Clear the mission and hide the panel (called at end of turn).</summary>
    public void EndMission()
    {
        currentActiveMission = null;
        pendingResolution = null;
        awaitingResolution = false;
        if (missionPanel != null) missionPanel.Hide();
    }

    public void ClearCurrentMission()
    {
        currentActiveMission = null;
        pendingResolution = null;
    }

    public MissionSO GetCurrentMission()
    {
        return currentActiveMission;
    }
}
