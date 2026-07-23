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
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameSessionSettings.SelectDifficulty(GameDifficulty.Normal);
            if (loadMainMapAfterSelection) SceneManager.LoadScene("MainMap");
            return;
        }

        Font font = Resources.Load<Font>("Fonts/NotoSansSC");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        difficultyPanel = new GameObject("Difficulty Selection", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        difficultyPanel.transform.SetParent(canvas.transform, false);
        RectTransform overlayRect = difficultyPanel.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        difficultyPanel.GetComponent<Image>().color = new Color(0.025f, 0.02f, 0.015f, 0.82f);

        Image panel = CreateImage("Panel", difficultyPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(510f, 390f), new Color(0.18f, 0.13f, 0.08f, 1f));
        CreateText("Title", panel.transform, "选择难度", font, 30, FontStyle.Bold, new Vector2(0f, 142f), new Vector2(420f, 44f));
        CreateText("Description", panel.transform, "属性初始均为 5。难度会影响每回合的属性回归区间。", font, 16, FontStyle.Normal, new Vector2(0f, 101f), new Vector2(440f, 34f));
        CreateDifficultyButton(panel.transform, font, "简单", "低于 5 时 +1；高于 7 时 -1", GameDifficulty.Easy, 42f, new Color(0.2f, 0.43f, 0.28f));
        CreateDifficultyButton(panel.transform, font, "普通", "低于 5 时 +1；高于 5 时 -1", GameDifficulty.Normal, -33f, new Color(0.32f, 0.28f, 0.17f));
        CreateDifficultyButton(panel.transform, font, "困难", "低于 3 时 +1；高于 5 时 -1", GameDifficulty.Hard, -108f, new Color(0.48f, 0.2f, 0.16f));
        CreateButton("Cancel", panel.transform, "返回", font, new Vector2(0f, -157f), new Vector2(116f, 36f), new Color(0.26f, 0.23f, 0.19f)).onClick.AddListener(() => Destroy(difficultyPanel));
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
