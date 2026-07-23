using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The five victory outcomes, ordered by judgement priority (first match wins).
// None = no victory condition met at the final turn (a defeat).
public enum VictoryKind
{
    None,
    RiseToThrone,   // Major: KR, CR, AR all > 8
    EternalRule,    // Major: PO, MS, AL all > 8
    Loyalty,        // Minor: AL & KR > 8
    Devout,         // Minor: PO & CR > 8
    Power           // Minor: MS & AR > 8
}

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
        BuildEndingInterface(EvaluateVictory(stats));
    }

    // Judge the five victory conditions in priority order and return the first that holds.
    // Thresholds are 8 or above. Two majors are checked first, then the three minors.
    // None => defeat.
    public static VictoryKind EvaluateVictory(StatManager stats)
    {
        if (stats == null)
        {
            return VictoryKind.None;
        }

        bool po = stats.GetPO() >= 8, ms = stats.GetMS() >= 8, al = stats.GetAL() >= 8;
        bool kr = stats.GetKR() >= 8, cr = stats.GetCR() >= 8, ar = stats.GetAR() >= 8;

        if (kr && cr && ar) return VictoryKind.RiseToThrone;   // Major: all reputations
        if (po && ms && al) return VictoryKind.EternalRule;    // Major: all resources
        if (al && kr) return VictoryKind.Loyalty;              // Minor
        if (po && cr) return VictoryKind.Devout;               // Minor
        if (ms && ar) return VictoryKind.Power;                // Minor
        return VictoryKind.None;
    }

    // Per-victory panel text (localised). category is the Major/Minor banner; empty for defeat.
    private static void GetEndingContent(VictoryKind kind, bool chinese, out string title, out string category, out string description)
    {
        switch (kind)
        {
            case VictoryKind.RiseToThrone:
                title = chinese ? "擢升为王" : "Rise to the Throne";
                category = chinese ? "全面胜利" : "Major Victory";
                description = chinese ? "国王、教会与贵族一致拥戴你，王冠已归你所有。"
                                      : "The King, the Church, and the nobility all acclaim you. The crown is yours.";
                return;
            case VictoryKind.EternalRule:
                title = chinese ? "永恒统治" : "Eternal Rule";
                category = chinese ? "全面胜利" : "Major Victory";
                description = chinese ? "民意、军力与权威尽在掌握，你的统治将永世长存。"
                                      : "Opinion, arms, and authority stand in perfect balance. Your reign will never end.";
                return;
            case VictoryKind.Loyalty:
                title = chinese ? "忠诚" : "Loyalty";
                category = chinese ? "普通胜利" : "Minor Victory";
                description = chinese ? "你向国王证明了忠诚，他的庇护便是你的奖赏。"
                                      : "You proved your loyalty to the King. His shelter is your reward.";
                return;
            case VictoryKind.Devout:
                title = chinese ? "虔诚" : "Devout";
                category = chinese ? "普通胜利" : "Minor Victory";
                description = chinese ? "农夫与教士将你铭记为圣人。"
                                      : "Farmers and priests remember you as a saint.";
                return;
            case VictoryKind.Power:
                title = chinese ? "力量" : "Power";
                category = chinese ? "普通胜利" : "Minor Victory";
                description = chinese ? "贵族世家臣服于你的力量。"
                                      : "The noble houses submit to your power.";
                return;
            default:
                title = chinese ? "失败" : "Defeat";
                category = string.Empty;
                description = chinese ? "你的抱负未能实现，王国将没有你而继续。"
                                      : "Your ambitions fell short. The realm moves on without you.";
                return;
        }
    }

    // 创建覆盖所有游戏操作的结局界面，每种胜利显示各自独有的信息。
    private void BuildEndingInterface(VictoryKind kind)
    {
        bool chinese = GameSessionSettings.UseChinese;
        bool major = kind == VictoryKind.RiseToThrone || kind == VictoryKind.EternalRule;
        GetEndingContent(kind, chinese, out string title, out string category, out string description);

        Color titleColor = kind == VictoryKind.None ? new Color(0.45f, 0.12f, 0.10f, 1f)
                          : major ? new Color(0.60f, 0.40f, 0.08f, 1f)
                          : new Color(0.50f, 0.34f, 0.12f, 1f);

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

        Image panel = CreateImage("Ending Panel", canvasObject.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640f, 420f), GameUITheme.Parchment);
        panel.sprite = GameUITheme.GetPaperSprite();
        Outline panelOutline = panel.gameObject.AddComponent<Outline>();
        panelOutline.effectColor = GameUITheme.Gold;
        panelOutline.effectDistance = new Vector2(5f, -5f);

        // Major / Minor Victory banner (absent on defeat).
        if (!string.IsNullOrEmpty(category))
        {
            Text categoryText = CreateText("Ending Category", panel.transform, category, 26, FontStyle.Bold, new Vector2(0.5f, 0.5f), new Vector2(0f, 148f), new Vector2(540f, 40f));
            categoryText.color = major ? new Color(0.42f, 0.30f, 0.10f, 1f) : new Color(0.35f, 0.30f, 0.20f, 1f);
        }

        Text titleText = CreateText("Ending Result", panel.transform, title, 46, FontStyle.Bold, new Vector2(0.5f, 0.5f), new Vector2(0f, 86f), new Vector2(580f, 78f));
        titleText.color = titleColor;

        Text descriptionText = CreateText("Ending Description", panel.transform, description, 21, FontStyle.Normal, new Vector2(0.5f, 0.5f), new Vector2(0f, -8f), new Vector2(544f, 150f));
        descriptionText.color = new Color(0.20f, 0.16f, 0.10f, 1f);

        Button returnButton = CreateButton("Return Button", panel.transform, chinese ? "返回主菜单" : "Main Menu", new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(200f, 48f));
        returnButton.onClick.AddListener(ReturnToMainMenu);
    }

    // 通过 LoadScene.LoadMainMenu() 返回主菜单（LoadMainMenu 是实例方法，故取/建一个实例来调用）。
    private void ReturnToMainMenu()
    {
        LoadScene loader = FindAnyObjectByType<LoadScene>();
        if (loader == null) loader = new GameObject("LoadScene").AddComponent<LoadScene>();
        loader.LoadMainMenu();
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
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
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
