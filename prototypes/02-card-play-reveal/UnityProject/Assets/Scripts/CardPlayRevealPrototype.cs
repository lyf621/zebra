using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPlayRevealPrototype : MonoBehaviour
{
    private enum MovingCardMode
    {
        FaceUp,
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

    private const float kDesignWidth = 1280f;
    private const float kDesignHeight = 720f;
    private const float kCardWidth = 120f;
    private const float kCardHeight = 168f;

    private readonly List<int> mHand = new List<int> { 1, 2, 3 };
    private readonly List<int> mDiscardPile = new List<int>();
    private int mDisplayedCard = -1;
    private int mSelectedCard = -1;
    private bool mIsAnimating = false;
    private bool mRevealFinished = false;
    private MovingCard mMovingCard;

    private GUIStyle mLabelStyle;
    private GUIStyle mCountStyle;
    private GUIStyle mNumberStyle;

    // 绘制手牌、中央展示牌、弃牌堆和动画中的牌。
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
        DrawDisplayedCard();
        DrawHand();
        DrawDiscardPile();
        DrawMovingCard();

        GUI.matrix = oldMatrix;
    }

    // 创建界面使用的文字样式。
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

    // 结束回合后按顺序逐张展示仍在手中的牌。
    private void DrawEndTurnButton()
    {
        Rect buttonRect = new Rect(552f, 24f, 176f, 48f);
        Rect innerRect = new Rect(554f, 26f, 172f, 44f);
        GUI.SetNextControlName("EndTurnButton");
        bool clicked = !mIsAnimating && !mRevealFinished && GUI.Button(innerRect, GUIContent.none, GUIStyle.none);
        GUI.FocusControl(null);
        DrawRect(buttonRect, Color.black);
        DrawRect(innerRect, Color.white);
        GUI.Label(innerRect, "End Turn", mLabelStyle);

        if (clicked)
        {
            StartCoroutine(RevealRemainingCardsRoutine());
        }
    }

    // 在中央绘制当前被打出或展示的牌。
    private void DrawDisplayedCard()
    {
        if (mDisplayedCard >= 0)
        {
            DrawNumberCard(GetDisplayRect(), mDisplayedCard, false);
        }
    }

    // 将手牌绘制成轻微重叠的扇形，并允许点击打出。
    private void DrawHand()
    {
        GUI.Label(new Rect(340f, 626f, 600f, 30f), "In Hand", mLabelStyle);
        int[] cards = mHand.ToArray();
        List<int> drawOrder = new List<int>();

        for (int i = 0; i < cards.Length; i++)
        {
            drawOrder.Add(i);
        }

        drawOrder.Sort((left, right) => Mathf.Abs(right - (cards.Length - 1) * 0.5f).CompareTo(Mathf.Abs(left - (cards.Length - 1) * 0.5f)));
        if (mSelectedCard >= 0)
        {
            int selectedIndex = System.Array.IndexOf(cards, mSelectedCard);
            drawOrder.Remove(selectedIndex);
            drawOrder.Add(selectedIndex);
        }

        for (int i = 0; i < drawOrder.Count; i++)
        {
            int cardIndex = drawOrder[i];
            Rect rect = GetHandCardRect(cardIndex, cards.Length);
            float angle = GetHandCardAngle(cardIndex, cards.Length);
            if (cards[cardIndex] == mSelectedCard)
            {
                rect = GetSelectedHandCardRect(rect);
                angle = 0f;
            }

            DrawRotatedNumberCard(rect, cards[cardIndex], !mIsAnimating && !mRevealFinished, angle);
        }
    }

    // 在右侧绘制灰色背面的弃牌堆和牌数。
    private void DrawDiscardPile()
    {
        GUI.Label(new Rect(1006f, 184f, 220f, 30f), "Discard Pile", mLabelStyle);

        if (mDiscardPile.Count > 0)
        {
            DrawCardStack(GetDiscardRect());
        }

        GUI.Label(new Rect(1006f, 420f, 220f, 30f), mDiscardPile.Count.ToString(), mCountStyle);
    }

    // 绘制白色数字牌，透明按钮只负责检测鼠标点击。
    private void DrawNumberCard(Rect rect, int cardNumber, bool clickable)
    {
        Rect innerRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f);
        GUI.SetNextControlName("Card" + cardNumber);
        bool clicked = clickable && GUI.Button(innerRect, GUIContent.none, GUIStyle.none);
        GUI.FocusControl(null);
        DrawRect(rect, Color.black);
        DrawRect(innerRect, Color.white);
        GUI.Label(innerRect, cardNumber.ToString(), mNumberStyle);

        if (clicked)
        {
            HandleCardClick(cardNumber);
        }
    }

    // 第一次点击只选中并突出卡牌，再次点击同一张牌才将它打出。
    private void HandleCardClick(int cardNumber)
    {
        if (mSelectedCard == cardNumber)
        {
            StartCoroutine(PlayCardRoutine(cardNumber));
            return;
        }

        mSelectedCard = cardNumber;
    }

    // 旋转手牌并保持 Unity 对旋转后按钮的命中检测。
    private void DrawRotatedNumberCard(Rect rect, int cardNumber, bool clickable, float angle)
    {
        Matrix4x4 oldMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, rect.center);
        DrawNumberCard(rect, cardNumber, clickable);
        GUI.matrix = oldMatrix;
    }

    // 绘制移动中的牌；弃牌时由白色正面翻为灰色背面。
    private void DrawMovingCard()
    {
        if (mMovingCard == null)
        {
            return;
        }

        float progress = Mathf.SmoothStep(0f, 1f, mMovingCard.Progress);
        float x = Mathf.Lerp(mMovingCard.StartRect.x, mMovingCard.EndRect.x, progress);
        float y = Mathf.Lerp(mMovingCard.StartRect.y, mMovingCard.EndRect.y, progress) - Mathf.Sin(progress * Mathf.PI) * 42f;
        float angle = Mathf.Lerp(mMovingCard.StartAngle, mMovingCard.EndAngle, progress);
        Rect rect = new Rect(x, y, kCardWidth, kCardHeight);
        Matrix4x4 oldMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, rect.center);

        if (mMovingCard.Mode == MovingCardMode.FaceUp)
        {
            DrawNumberCard(rect, mMovingCard.CardNumber, false);
        }
        else
        {
            float widthScale = Mathf.Max(0.04f, Mathf.Abs(Mathf.Cos(progress * Mathf.PI)));
            Rect flipRect = new Rect(rect.center.x - kCardWidth * widthScale * 0.5f, rect.y, kCardWidth * widthScale, kCardHeight);

            if (progress < 0.5f)
            {
                DrawNumberCard(flipRect, mMovingCard.CardNumber, false);
            }
            else
            {
                DrawCardBack(flipRect);
            }
        }

        GUI.matrix = oldMatrix;
    }

    // 点击手牌时，先弃掉旧展示牌，再将所选牌移动到中央。
    private IEnumerator PlayCardRoutine(int cardNumber)
    {
        if (mIsAnimating)
        {
            yield break;
        }

        int handIndex = mHand.IndexOf(cardNumber);
        if (handIndex < 0)
        {
            yield break;
        }

        mIsAnimating = true;

        if (mDisplayedCard >= 0)
        {
            yield return DiscardDisplayedCardRoutine();
        }

        Rect startRect = GetSelectedHandCardRect(GetHandCardRect(handIndex, mHand.Count));
        float startAngle = 0f;
        mSelectedCard = -1;
        mHand.RemoveAt(handIndex);
        yield return AnimateCard(cardNumber, startRect, GetDisplayRect(), MovingCardMode.FaceUp, 0.22f, startAngle, 0f);
        mDisplayedCard = cardNumber;
        mIsAnimating = false;
    }

    // 结束回合后逐张展示剩余手牌，最后一张保留在中央。
    private IEnumerator RevealRemainingCardsRoutine()
    {
        if (mIsAnimating)
        {
            yield break;
        }

        mIsAnimating = true;
        mSelectedCard = -1;

        if (mDisplayedCard >= 0)
        {
            yield return DiscardDisplayedCardRoutine();
        }

        while (mHand.Count > 0)
        {
            int cardNumber = mHand[0];
            Rect startRect = GetHandCardRect(0, mHand.Count);
            float startAngle = GetHandCardAngle(0, mHand.Count);
            mHand.RemoveAt(0);
            yield return AnimateCard(cardNumber, startRect, GetDisplayRect(), MovingCardMode.FaceUp, 0.22f, startAngle, 0f);
            mDisplayedCard = cardNumber;
            yield return new WaitForSeconds(0.55f);

            if (mHand.Count > 0)
            {
                yield return DiscardDisplayedCardRoutine();
                yield return new WaitForSeconds(0.06f);
            }
        }

        mRevealFinished = true;
        mIsAnimating = false;
    }

    // 将中央展示牌移动到弃牌堆并翻到灰色背面。
    private IEnumerator DiscardDisplayedCardRoutine()
    {
        int cardNumber = mDisplayedCard;
        mDisplayedCard = -1;
        yield return AnimateCard(cardNumber, GetDisplayRect(), GetDiscardRect(), MovingCardMode.DiscardFlip, 0.18f, 0f, 0f);
        mDiscardPile.Add(cardNumber);
    }

    // 播放单张牌在两个位置之间的弧线移动。
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

    // 绘制灰色卡背。
    private void DrawCardBack(Rect rect)
    {
        DrawRect(rect, Color.black);
        DrawRect(new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f), new Color(0.58f, 0.58f, 0.58f));
    }

    // 绘制灰色弃牌堆。
    private void DrawCardStack(Rect rect)
    {
        DrawRect(new Rect(rect.x + 8f, rect.y + 8f, rect.width, rect.height), new Color(0.42f, 0.42f, 0.42f));
        DrawRect(new Rect(rect.x + 4f, rect.y + 4f, rect.width, rect.height), Color.black);
        DrawRect(new Rect(rect.x + 6f, rect.y + 6f, rect.width - 4f, rect.height - 4f), new Color(0.58f, 0.58f, 0.58f));
        DrawCardBack(rect);
    }

    // 返回中央展示牌的位置。
    private Rect GetDisplayRect()
    {
        return new Rect(580f, 148f, kCardWidth, kCardHeight);
    }

    // 返回右侧弃牌堆的位置。
    private Rect GetDiscardRect()
    {
        return new Rect(1056f, 238f, kCardWidth, kCardHeight);
    }

    // 根据手牌数量计算相互遮挡的扇形位置。
    private Rect GetHandCardRect(int index, int layoutCount)
    {
        const float cardSpacing = 82f;
        float totalWidth = kCardWidth + Mathf.Max(0, layoutCount - 1) * cardSpacing;
        float startX = (kDesignWidth - totalWidth) * 0.5f;
        float centerOffset = index - (layoutCount - 1) * 0.5f;
        float y = 438f - Mathf.Abs(centerOffset) * 9f;
        return new Rect(startX + index * cardSpacing, y, kCardWidth, kCardHeight);
    }

    // 根据手牌位置计算向外展开的角度。
    private float GetHandCardAngle(int index, int layoutCount)
    {
        float centerOffset = index - (layoutCount - 1) * 0.5f;
        return centerOffset * 7f;
    }

    // 将选中的手牌向上抬起并稍微放大。
    private Rect GetSelectedHandCardRect(Rect rect)
    {
        return new Rect(rect.x - 12f, rect.y - 70f, rect.width + 24f, rect.height + 28f);
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
