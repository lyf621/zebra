using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTurnCyclePrototype : MonoBehaviour
{
    private enum MovingCardMode
    {
        DrawFlip,
        DiscardFlip
    }

    private class MovingCard
    {
        public int CardNumber;
        public Rect StartRect;
        public Rect EndRect;
        public float StartAngle;
        public float EndAngle;
        public float Progress;
        public MovingCardMode Mode;
    }

    private const int kTotalCardCount = 7;
    private const int kHandSize = 3;
    private const float kDesignWidth = 1280f;
    private const float kDesignHeight = 720f;
    private const float kCardWidth = 120f;
    private const float kCardHeight = 168f;

    private readonly List<int> mDrawPile = new List<int>();
    private readonly List<int> mHand = new List<int>();
    private readonly List<int> mDiscardPile = new List<int>();

    private bool mIsAnimating = false;
    private int mShuffleCount = 0;
    private int mFixedHandLayoutCount = 0;
    private int mMovingPileCount = 0;
    private Vector2 mMovingPilePosition;
    private MovingCard mMovingCard;

    private GUIStyle mLabelStyle;
    private GUIStyle mCountStyle;
    private GUIStyle mNumberStyle;

    // 建立七张牌，随机洗牌，并播放第一轮抽牌动画。
    private void Start()
    {
        for (int i = 1; i <= kTotalCardCount; i++)
        {
            mDrawPile.Add(i);
        }

        Shuffle(mDrawPile);
        StartCoroutine(DrawToHandRoutine());
    }

    // 绘制纯 2D 卡牌界面和当前正在播放的动画。
    private void OnGUI()
    {
        CreateStyles();

        float scale = Mathf.Min(Screen.width / kDesignWidth, Screen.height / kDesignHeight);
        float offsetX = (Screen.width - kDesignWidth * scale) * 0.5f;
        float offsetY = (Screen.height - kDesignHeight * scale) * 0.5f;
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3(offsetX, offsetY, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));

        DrawRect(new Rect(0f, 0f, kDesignWidth, kDesignHeight), Color.white);
        DrawEndTurnButton();
        DrawDrawPile();
        DrawHand();
        DrawDiscardPile();
        DrawMovingPile();
        DrawMovingCard();

        GUI.matrix = oldMatrix;
    }

    // 创建文字、按钮和鼠标悬停样式。
    private void CreateStyles()
    {
        if (mLabelStyle != null)
        {
            return;
        }

        mLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.black } };
        mCountStyle = new GUIStyle(GUI.skin.label) { fontSize = 17, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.black } };
        mNumberStyle = new GUIStyle(GUI.skin.label) { fontSize = 56, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.black } };
    }

    // 使用 Unity 按钮处理命中，但只接受鼠标左键并立即清除键盘焦点。
    private void DrawEndTurnButton()
    {
        Rect buttonRect = new Rect(552f, 24f, 176f, 48f);
        Rect innerRect = new Rect(554f, 26f, 172f, 44f);
        GUI.SetNextControlName("EndTurnButton");
        bool clicked = !mIsAnimating && GUI.Button(innerRect, GUIContent.none, GUIStyle.none);
        GUI.FocusControl(null);
        DrawRect(buttonRect, Color.black);
        DrawRect(innerRect, Color.white);
        GUI.Label(innerRect, "End Turn", mLabelStyle);

        if (clicked)
        {
            StartCoroutine(EndTurnRoutine());
        }
    }

    // 在左侧绘制灰色背面的牌库和牌数。
    private void DrawDrawPile()
    {
        GUI.Label(new Rect(54f, 184f, 220f, 30f), "Deck", mLabelStyle);
        Rect cardRect = GetDeckRect();

        if (mDrawPile.Count > 0)
        {
            DrawCardStack(cardRect, new Color(0.58f, 0.58f, 0.58f));
        }

        GUI.Label(new Rect(54f, 420f, 220f, 30f), mDrawPile.Count.ToString(), mCountStyle);
    }

    // 在下方将白色数字手牌绘制成轻微扇形。
    private void DrawHand()
    {
        GUI.Label(new Rect(340f, 382f, 600f, 30f), "In Hand", mLabelStyle);
        int layoutCount = mFixedHandLayoutCount > 0 ? mFixedHandLayoutCount : mHand.Count;
        int[] cards = mHand.ToArray();
        List<int> drawOrder = new List<int>();

        for (int i = 0; i < cards.Length; i++)
        {
            drawOrder.Add(i);
        }

        drawOrder.Sort((left, right) => Mathf.Abs(right - (layoutCount - 1) * 0.5f).CompareTo(Mathf.Abs(left - (layoutCount - 1) * 0.5f)));

        for (int i = 0; i < drawOrder.Count; i++)
        {
            int cardIndex = drawOrder[i];
            DrawRotatedNumberCard(GetHandCardRect(cardIndex, layoutCount), cards[cardIndex], GetHandCardAngle(cardIndex, layoutCount));
        }
    }

    // 在右侧绘制灰色背面的弃牌堆和牌数。
    private void DrawDiscardPile()
    {
        GUI.Label(new Rect(1006f, 184f, 220f, 30f), "Discard Pile", mLabelStyle);
        Rect cardRect = GetDiscardRect();

        if (mDiscardPile.Count > 0)
        {
            DrawCardStack(cardRect, new Color(0.58f, 0.58f, 0.58f));
        }

        GUI.Label(new Rect(1006f, 420f, 220f, 30f), mDiscardPile.Count.ToString(), mCountStyle);
    }

    // 绘制一张不响应点击的白色数字牌。
    private void DrawNumberCard(Rect rect, int cardNumber)
    {
        Rect innerRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f);
        DrawRect(rect, Color.black);
        DrawRect(innerRect, Color.white);
        GUI.Label(innerRect, cardNumber.ToString(), mNumberStyle);
    }

    // 旋转手牌形成轻微扇形。
    private void DrawRotatedNumberCard(Rect rect, int cardNumber, float angle)
    {
        Matrix4x4 oldMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, rect.center);
        DrawNumberCard(rect, cardNumber);
        GUI.matrix = oldMatrix;
    }

    // 绘制正在移动的牌，并在抽牌或弃牌时模拟翻面。
    private void DrawMovingCard()
    {
        if (mMovingCard == null)
        {
            return;
        }

        float easedProgress = Mathf.SmoothStep(0f, 1f, mMovingCard.Progress);
        float x = Mathf.Lerp(mMovingCard.StartRect.x, mMovingCard.EndRect.x, easedProgress);
        float y = Mathf.Lerp(mMovingCard.StartRect.y, mMovingCard.EndRect.y, easedProgress) - Mathf.Sin(easedProgress * Mathf.PI) * 44f;
        Rect rect = new Rect(x, y, kCardWidth, kCardHeight);
        float angle = Mathf.Lerp(mMovingCard.StartAngle, mMovingCard.EndAngle, easedProgress);
        Matrix4x4 oldMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, rect.center);

        float widthScale = Mathf.Max(0.04f, Mathf.Abs(Mathf.Cos(easedProgress * Mathf.PI)));
        Rect flipRect = new Rect(rect.center.x - kCardWidth * widthScale * 0.5f, rect.y, kCardWidth * widthScale, kCardHeight);
        bool showBack = mMovingCard.Mode == MovingCardMode.DrawFlip ? easedProgress < 0.5f : easedProgress >= 0.5f;

        if (showBack)
        {
            DrawCardBack(flipRect);
        }
        else
        {
            DrawNumberCard(flipRect, mMovingCard.CardNumber);
        }

        GUI.matrix = oldMatrix;
    }

    // 绘制弃牌堆洗回牌库时移动的灰色牌堆。
    private void DrawMovingPile()
    {
        if (mMovingPileCount <= 0)
        {
            return;
        }

        Rect rect = new Rect(mMovingPilePosition.x, mMovingPilePosition.y, kCardWidth, kCardHeight);
        DrawCardStack(rect, new Color(0.58f, 0.58f, 0.58f));
    }

    // 绘制一张灰色背面牌。
    private void DrawCardBack(Rect rect)
    {
        DrawRect(rect, Color.black);
        DrawRect(new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f), new Color(0.58f, 0.58f, 0.58f));
    }

    // 绘制由数张灰色背面牌组成的牌堆。
    private void DrawCardStack(Rect rect, Color cardColor)
    {
        DrawRect(new Rect(rect.x + 8f, rect.y + 8f, rect.width, rect.height), new Color(0.42f, 0.42f, 0.42f));
        DrawRect(new Rect(rect.x + 4f, rect.y + 4f, rect.width, rect.height), Color.black);
        DrawRect(new Rect(rect.x + 6f, rect.y + 6f, rect.width - 4f, rect.height - 4f), cardColor);
        DrawCardBack(rect);
    }

    // 播放所有手牌飞向弃牌堆的动画，然后开始下一轮抽牌。
    private IEnumerator EndTurnRoutine()
    {
        if (mIsAnimating)
        {
            yield break;
        }

        mIsAnimating = true;
        List<int> cardsToDiscard = new List<int>();
        List<Rect> startRects = new List<Rect>();
        List<float> startAngles = new List<float>();

        for (int i = 0; i < mHand.Count; i++)
        {
            cardsToDiscard.Add(mHand[i]);
            startRects.Add(GetHandCardRect(i, mHand.Count));
            startAngles.Add(GetHandCardAngle(i, mHand.Count));
        }

        mHand.Clear();

        for (int i = 0; i < cardsToDiscard.Count; i++)
        {
            yield return AnimateCard(cardsToDiscard[i], startRects[i], GetDiscardRect(), MovingCardMode.DiscardFlip, 0.16f, startAngles[i], 0f);
            mDiscardPile.Add(cardsToDiscard[i]);
            yield return new WaitForSeconds(0.02f);
        }

        yield return DrawToHandRoutine();
    }

    // 从左侧牌库依次抽到三张，并为每张牌播放移动和翻面动画。
    private IEnumerator DrawToHandRoutine()
    {
        mIsAnimating = true;
        mFixedHandLayoutCount = kHandSize;

        while (mHand.Count < kHandSize)
        {
            if (mDrawPile.Count == 0)
            {
                if (mDiscardPile.Count == 0)
                {
                    break;
                }

                yield return ShuffleDiscardIntoDeckRoutine();
            }

            int topIndex = mDrawPile.Count - 1;
            int cardNumber = mDrawPile[topIndex];
            mDrawPile.RemoveAt(topIndex);
            Rect endRect = GetHandCardRect(mHand.Count, kHandSize);
            float endAngle = GetHandCardAngle(mHand.Count, kHandSize);

            yield return AnimateCard(cardNumber, GetDeckRect(), endRect, MovingCardMode.DrawFlip, 0.20f, 0f, endAngle);

            mHand.Add(cardNumber);
            yield return new WaitForSeconds(0.03f);
        }

        mFixedHandLayoutCount = 0;
        mIsAnimating = false;
    }

    // 将灰色弃牌堆从右侧移动回左侧，随后随机洗牌。
    private IEnumerator ShuffleDiscardIntoDeckRoutine()
    {
        List<int> recycledCards = new List<int>(mDiscardPile);
        mDiscardPile.Clear();
        mMovingPileCount = recycledCards.Count;
        Rect startRect = GetDiscardRect();
        Rect endRect = GetDeckRect();
        float elapsed = 0f;
        const float duration = 0.42f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            float x = Mathf.Lerp(startRect.x, endRect.x, progress);
            float y = Mathf.Lerp(startRect.y, endRect.y, progress) - Mathf.Sin(progress * Mathf.PI) * 70f;
            mMovingPilePosition = new Vector2(x, y);
            yield return null;
        }

        mMovingPileCount = 0;
        mDrawPile.AddRange(recycledCards);
        Shuffle(mDrawPile);
        yield return new WaitForSeconds(0.08f);
    }

    // 播放单张牌在两个位置之间移动的动画。
    private IEnumerator AnimateCard(int cardNumber, Rect startRect, Rect endRect, MovingCardMode mode, float duration, float startAngle, float endAngle)
    {
        mMovingCard = new MovingCard { CardNumber = cardNumber, StartRect = startRect, EndRect = endRect, StartAngle = startAngle, EndAngle = endAngle, Progress = 0f, Mode = mode };
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mMovingCard.Progress = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        mMovingCard = null;
    }

    // 使用 Fisher-Yates 和 Random.Range 随机排列指定牌堆。
    private void Shuffle(List<int> cards)
    {
        List<int> randomIndices = new List<int>();

        for (int i = cards.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            randomIndices.Add(randomIndex);
            int oldValue = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = oldValue;
        }

        mShuffleCount++;
        Debug.Log("Shuffle " + mShuffleCount + " random indices: " + string.Join(", ", randomIndices));
        Debug.Log("Draw pile bottom to top: " + string.Join(", ", cards));
    }

    // 取得左侧牌库的固定位置。
    private Rect GetDeckRect()
    {
        return new Rect(104f, 238f, kCardWidth, kCardHeight);
    }

    // 取得右侧弃牌堆的固定位置。
    private Rect GetDiscardRect()
    {
        return new Rect(1056f, 238f, kCardWidth, kCardHeight);
    }

    // 根据手牌总数计算指定手牌的排列位置。
    private Rect GetHandCardRect(int index, int layoutCount)
    {
        const float cardSpacing = 82f;
        float totalWidth = kCardWidth + Mathf.Max(0, layoutCount - 1) * cardSpacing;
        float startX = (kDesignWidth - totalWidth) * 0.5f;
        float centerOffset = index - (layoutCount - 1) * 0.5f;
        float y = 438f - Mathf.Abs(centerOffset) * 9f;
        return new Rect(startX + index * cardSpacing, y, kCardWidth, kCardHeight);
    }

    // 根据手牌位置计算向外展开的小角度。
    private float GetHandCardAngle(int index, int layoutCount)
    {
        float centerOffset = index - (layoutCount - 1) * 0.5f;
        return centerOffset * 7f;
    }

    // 使用 Unity 内置白色纹理绘制纯色矩形。
    private void DrawRect(Rect rect, Color color)
    {
        Color oldColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = oldColor;
    }

}
