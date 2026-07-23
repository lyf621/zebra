using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameDifficulty
{
    Easy,
    Normal,
    Hard
}

/// <summary>Session-only game settings chosen before entering a play scene.</summary>
public static class GameSessionSettings
{
    public static GameDifficulty Difficulty { get; private set; } = GameDifficulty.Normal;
    public static bool HasSelectedDifficulty { get; private set; }

    public static int BalanceLowerBound
    {
        get
        {
            return Difficulty == GameDifficulty.Easy ? 5 : Difficulty == GameDifficulty.Hard ? 3 : 5;
        }
    }

    public static int BalanceUpperBound
    {
        get
        {
            return Difficulty == GameDifficulty.Easy ? 7 : 5;
        }
    }

    public static void SelectDifficulty(GameDifficulty difficulty)
    {
        Difficulty = difficulty;
        HasSelectedDifficulty = true;
    }

    public static void ClearDifficultySelection()
    {
        Difficulty = GameDifficulty.Normal;
        HasSelectedDifficulty = false;
    }

    // Language persists across scenes and sessions so the MainMenu choice carries into the game.
    private const string kLanguageKey = "ZebraUseChinese";
    private static bool sLanguageLoaded;
    private static bool sUseChinese;
    public static bool UseChinese
    {
        get
        {
            if (!sLanguageLoaded) { sUseChinese = PlayerPrefs.GetInt(kLanguageKey, 0) == 1; sLanguageLoaded = true; }
            return sUseChinese;
        }
        set
        {
            sUseChinese = value;
            sLanguageLoaded = true;
            PlayerPrefs.SetInt(kLanguageKey, value ? 1 : 0);
        }
    }
}

public class LoadScene : MonoBehaviour
{
    private GameObject difficultyPanel;
    private bool loadMainMapAfterSelection;

    public void LoadMainMap()
    {
        if (difficultyPanel == null)
        {
            ShowDifficultySelection(true);
        }
    }

    public void LoadTutorial()
    {
        SceneManager.LoadScene("TutorialScene");
    }

    public void LoadMainMenu()
    {
        GameSessionSettings.ClearDifficultySelection();
        SceneManager.LoadScene("MainMenu");
    }

    public void ShowDifficultySelection(bool loadGameAfterSelection)
    {
        loadMainMapAfterSelection = loadGameAfterSelection;
        Font font = Resources.Load<Font>("Fonts/NotoSansSC");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Build on a dedicated overlay canvas so the panel renders completely and identically
        // whether it is opened from the MainMenu or directly in MainMap, independent of any
        // scene canvas's CanvasScaler. Matches the gameplay canvas (1280x720) the layout was
        // designed for, so the fixed-pixel content is never clipped or mis-scaled.
        difficultyPanel = new GameObject("Difficulty Selection", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = difficultyPanel.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;
        CanvasScaler scaler = difficultyPanel.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        Image dim = difficultyPanel.AddComponent<Image>();
        dim.color = new Color(0.025f, 0.02f, 0.015f, 0.82f);

        bool chinese = GameSessionSettings.UseChinese;
        Image panel = CreateImage("Panel", difficultyPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(510f, 390f), new Color(0.18f, 0.13f, 0.08f, 1f));
        CreateText("Title", panel.transform, chinese ? "选择难度" : "Select Difficulty", font, 30, FontStyle.Bold, new Vector2(0f, 142f), new Vector2(420f, 44f));
        CreateText("Description", panel.transform, chinese ? "属性初始均为 5。难度会影响每回合的属性回归区间。" : "All stats start at 5. Difficulty sets the range each stat drifts back toward every turn.", font, 16, FontStyle.Normal, new Vector2(0f, 101f), new Vector2(440f, 34f));
        CreateDifficultyButton(panel.transform, font, chinese ? "简单" : "Easy", chinese ? "低于 5 时 +1；高于 7 时 -1" : "+1 below 5;  -1 above 7", GameDifficulty.Easy, 42f, new Color(0.2f, 0.43f, 0.28f));
        CreateDifficultyButton(panel.transform, font, chinese ? "普通" : "Normal", chinese ? "低于 5 时 +1；高于 5 时 -1" : "+1 below 5;  -1 above 5", GameDifficulty.Normal, -33f, new Color(0.32f, 0.28f, 0.17f));
        CreateDifficultyButton(panel.transform, font, chinese ? "困难" : "Hard", chinese ? "低于 3 时 +1；高于 5 时 -1" : "+1 below 3;  -1 above 5", GameDifficulty.Hard, -108f, new Color(0.48f, 0.2f, 0.16f));
        CreateButton("Cancel", panel.transform, chinese ? "返回" : "Back", font, new Vector2(0f, -157f), new Vector2(116f, 36f), new Color(0.26f, 0.23f, 0.19f)).onClick.AddListener(() => Destroy(difficultyPanel));
    }

    private void StartGame(GameDifficulty difficulty)
    {
        GameSessionSettings.SelectDifficulty(difficulty);
        if (loadMainMapAfterSelection)
        {
            SceneManager.LoadScene("MainMap");
        }
        else if (difficultyPanel != null)
        {
            Destroy(difficultyPanel);
        }
    }

    private void CreateDifficultyButton(Transform parent, Font font, string title, string detail, GameDifficulty difficulty, float y, Color color)
    {
        Button button = CreateButton(title, parent, title + "\n<size=13>" + detail + "</size>", font, new Vector2(0f, y), new Vector2(420f, 62f), color);
        button.onClick.AddListener(() => StartGame(difficulty));
    }

    private static Image CreateImage(string name, Transform parent, Vector2 position, Vector2 size, Color color)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        gameObject.transform.SetParent(parent, false);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = gameObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static Text CreateText(string name, Transform parent, string value, Font font, int size, FontStyle style, Vector2 position, Vector2 rectSize)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        gameObject.transform.SetParent(parent, false);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = rectSize;
        Text text = gameObject.GetComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.96f, 0.91f, 0.78f);
        text.supportRichText = true;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(string name, Transform parent, string label, Font font, Vector2 position, Vector2 size, Color color)
    {
        Image image = CreateImage(name, parent, position, size, color);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.13f, 1.13f, 1.13f, 1f);
        colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
        button.colors = colors;
        CreateText("Label", image.transform, label, font, 19, FontStyle.Bold, Vector2.zero, size - new Vector2(16f, 8f));
        return button;
    }
}

/// <summary>
/// Shows the same difficulty prompt when MainMap is launched directly from a build or the editor.
/// This keeps the game playable while the start-menu flow is still being integrated.
/// </summary>
public class DifficultySelectionBootstrap : MonoBehaviour
{
    private System.Collections.IEnumerator Start()
    {
        if (GameSessionSettings.HasSelectedDifficulty || SceneManager.GetActiveScene().name != "MainMap")
        {
            yield break;
        }

        // ZebraGameController creates the gameplay canvas in Start, so wait one frame for it.
        yield return null;
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null || GameSessionSettings.HasSelectedDifficulty)
        {
            yield break;
        }

        LoadScene selector = canvas.GetComponent<LoadScene>();
        if (selector == null) selector = canvas.gameObject.AddComponent<LoadScene>();
        selector.ShowDifficultySelection(false);
    }
}
