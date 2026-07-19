using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
        }
    }

    // 切换语言时立即刷新当前事件与选项，无需关闭事件界面。
    private void RefreshLanguage(bool chinese)
    {
        if (currentEvent == null || currentManager == null) return;
        ApplyTexts();
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
        foreach (GameObject go in spawnedButtons)
            if (go != null) Destroy(go);
        spawnedButtons.Clear();
    }
}
