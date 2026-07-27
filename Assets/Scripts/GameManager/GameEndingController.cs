using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The victory outcomes, ordered by judgement priority (first match wins).
// None = no victory condition met at the final turn; the cascade then falls to Mid End.
public enum VictoryKind
{
    None,
    Divinity,       // Divine: PO, MS, AL, KR, CR, AR all >= 8
    Bankruptcy,
    RiseToThrone,   // Major: KR, CR, AR all >= 8
    EternalRule,    // Major: PO, MS, AL all >= 8
    Loyalty,        // Minor: AL & KR >= 8
    Devout,         // Minor: PO & CR >= 8
    Power           // Minor: MS & AR >= 8
}

public enum MidEndKind  // Triggered if not Victory
{
    None,
    Monopoly,  // gold >= 15
    Wishful,   // AL>=8 || KR>=8
    Pleaser,   // PO>=8 || CR>=8
    Tyrant     // MS>=8 || AR>=8
}

public enum LossKind    // Triggered if not Mid End
{
    None,
    Imprison,          // KR < 4
    Exile,             // AR < 4
    Excommunication,   // CR < 4
    Overthrown,        // PO < 4
    Disgrace,          // MS < 4
    Assassinated,      // AL < 4
    Mortal             // nothing collapsed and nothing stood out — the catch-all ending
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

    // 第十回合结束后按三个层级依次判定结局：胜利 -> 中间结局 -> 失败。
    // Three tiers, first match wins. Fail to earn a Victory and you drop to a Mid End; fail
    // that too and you drop to a Loss. The Loss tier always produces something — Mortal is its
    // catch-all — so the cascade can never end without a panel.
    public void ShowEnding(StatManager stats)
    {
        if (mIsShowing)
        {
            return;
        }
        mIsShowing = true;

        VictoryKind victory = EvaluateVictory(stats);
        if (victory != VictoryKind.None)
        {
            BuildVictoryInterface(victory);
            return;
        }

        MidEndKind midEnd = EvaluateMidEnd(stats);
        if (midEnd != MidEndKind.None)
        {
            BuildMidEndInterface(midEnd);
            return;
        }

        BuildLossInterface(EvaluateLoss(stats));
    }

    // Judge the outcome in priority order and return the first that holds.
    // Divinity outranks everything, debt included: a player who mastered all six stats is not
    // called bankrupt over a negative treasury. Debt comes next, ahead of every ordinary
    // victory. Then the two majors, then the three minors.
    // Thresholds are 8 or above. None => defeat.
    public static VictoryKind EvaluateVictory(StatManager stats)
    {
        if (stats == null)
        {
            return VictoryKind.None;
        }

        bool po = stats.GetPO() >= 8, ms = stats.GetMS() >= 8, al = stats.GetAL() >= 8;
        bool kr = stats.GetKR() >= 8, cr = stats.GetCR() >= 8, ar = stats.GetAR() >= 8;

        // Checked before the bankruptcy test, so it wins even with the treasury in debt.
        if (po && ms && al && kr && cr && ar) return VictoryKind.Divinity;

        if (stats.GetGold() < 0) return VictoryKind.Bankruptcy;

        if (kr && cr && ar) return VictoryKind.RiseToThrone;   // Major: all reputations
        if (po && ms && al) return VictoryKind.EternalRule;    // Major: all resources
        if (al && kr) return VictoryKind.Loyalty;              // Minor
        if (po && cr) return VictoryKind.Devout;               // Minor
        if (ms && ar) return VictoryKind.Power;                // Minor
        return VictoryKind.None;
    }

    // Reached only when no Victory was earned. One strong suit is enough here, where a Victory
    // demanded a matching pair. Declaration order decides ties, as above.
    public static MidEndKind EvaluateMidEnd(StatManager stats)
    {
        if (stats == null)
        {
            return MidEndKind.None;
        }

        if (stats.GetGold() >= 15) return MidEndKind.Monopoly;
        if (stats.GetAL() >= 8 || stats.GetKR() >= 8) return MidEndKind.Wishful;
        if (stats.GetPO() >= 8 || stats.GetCR() >= 8) return MidEndKind.Pleaser;
        if (stats.GetMS() >= 8 || stats.GetAR() >= 8) return MidEndKind.Tyrant;
        return MidEndKind.None;
    }

