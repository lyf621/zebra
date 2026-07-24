using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Controls the Event popup panel.
/// Shows an EventSO's title / description and spawns one button per available option.
/// It does NOT know any game rules — it just reports the clicked option index back to EventManager.
/// </summary>
public class EventPanelUI : MonoBehaviour
{
    [Header("Panel window (the visual root that gets shown / hidden)")]
    [SerializeField] private GameObject panelRoot;

    [Header("Text fields")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Option buttons")]
    [Tooltip("Empty parent under which option buttons are spawned (e.g. a Vertical Layout Group).")]
    [SerializeField] private Transform optionContainer;
    [Tooltip("Prefab containing a Button and a TMP_Text somewhere in its children.")]
    [SerializeField] private GameObject optionButtonPrefab;

    private readonly List<GameObject> spawnedButtons = new List<GameObject>();
    private EventSO currentEvent;
    private EventManager currentManager;
    private ZebraGameController cards;
    private bool showingConfirmation;
    private Button confirmationButton;
    private int selectedOptionIndex = -1;
    private System.Action confirmationAction;

    private void Awake()
    {
        // Make sure the panel starts hidden.
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void Start()
    {
        cards = FindAnyObjectByType<ZebraGameController>();
        if (cards != null) cards.LanguageChanged += RefreshLanguage;
    }

    private void OnDestroy()
    {
        if (cards != null) cards.LanguageChanged -= RefreshLanguage;
    }

    /// <summary>Populate and show the panel for the given event.</summary>
    public void Show(EventSO ev, EventManager manager)
    {
        if (ev == null || manager == null) return;

        currentEvent = ev;
        currentManager = manager;
        showingConfirmation = false;
        confirmationButton = null;
        selectedOptionIndex = -1;
        confirmationAction = null;
        ApplyTexts();

        BuildOptions(ev, manager);

        if (panelRoot != null) panelRoot.SetActive(true);
    }

    /// <summary>Hide the panel and clear spawned buttons.</summary>
    public void Hide()
    {
        ClearButtons();
        if (panelRoot != null) panelRoot.SetActive(false);
        currentEvent = null;
        currentManager = null;
        showingConfirmation = false;
        confirmationButton = null;
        selectedOptionIndex = -1;
        confirmationAction = null;
    }

    /// <summary>
    /// Replace the clicked choices with the linked mission's possible results and one confirm button.
    /// </summary>
    public void ShowSelectedOptionConfirmation(int selectedOptionIndex, string confirmLabel, System.Action onConfirm)
    {
        showingConfirmation = true;
        this.selectedOptionIndex = selectedOptionIndex;
        confirmationAction = onConfirm;
        BuildConfirmationLayout(confirmLabel);
    }

    private void BuildConfirmationLayout(string confirmLabel)
    {
        ClearButtons();
        if (currentEvent == null || optionContainer == null || optionButtonPrefab == null) return;

        bool chinese = cards != null && cards.UseChinese;
        if (descriptionText != null)
        {
            string description = currentEvent.GetDescription(chinese);
            EventOption option = currentEvent.availableOptions != null && selectedOptionIndex >= 0 && selectedOptionIndex < currentEvent.availableOptions.Count
                ? currentEvent.availableOptions[selectedOptionIndex] : null;
            string results = option != null && option.linkedMission != null
                ? BuildMissionPreview(option.linkedMission, chinese)
                : option != null ? option.GetButtonText(chinese) : string.Empty;
            descriptionText.text = string.IsNullOrEmpty(results) ? description : description + "\n\n" + results;
            // The confirmation view intentionally keeps the entire selected mission outcome
            // list readable.  Do not collapse the second result to an ellipsis.
            descriptionText.overflowMode = TextOverflowModes.Overflow;
        }

        GameObject confirm = Instantiate(optionButtonPrefab, optionContainer);
        spawnedButtons.Add(confirm);
        TMP_Text confirmText = confirm.GetComponentInChildren<TMP_Text>();
        if (confirmText != null) confirmText.text = confirmLabel;
        confirmationButton = confirm.GetComponentInChildren<Button>();
        if (confirmationButton != null)
        {
            GameUITheme.StyleButton(confirmationButton);
            confirmationButton.onClick.RemoveAllListeners();
            confirmationButton.onClick.AddListener(() => confirmationAction?.Invoke());
        }
    }

    private void BuildOptions(EventSO ev, EventManager manager)
    {
        ClearButtons();
        if (optionContainer == null || optionButtonPrefab == null || ev.availableOptions == null) return;

        for (int i = 0; i < ev.availableOptions.Count; i++)
        {
            int capturedIndex = i;                       // capture for the closure
            EventOption option = ev.availableOptions[i];

            GameObject go = Instantiate(optionButtonPrefab, optionContainer);
            spawnedButtons.Add(go);

            TMP_Text label = go.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = option.GetButtonText(cards != null && cards.UseChinese);

            Button btn = go.GetComponentInChildren<Button>();
            if (btn != null)
            {
                GameUITheme.StyleButton(btn);
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => manager.OnOptionSelected(capturedIndex));
            }

            // 悬停时预览该选项所连任务的潜在数值变化。
            if (option.linkedMission != null)
            {
                MissionSO mission = option.linkedMission;
                EventTrigger trigger = go.GetComponent<EventTrigger>();
                if (trigger == null) trigger = go.AddComponent<EventTrigger>();
                trigger.triggers.Clear();
                AddTrigger(trigger, EventTriggerType.PointerEnter, d => ShowMissionPreview(mission, ((PointerEventData)d).position));
                AddTrigger(trigger, EventTriggerType.PointerExit, d => HideMissionPreview());
            }
        }
    }

    private void AddTrigger(EventTrigger trigger, EventTriggerType type, System.Action<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(d => callback(d));
        trigger.triggers.Add(entry);
    }

    private void ShowMissionPreview(MissionSO mission, Vector2 screenPos)
    {
        if (mission == null) return;
        bool chinese = cards != null && cards.UseChinese;
        MissionPreviewTooltip.EnsureExists().Show(mission.GetTitle(chinese), BuildMissionPreview(mission, chinese), screenPos);
    }

    private void HideMissionPreview()
    {
        MissionPreviewTooltip.HideTooltip();
    }

    // 列出任务每个处理选项及其七项数值变化（Gold / PO / MS / AL / KR / CR / AR）。
    private string BuildMissionPreview(MissionSO mission, bool chinese)
    {
        if (mission == null || mission.possibleResolutions == null) return string.Empty;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < mission.possibleResolutions.Count; i++)
        {
            MissionResolution res = mission.possibleResolutions[i];
            if (res == null) continue;
            StatModifier e = res.resolutionEffect;
            if (sb.Length > 0) sb.Append('\n');
            sb.Append(res.GetButtonText(chinese)).Append('\n');
            sb.Append(GameLocalization.FormatStatChanges(e, chinese, "  "));
        }
        return sb.ToString();
    }

