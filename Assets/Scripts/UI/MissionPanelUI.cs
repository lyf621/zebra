using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the Mission panel.
/// Two states:
///   1. Display mode  -> just shows the mission title / description (right after the event is resolved).
///   2. Resolution mode -> also spawns one button per possible resolution (during Turn Phase 3).
/// The player can freely show / hide this panel via ToggleWindow() (bound to a UI button).
/// Like EventPanelUI, this class holds no game rules; it reports the chosen index to MissionManager.
/// Text follows the current language (ZebraGameController.UseChinese) and refreshes live.
/// </summary>
public class MissionPanelUI : MonoBehaviour
{
    [Header("Panel window (the visual root that gets shown / hidden)")]
    [SerializeField] private GameObject panelRoot;

    [Header("Text fields")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [Tooltip("Optional: shows the outcome text after a resolution is chosen.")]
    [SerializeField] private TMP_Text resultText;

    [Header("Resolution buttons")]
    [Tooltip("Empty parent under which resolution buttons are spawned.")]
    [SerializeField] private Transform resolutionContainer;
    [Tooltip("Prefab containing a Button and a TMP_Text somewhere in its children.")]
    [SerializeField] private GameObject resolutionButtonPrefab;

    private readonly List<GameObject> spawnedButtons = new List<GameObject>();
    private MissionSO currentMission;
    private MissionManager currentManager;   // non-null only while resolution buttons are shown
    private MissionResolution currentResult;  // set once a resolution has been chosen
    private ZebraGameController cards;

    private void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        if (resultText != null) resultText.text = string.Empty;
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

    /// <summary>Display-only: show the mission contents (no resolution buttons yet).</summary>
    public void Show(MissionSO mission)
    {
        if (mission == null) return;

        currentMission = mission;
        currentManager = null;
        currentResult = null;

        ApplyTexts();
        if (resultText != null) resultText.text = string.Empty;

        ClearButtons();
        if (panelRoot != null) panelRoot.SetActive(true);
    }

    /// <summary>Resolution mode: reveal the buttons the player uses to react to the mission.</summary>
    public void ShowResolutions(MissionSO mission, MissionManager manager)
    {
        if (mission == null || manager == null) return;

        currentMission = mission;
        currentManager = manager;
        currentResult = null;

        // Ensure it is visible even if the player had hidden it.
        if (panelRoot != null) panelRoot.SetActive(true);
        ApplyTexts();
        BuildResolutionButtons();
    }

    /// <summary>Called after a resolution is chosen: remove buttons and show the outcome.</summary>
    public void ShowResult(MissionResolution resolution)
    {
        currentManager = null;
        currentResult = resolution;
        ClearButtons();
        ApplyResultText();
    }

    /// <summary>Hide and reset the panel (called at end of turn).</summary>
    public void Hide()
    {
        ClearButtons();
        if (resultText != null) resultText.text = string.Empty;
        if (panelRoot != null) panelRoot.SetActive(false);
        currentMission = null;
        currentManager = null;
        currentResult = null;
    }

    /// <summary>Player-facing show / hide toggle (bind a UI button to this).</summary>
    public void ToggleWindow()
    {
        if (panelRoot != null) panelRoot.SetActive(!panelRoot.activeSelf);
    }

    // 决策查看控制器用此方法暂时隐藏或恢复任务界面，不清除玩家尚未选择的按钮。
    public void SetVisibleForReview(bool visible)
    {
        if (panelRoot != null) panelRoot.SetActive(visible);
    }

    // 切换语言时立即刷新当前任务、选项与结果文本。
    private void RefreshLanguage(bool chinese)
    {
        if (currentMission == null) return;
        ApplyTexts();
        if (currentManager != null) BuildResolutionButtons();
        if (currentResult != null) ApplyResultText();
    }

    // 使用当前语言填充标题和说明。
    private void ApplyTexts()
    {
        if (currentMission == null) return;
        bool chinese = cards != null && cards.UseChinese;
        if (titleText != null) titleText.text = currentMission.GetTitle(chinese);
        if (descriptionText != null) descriptionText.text = currentMission.GetDescription(chinese);
    }

    // 使用当前语言重建任务处理选项按钮。
    private void BuildResolutionButtons()
    {
        ClearButtons();
        if (currentMission == null || currentManager == null) return;
        if (resolutionContainer == null || resolutionButtonPrefab == null || currentMission.possibleResolutions == null) return;

        bool chinese = cards != null && cards.UseChinese;
        MissionManager manager = currentManager;
        for (int i = 0; i < currentMission.possibleResolutions.Count; i++)
        {
            int capturedIndex = i;
            MissionResolution res = currentMission.possibleResolutions[i];

            GameObject go = Instantiate(resolutionButtonPrefab, resolutionContainer);
            spawnedButtons.Add(go);

            TMP_Text label = go.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = res.GetButtonText(chinese);

            Button btn = go.GetComponentInChildren<Button>();
            if (btn != null)
            {
                GameUITheme.StyleButton(btn);
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => manager.OnResolutionSelected(capturedIndex));
            }
        }
    }

    // 使用当前语言填充结果文本。
    private void ApplyResultText()
    {
        if (resultText == null || currentResult == null) return;
        bool chinese = cards != null && cards.UseChinese;
        string label = currentResult.GetButtonText(chinese);
        if (chinese)
            resultText.text = (currentResult.isFailure ? "失败：" : "完成：") + label;
        else
            resultText.text = (currentResult.isFailure ? "Failed: " : "Resolved: ") + label;
    }

    private void ClearButtons()
    {
        foreach (GameObject go in spawnedButtons)
            if (go != null) Destroy(go);
        spawnedButtons.Clear();
    }
}