    // Reached only when neither a Victory nor a Mid End was earned. Any single stat that has
    // collapsed below 4 ends the reign; declaration order decides which faction gets the blame.
    // This tier is the end of the line, so it never returns None: a player who avoided collapse
    // but achieved nothing either gets Mortal.
    public static LossKind EvaluateLoss(StatManager stats)
    {
        if (stats == null)
        {
            return LossKind.Mortal;
        }

        if (stats.GetKR() < 4) return LossKind.Imprison;
        if (stats.GetAR() < 4) return LossKind.Exile;
        if (stats.GetCR() < 4) return LossKind.Excommunication;
        if (stats.GetPO() < 4) return LossKind.Overthrown;
        if (stats.GetMS() < 4) return LossKind.Disgrace;
        if (stats.GetAL() < 4) return LossKind.Assassinated;
        return LossKind.Mortal;
    }

    // Per-victory panel text (localised). category is the Major/Minor banner; empty for defeat.
    private static void GetEndingContent(VictoryKind kind, bool chinese, out string title, out string category, out string description)
    {
        switch (kind)
        {
            case VictoryKind.Divinity:       // all six stats >= 8; outranks even Bankruptcy
                title = chinese ? "奇迹" : "Miracle";
                category = chinese ? "神级胜利" : "Divine Victory";
                description = chinese ? "你的领地繁荣而强大，你的名声受所有人尊重。你就是神性的化身。" 
                                      : "Your realm is prosperous and powerful. Your reputation is honored by all. You are the personification of divinity.";
                return;
            case VictoryKind.Bankruptcy:
                title = chinese ? "破产" : "Bankruptcy";
                category = string.Empty;
                description = chinese
                    ? "你透支了国库，深陷债务泥潭。你出售了你的土地，你的资产，甚至你那荣耀的贵族头衔…… 最终你偿清了债务，但你也变得一无所有。除了一座河边的小木屋。"
                    : "You exhausted the treasury and fell deeply into debt. You sold your land, your properties, even your glorious titles... The debt is finally paid, but you have nothing left but a cabin by the river.";
                return;
            case VictoryKind.RiseToThrone:
                title = chinese ? "擢升为王" : "Rise to the Throne";
                category = chinese ? "全面胜利" : "Major Victory";
                description = chinese ? "国王、教会与贵族齐声称颂你的美德，推举你为王位的继承人。多年以后，你作为王国的新任统治者为已故的国王举办葬礼。"
                                      : "The King, the Church, and the nobility acclaimed you in a chorus, electing you the heir to the throne. Years later, you hold the late King's funeral as the new ruler of the Kingdom.";
                return;
            case VictoryKind.EternalRule:
                title = chinese ? "永恒统治" : "Eternal Rule";
                category = chinese ? "全面胜利" : "Major Victory";
                description = chinese ? "人民的需求、军队的武力与律法的权威在你的领地里达成完美的平衡。你对公国的统治将会延续数个世纪。"
                                      : "The people's needs, the army's might and the authority of laws stand in perfect balance. Your reign will last for centuries to come.";
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
                description = chinese ? "农夫与教士将你铭记为圣人。多年以后，历史也将这样记载你。"
                                      : "Farmers and priests remember you as a saint. So will history.";
                return;
            case VictoryKind.Power:
                title = chinese ? "力量" : "Power";
                category = chinese ? "普通胜利" : "Minor Victory";
                description = chinese ? "贵族世家臣服于你的力量。再没有人胆敢挑战你的地位。"
                                      : "The noble houses submit to your power. No one will challenge your status, not anymore.";
                return;
            default:
                // VictoryKind.None no longer renders: it means "no victory", which sends the
                // cascade on to the Mid End tier. Mortal now lives in GetLossContent.
                title = string.Empty;
                category = string.Empty;
                description = string.Empty;
                return;
        }
    }

