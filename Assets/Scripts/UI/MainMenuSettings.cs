using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A standalone Settings panel for the MainMenu scene (Language + Rules + Close).
/// The in-game settings live inside ZebraGameController and are only available during
/// play, so this gives the menu its own lightweight version. Language is stored in
/// GameSessionSettings.UseChinese, which persists and is read by ZebraGameController on
/// start, so the choice made here carries into the game.
///
/// Setup in Unity:
///   1. Add this component to any object in MainMenu (e.g. the Canvas or an empty GO).
///   2. Add a "Settings" button to the menu and wire its OnClick -> MainMenuSettings.OpenSettings().
/// The panel builds its own overlay canvas, so it does not depend on the menu's layout.
/// </summary>
public class MainMenuSettings : MonoBehaviour
{
    [Tooltip("Web page (PDF/Word) opened by the Rules button. Replace with your real link.")]
    [SerializeField] private string rulesUrl = "https://example.com/rules.pdf";

    private GameObject mPanelRoot;
    private Font mFont;

    // Bind a MainMenu "Settings" button's OnClick to this.
    public void OpenSettings()
    {
        if (mPanelRoot != null) return;

        mFont = GameUITheme.GetLegacyFont();
        if (mFont == null) mFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Dedicated overlay canvas (matches the 1280x720 scaler the game UI is designed for).
        mPanelRoot = new GameObject("MainMenu Settings", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = mPanelRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;
        CanvasScaler scaler = mPanelRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        Image dim = mPanelRoot.AddComponent<Image>();
        dim.color = new Color(0.025f, 0.02f, 0.015f, 0.82f);

        BuildContents();
    }

    public void CloseSettings()
    {
        if (mPanelRoot != null) Destroy(mPanelRoot);
        mPanelRoot = null;
    }

    private void SetLanguage(bool chinese)
    {
        GameSessionSettings.UseChinese = chinese;
        Rebuild();   // refresh selection highlight + localized labels
    }

    private void Rebuild()
    {
        // Remove everything except the dim background, then rebuild.
        for (int i = mPanelRoot.transform.childCount - 1; i >= 0; i--)
            Destroy(mPanelRoot.transform.GetChild(i).gameObject);
        BuildContents();
    }

    private void BuildContents()
    {
        bool chinese = GameSessionSettings.UseChinese;

        Image panel = CreateImage("Panel", mPanelRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(500f, 320f), new Color(0.88f, 0.84f, 0.72f, 1f));

        CreateText("Title", panel.transform, chinese ? "设置" : "Settings", 28, FontStyle.Bold, new Vector2(0f, 118f), new Vector2(300f, 50f), new Color(0.12f, 0.11f, 0.09f));

        // Rules
        CreateButton("Rules", panel.transform, chinese ? "规则" : "Rules", new Vector2(0f, 56f), new Vector2(200f, 46f), new Color(0.20f, 0.34f, 0.42f))
            .onClick.AddListener(() => Application.OpenURL(rulesUrl));

        // Language
        CreateText("Language", panel.transform, chinese ? "语言" : "Language", 20, FontStyle.Bold, new Vector2(0f, 8f), new Vector2(260f, 40f), new Color(0.12f, 0.11f, 0.09f));
        CreateButton("English", panel.transform, "English", new Vector2(-92f, -40f), new Vector2(160f, 46f), !chinese ? new Color(0.18f, 0.43f, 0.31f) : new Color(0.34f, 0.33f, 0.3f))
            .onClick.AddListener(() => SetLanguage(false));
        CreateButton("Chinese", panel.transform, "中文", new Vector2(92f, -40f), new Vector2(160f, 46f), chinese ? new Color(0.18f, 0.43f, 0.31f) : new Color(0.34f, 0.33f, 0.3f))
            .onClick.AddListener(() => SetLanguage(true));

        // Close
        CreateButton("Close", panel.transform, chinese ? "关闭" : "Close", new Vector2(0f, -108f), new Vector2(140f, 42f), new Color(0.32f, 0.3f, 0.27f))
            .onClick.AddListener(CloseSettings);
    }

    // ---------------------------------------------------------------- helpers
    private Image CreateImage(string name, Transform parent, Vector2 position, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = go.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private Text CreateText(string name, Transform parent, string value, int size, FontStyle style, Vector2 position, Vector2 rectSize, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = rectSize;
        Text text = go.GetComponent<Text>();
        text.font = mFont;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.supportRichText = true;
        text.raycastTarget = false;
        return text;
    }

    private Button CreateButton(string name, Transform parent, string label, Vector2 position, Vector2 size, Color color)
    {
        Image image = CreateImage(name, parent, position, size, color);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        CreateText("Label", image.transform, label, 19, FontStyle.Bold, Vector2.zero, size - new Vector2(16f, 8f), Color.white);
        GameUITheme.StyleButton(button);   // same burgundy lacquer strip as MainMap buttons
        return button;
    }
}