    private string Sign(int v) { return (v > 0 ? "+" : "") + v; }

    // 切换语言时立即刷新当前事件与选项，无需关闭事件界面。
    private void RefreshLanguage(bool chinese)
    {
        if (currentEvent == null || currentManager == null) return;
        ApplyTexts();
        if (showingConfirmation)
        {
            BuildConfirmationLayout(chinese ? "确认" : "Confirm");
            return;
        }
        BuildOptions(currentEvent, currentManager);
    }

    // 使用当前语言填充标题和说明。
    private void ApplyTexts()
    {
        if (currentEvent == null) return;
        bool chinese = cards != null && cards.UseChinese;
        if (titleText != null) titleText.text = currentEvent.GetTitle(chinese);
        if (descriptionText != null) descriptionText.text = currentEvent.GetDescription(chinese);
    }

    // 半返回控制器用此方法暂时隐藏或恢复事件界面，不清除玩家尚未选择的按钮。
    public void SetVisibleForReview(bool visible) { if (panelRoot != null) panelRoot.SetActive(visible); }

    private void ClearButtons()
    {
        MissionPreviewTooltip.HideTooltip();   // 清理/重建按钮时收起悬停预览
        foreach (GameObject go in spawnedButtons)
            if (go != null) Destroy(go);
        spawnedButtons.Clear();
    }
}