    // ===========================================================================================
    //  MID END TEXT — fill in the titles and descriptions below.
    //  category is the banner above the title; leave it empty to hide the banner entirely.
    // ===========================================================================================
    private static void GetMidEndContent(MidEndKind kind, bool chinese, out string title, out string category, out string description)
    {
        switch (kind)
        {
            case MidEndKind.Monopoly:        // gold >= 15
                title = chinese ? "大富翁" : "Monopoly";
                category = chinese ? "特殊结局" : "Special Ending";
                description = chinese ? "金币简直要从你的金库里溢出来了！的确令人印象深刻，但请记住，这游戏不是大富翁。" 
                                      : "Gold is literally spilling out of your treasury! Impressive, indeed, but make sure you remember you're not playing Monopoly.";
                return;
            case MidEndKind.Wishful:         // AL or KR >= 8
                title = chinese ? "一厢情愿" : "Wishful Thinking";
                category = chinese ? "特殊结局" : "Special Ending";
                description = chinese ? "你向国王表明忠心的努力基本白费了。你的真诚姿态得到了国王的嘉许，但你没能建立更深层次的纽带。" 
                                      : "Your efforts to display loyalty to the crown were in vain. The King praised you for your kind gestures, but deeper bonds were not forged.";
                return;
            case MidEndKind.Pleaser:         // PO or CR >= 8
                title = chinese ? "老好人" : "People Pleaser";
                category = chinese ? "特殊结局" : "Special Ending";
                description = chinese ? "尽管你付出了许多努力，但你为所有人谋福利的高尚愿景没能实现。不过，有许多人在传颂关于你的慷慨的故事。" 
                                      : "Despite all the endeavor, your intention to improve welfare for all didn't bear fruit. Nevertheless, stories of your generousity are heard in bars and markets.";
                return;
            case MidEndKind.Tyrant:          // MS or AR >= 8
                title = chinese ? "暴君" : "The Tyrant";
                category = chinese ? "特殊结局" : "Special Ending";
                description = chinese ? "你梦想拥有一支无敌的军队，以及一份被贵族们畏惧的名声。你实现这一梦想的手段使你被称作暴君。" 
                                      : "You dreamt of an invincible army and a reputation feared by Aristocrats. Your methods to realize that dream made you a tyrant.";
                return;
            default:
                title = string.Empty;
                category = string.Empty;
                description = string.Empty;
                return;
        }
    }

