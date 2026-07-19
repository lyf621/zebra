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

    private void Awake()
    {
        // Make sure the panel starts hidden.
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    /// <summary>Populate and show the panel for the given event.</summary>
    public void Show(EventSO ev, EventManager manager)
    {
        if (ev == null || manager == null) return;

        if (titleText != null) titleText.text = ev.eventTitle;
        if (descriptionText != null) descriptionText.text = ev.eventDescription;

        BuildOptions(ev, manager);

        if (panelRoot != null) panelRoot.SetActive(true);
    }

    /// <summary>Hide the panel and clear spawned buttons.</summary>
    public void Hide()
    {
        ClearButtons();
        if (panelRoot != null) panelRoot.SetActive(false);
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
            if (label != null) label.text = option.buttonText;

            Button btn = go.GetComponentInChildren<Button>();
            if (btn != null)
            {
                GameUITheme.StyleButton(btn);
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => manager.OnOptionSelected(capturedIndex));
            }
        }
    }

    private void ClearButtons()
    {
        foreach (GameObject go in spawnedButtons)
            if (go != null) Destroy(go);
        spawnedButtons.Clear();
    }
}
