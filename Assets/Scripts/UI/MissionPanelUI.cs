using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the Mission panel.
/// Two states:
///   1. Display mode  -> just shows the mission title / description (right after the event is resolved).
///   2. Resolution mode -> also spawns one button per possible resolution (during Turn Phase 2).
/// The player can freely show / hide this panel via ToggleWindow() (bound to a UI button).
/// Like EventPanelUI, this class holds no game rules; it reports the chosen index to MissionManager.
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

    private void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        if (resultText != null) resultText.text = string.Empty;
    }

    /// <summary>Display-only: show the mission contents (no resolution buttons yet).</summary>
    public void Show(MissionSO mission)
    {
        if (mission == null) return;

        if (titleText != null) titleText.text = mission.missionTitle;
        if (descriptionText != null) descriptionText.text = mission.missionDescription;
        if (resultText != null) resultText.text = string.Empty;

        ClearButtons();
        if (panelRoot != null) panelRoot.SetActive(true);
    }

    /// <summary>Resolution mode: reveal the buttons the player uses to react to the mission.</summary>
    public void ShowResolutions(MissionSO mission, MissionManager manager)
    {
        if (mission == null || manager == null) return;

        // Ensure it is visible even if the player had hidden it.
        if (panelRoot != null) panelRoot.SetActive(true);

        ClearButtons();
        if (resolutionContainer == null || resolutionButtonPrefab == null || mission.possibleResolutions == null) return;

        for (int i = 0; i < mission.possibleResolutions.Count; i++)
        {
            int capturedIndex = i;
            MissionResolution res = mission.possibleResolutions[i];

            GameObject go = Instantiate(resolutionButtonPrefab, resolutionContainer);
            spawnedButtons.Add(go);

            TMP_Text label = go.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = res.buttonText;

            Button btn = go.GetComponentInChildren<Button>();
            if (btn != null)
            {
                GameUITheme.StyleButton(btn);
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => manager.OnResolutionSelected(capturedIndex));
            }
        }
    }

    /// <summary>Called after a resolution is chosen: remove buttons and show the outcome.</summary>
    public void ShowResult(MissionResolution resolution)
    {
        ClearButtons();
        if (resultText != null)
            resultText.text = resolution.isFailure ? "Failed: " + resolution.buttonText
                                                    : "Resolved: " + resolution.buttonText;
    }

    /// <summary>Hide and reset the panel (called at end of turn).</summary>
    public void Hide()
    {
        ClearButtons();
        if (resultText != null) resultText.text = string.Empty;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    /// <summary>Player-facing show / hide toggle (bind a UI button to this).</summary>
    public void ToggleWindow()
    {
        if (panelRoot != null) panelRoot.SetActive(!panelRoot.activeSelf);
    }

    private void ClearButtons()
    {
        foreach (GameObject go in spawnedButtons)
            if (go != null) Destroy(go);
        spawnedButtons.Clear();
    }
}
