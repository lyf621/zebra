using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 负责第十回合结束后的胜负判断、结局遮罩和返回下一局。
public class GameEndingController : MonoBehaviour
{
    private bool mIsShowing;

    // 确保场景中只有一个结局控制器；由 TurnController 启动时调用。
    public static GameEndingController EnsureExists()
    {
        GameEndingController controller = FindAnyObjectByType<GameEndingController>();
        if (controller == null)
        {
            controller = new GameObject("Game Ending Controller").AddComponent<GameEndingController>();
        }
        return controller;
    }

    // 根据提案中的五种条件判断胜利，并显示胜利或失败界面。
    public void ShowEnding(StatManager stats)
    {
        if (mIsShowing)
        {
            return;
        }
        mIsShowing = true;
        BuildEndingInterface(IsVictory(stats));
    }

    // 判断任意一种提案胜利条件是否满足；所有“最大”均使用 StatManager 的属性上限。
    public static bool IsVictory(StatManager stats)
    {
        if (stats == null)
        {
            return false;
        }

        int maximum = stats.GetMaxStat();
        bool allReputations = stats.GetKR() >= maximum && stats.GetCR() >= maximum && stats.GetAR() >= maximum;
        bool allResources = stats.GetPO() >= maximum && stats.GetMS() >= maximum && stats.GetAL() >= maximum;
        bool loyalty = stats.GetAL() >= maximum && stats.GetKR() >= maximum;
        bool devout = stats.GetPO() >= maximum && stats.GetCR() >= maximum;
        bool power = stats.GetMS() >= maximum && stats.GetAR() >= maximum;
        return allReputations || allResources || loyalty || devout || power;
    }

    // 创建覆盖所有游戏操作的简洁结局界面，只显示结果与返回按钮。
    private void BuildEndingInterface(bool victory)
    {
        GameObject canvasObject = new GameObject("Ending Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        Image overlay = CreateImage("Ending Overlay", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.015f, 0.02f, 0.018f, 0.92f));
        overlay.raycastTarget = true;

        Image panel = CreateImage("Ending Panel", canvasObject.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(580f, 330f), GameUITheme.Parchment);
        panel.sprite = GameUITheme.GetPaperSprite();
        Outline panelOutline = panel.gameObject.AddComponent<Outline>();
        panelOutline.effectColor = GameUITheme.Gold;
        panelOutline.effectDistance = new Vector2(5f, -5f);

        Text resultText = CreateText("Ending Result", panel.transform, victory ? "胜利" : "失败", 76, FontStyle.Bold, new Vector2(0.5f, 0.5f), new Vector2(0f, 52f), new Vector2(440f, 120f));
        resultText.color = victory ? new Color(0.53f, 0.34f, 0.08f, 1f) : new Color(0.45f, 0.12f, 0.10f, 1f);

        Button returnButton = CreateButton("Return Button", panel.transform, "返回", new Vector2(0.5f, 0f), new Vector2(0f, 52f), new Vector2(170f, 48f));
        returnButton.onClick.AddListener(ReturnToNextGame);
    }

    // 重新载入当前场景，清空本局状态并开始下一局。
    private void ReturnToNextGame()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    // 创建结局界面使用的 Image，并根据锚点配置稳定尺寸。
    private Image CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        if (anchorMin != anchorMax)
        {
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    // 创建支持中文的结局文字。
    private Text CreateText(string name, Transform parent, string value, int fontSize, FontStyle style, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Text text = textObject.GetComponent<Text>();
        text.font = GameUITheme.GetLegacyFont();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        return text;
    }

    // 创建结局返回按钮并应用统一的中世纪按钮样式。
    private Button CreateButton(string name, Transform parent, string label, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        CreateText("Label", buttonObject.transform, label, 19, FontStyle.Bold, new Vector2(0.5f, 0.5f), Vector2.zero, size);
        GameUITheme.StyleButton(button);
        return button;
    }
}