    // ===========================================================================================
    //  LOSS TEXT — fill in the titles and descriptions below.
    // ===========================================================================================
    private static void GetLossContent(LossKind kind, bool chinese, out string title, out string category, out string description)
    {
        switch (kind)
        {
            case LossKind.Imprison:          // KR < 4
                title = chinese ? "无地者" : "Landless";
                category = chinese ? "失败" : "Failure";
                description = chinese ? "你无视了自己与王室的糟糕关系，这种无视使你付出了终身监禁的代价。在你的牢房里，你听说了你的领地被没收的消息。" 
                                      : "Ignorance of your poor relationship with the King cost you an endless imprisonment. In your cell, you heard of the confiscation of your lands.";
                return;
            case LossKind.Exile:             // AR < 4
                title = chinese ? "流亡" : "Exile";
                category = chinese ? "失败" : "Failure";
                description = chinese ? "你给了大贵族一个攻打你的理由，却没有做好防御准备。他们的入侵迫使你逃离自己的领地。" 
                                      : "You gave the Aristocrats a reason to attack you without preparing to defend. Their invasion forced you to flee your realm.";
                return;
            case LossKind.Excommunication:   // CR < 4
                title = chinese ? "绝罚" : "Excommunication";
                category = chinese ? "失败" : "Failure";
                description = chinese ? "教会因为你对神明显而易见的不敬宣布将你绝罚。你不再是任何土地的合法主人了。" 
                                      : "The Church excommunicated you for your apparent disbelief in god. You're no longer the rightful owner of any land.";
                return;
            case LossKind.Overthrown:        // PO < 4
                title = chinese ? "推翻" : "Overthrown";
                category = chinese ? "失败" : "Failure";
                description = chinese ? "当愤怒的农民围困你的城堡，你发现自己既没有足以驱散他们的实力，也没有用来安抚他们的资源。你只有两条用来逃跑的腿。" 
                                      : "When furious farmers surrounded your castle, you had neither the power to disperse them nor the resources to appease them. You only had legs to run.";
                return;
            case LossKind.Disgrace:          // MS < 4
                title = chinese ? "耻辱" : "Disgrace";
                category = chinese ? "失败" : "Failure";
                description = chinese ? "脆弱的防御力量使你成为贪婪邻居们眼里的一块肥肉。你在耻辱中投降并交出了土地。" 
                                      : "Weak defense made you an easy target for greedy neighbors. You surrendered your land in disgrace.";
                return;
            case LossKind.Assassinated:      // AL < 4
                title = chinese ? "暗杀" : "Assassination";
                category = chinese ? "失败" : "Failure";
                description = chinese ? "你的法律既没有得到有力执行，也没有受人尊重。这片混乱催生了无数阴谋，其中一桩夺去了你的生命。" 
                                      : "Your laws are poorly enforced and barely respected. The chaos gave birth to countless conspiracies, one of which took your life.";
                return;
            case LossKind.Mortal:            // nothing collapsed, but nothing stood out either
            default:                         // also covers LossKind.None, which cannot occur
                title = chinese ? "平庸" : "Mortal";
                category = string.Empty;
                description = chinese ? "你的抱负未能实现。一切维持原状，你的名字就这样隐没在历史的一个安静的角落里。"
                                      : "Your ambitions fell short. The status quo remains, and your name vanished into a quiet corner of history.";
                return;
        }
    }

    // 三个层级各有一个入口，最终都调用同一个 BuildEndingInterface 绘制面板。
    private void BuildVictoryInterface(VictoryKind kind)
    {
        bool chinese = GameSessionSettings.UseChinese;
        // Divinity counts as major for banner styling — it is the strongest outcome in the game.
        bool major = kind == VictoryKind.Divinity || kind == VictoryKind.RiseToThrone || kind == VictoryKind.EternalRule;
        GetEndingContent(kind, chinese, out string title, out string category, out string description);

        Color titleColor = kind == VictoryKind.Divinity ? DivinityColor
                          : kind == VictoryKind.None || kind == VictoryKind.Bankruptcy ? DefeatColor
                          : major ? new Color(0.60f, 0.40f, 0.08f, 1f)
                          : new Color(0.50f, 0.34f, 0.12f, 1f);

        BuildEndingInterface(title, category, description, titleColor, major, chinese);
    }

    private void BuildMidEndInterface(MidEndKind kind)
    {
        bool chinese = GameSessionSettings.UseChinese;
        GetMidEndContent(kind, chinese, out string title, out string category, out string description);
        BuildEndingInterface(title, category, description, MidEndColor, false, chinese);
    }

    private void BuildLossInterface(LossKind kind)
    {
        bool chinese = GameSessionSettings.UseChinese;
        GetLossContent(kind, chinese, out string title, out string category, out string description);
        BuildEndingInterface(title, category, description, DefeatColor, false, chinese);
    }

    private static readonly Color DefeatColor = new Color(0.45f, 0.12f, 0.10f, 1f);
    private static readonly Color MidEndColor = new Color(0.42f, 0.34f, 0.16f, 1f);
    private static readonly Color DivinityColor = new Color(0.72f, 0.53f, 0.06f, 1f);   // brighter than Major gold

    // 创建覆盖所有游戏操作的结局界面，每种结局显示各自独有的信息。
    private void BuildEndingInterface(string title, string category, string description, Color titleColor, bool major, bool chinese)
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
