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

    // 金币检查（仅针对金币）：处理方式扣金币且玩家金币不足以支付全额时返回 false。
    // 此时选项仍可点击，但会先弹出警告面板，确认后才会执行并使金币变为负数。
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

        // Every resolution is selectable, affordable or not. An unaffordable one raises a
        // warning first (see OnResolutionSelected) and, once confirmed, drives the treasury into
        // debt — which is what now triggers the bankruptcy ending.
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
        if (missionPanel == null) return;
        missionPanel.ConvertEventConfirmationToReviewToggle();
        missionPanel.SetVisibleForReview(false);
    }

    /// <summary>Called by MissionPanelUI when the player clicks a resolution button.</summary>
    public void OnResolutionSelected(int resolutionIndex)
    {
        if (currentActiveMission == null) return;
        if (pendingResolution != null) return;
        if (GoldWarningPanel.EnsureExists().IsOpen()) return;   // answer the warning first
        if (resolutionIndex < 0 || resolutionIndex >= currentActiveMission.possibleResolutions.Count) return;

        MissionResolution res = currentActiveMission.possibleResolutions[resolutionIndex];

        // 金币不足时不再禁止选择：先弹出警告，确认后照常执行（金币会变为负数），
        // 取消则什么都不做，处理选项按钮仍在，玩家可以重新选择。
        if (!CanAffordResolution(res))
        {
            GoldWarningPanel.EnsureExists().Show(UseChinese(), () => CommitResolution(res), null);
            return;
        }

        CommitResolution(res);
    }

    /// <summary>Locks in a resolution: applies its effect, then asks for the final turn-advance.</summary>
    private void CommitResolution(MissionResolution res)
    {
        if (currentActiveMission == null || res == null) return;

        pendingResolution = res;

        // Apply the outcome now, while the mission panel is still on screen. The confirmation
        // step below then gives the player a visible beat in which to read the new HUD values.
        // Applying it on the confirm click instead meant the change landed in the same frame the
        // turn ended, and was overwritten by the balance drift before it was ever drawn.
        ApplyResolutionEffect(res);

        // Debt does not end the game here. The player carries the negative treasury forward and
        // may still earn their way out of it; bankruptcy is judged once, at the end of the final
        // turn, in GameEndingController.EvaluateVictory.
        if (missionPanel != null)
        {
            missionPanel.ShowSelectedResolutionConfirmation(
                res,
                UseChinese() ? "确认并进入下一回合" : "Confirm and Begin Next Turn",
                ConfirmSelectedResolution);
        }
        else
        {
            ConfirmSelectedResolution();
        }
    }

    private bool UseChinese()
    {
        ZebraGameController cards = FindAnyObjectByType<ZebraGameController>();
        return cards != null && cards.UseChinese;
    }

    /// <summary>
    /// The stat changes a resolution will actually produce right now. Fight (a temporary
    /// reveal-phase resource) absorbs part of any military-strength cost: a negative ms is
    /// reduced toward zero by the current Fight. Hover previews use this so the HUD projection
    /// matches what confirming the resolution really does.
    /// </summary>
    public StatModifier GetEffectiveResolutionEffect(MissionResolution res)
    {
        if (res == null) return default;

        StatModifier effect = res.resolutionEffect;   // struct copy — the asset is never mutated
        if (stats != null && effect.ms < 0)
            effect.ms = Mathf.Min(effect.ms + stats.GetFight(), 0);
        return effect;
    }

    /// <summary>Writes a resolution's stat changes into the StatManager.</summary>
    private void ApplyResolutionEffect(MissionResolution res)
    {
        if (stats == null || res == null) return;
        GetEffectiveResolutionEffect(res).ApplyTo(stats);
    }

    private void ConfirmSelectedResolution()
    {
        if (currentActiveMission == null || pendingResolution == null) return;

        // The effect was already applied in OnResolutionSelected; this step only closes the turn.
        pendingResolution = null;
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
