using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardRetainEffectsPrototype : MonoBehaviour
{
    private enum CardType
    {
        PublicOpinion,
        MilitaryStrength
    }

    private class MovingCard
    {
        public CardType Type;
        public Rect StartRect;
        public Rect EndRect;
        public float StartAngle;
        public float EndAngle;
        public float Progress;
        public bool FlipToDiscard;
    }

    private const float kDesignWidth = 1280f;
    private const float kDesignHeight = 720f;
    private const float kCardWidth = 150f;
    private const float kCardHeight = 210f;

    private readonly List<CardType> mHand = new List<CardType> { CardType.PublicOpinion, CardType.MilitaryStrength };
    private int mPublicOpinion = 5;
    private int mMilitaryStrength = 5;
    private int mAuthorityLevel = 5;
    private int mDiscardCount = 0;
    private bool mIsAnimating = false;
    private bool mTurnEnded = false;
    private CardType? mDisplayedCard;
    private MovingCard mMovingCard;
    private GUIStyle mLabelStyle;
    private GUIStyle mCardStyle;

    // 绘制三项属性、扇形手牌、中央展示牌和弃牌堆。
    private void OnGUI()
    {
        CreateStyles();
        float scale = Mathf.Min(Screen.width / kDesignWidth, Screen.height / kDesignHeight);
        float offsetX = (Screen.width - kDesignWidth * scale) * 0.5f;
        float offsetY = (Screen.height - kDesignHeight * scale) * 0.5f;
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3(offsetX, offsetY, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));
        DrawRect(new Rect(0f, 0f, kDesignWidth, kDesignHeight), Color.white);

        DrawAttributes();
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

        mLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 19, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.black } };
        mCardStyle = new GUIStyle(GUI.skin.label) { fontSize = 21, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true, normal = { textColor = Color.black } };
    }

    // 绘制内政对应的三项属性及当前数值。
    private void DrawAttributes()
    {
        GUI.Label(new Rect(40f, 28f, 380f, 36f), "Public Opinion (PO)  " + mPublicOpinion, mLabelStyle);
        GUI.Label(new Rect(450f, 28f, 380f, 36f), "Military Strength (MS)  " + mMilitaryStrength, mLabelStyle);
        GUI.Label(new Rect(860f, 28f, 380f, 36f), "Authority Level (AL)  " + mAuthorityLevel, mLabelStyle);
    }

    // 点击结束回合后，开始逐张展示并结算手牌。
    private void DrawEndTurnButton()
    {
        Rect rect = new Rect(560f, 78f, 160f, 48f);
        Rect innerRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f);
        bool enabled = !mIsAnimating && !mTurnEnded;
        bool clicked = enabled && GUI.Button(innerRect, GUIContent.none, GUIStyle.none);
        DrawRect(rect, Color.black);
        DrawRect(innerRect, enabled ? Color.white : new Color(0.82f, 0.82f, 0.82f));
        GUI.Label(innerRect, "End Turn", mLabelStyle);

        if (clicked)
        {
            StartCoroutine(ResolveRetainedCardsRoutine());
        }
    }

    // 在中央绘制当前正在展示和结算的牌。
    private void DrawDisplayedCard()
    {
        if (mDisplayedCard.HasValue)
        {
            DrawCard(GetDisplayRect(), mDisplayedCard.Value);
        }
    }

    // 将剩余手牌绘制成轻微重叠的小弧形。
    private void DrawHand()
    {
        GUI.Label(new Rect(390f, 650f, 500f, 30f), "In Hand", mLabelStyle);
        for (int i = 0; i < mHand.Count; i++)
        {
            Rect rect = GetHandCardRect(i, mHand.Count);
            float angle = GetHandCardAngle(i, mHand.Count);
            Matrix4x4 oldMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, rect.center);
            DrawCard(rect, mHand[i]);
            GUI.matrix = oldMatrix;
        }
    }

    // 绘制右侧灰色弃牌堆和当前牌数。
    private void DrawDiscardPile()
    {
        Rect rect = GetDiscardRect();
        GUI.Label(new Rect(rect.x - 50f, rect.y - 48f, rect.width + 100f, 34f), "Discard Pile", mLabelStyle);
        if (mDiscardCount > 0)
        {
            DrawRect(new Rect(rect.x + 8f, rect.y + 8f, rect.width, rect.height), new Color(0.42f, 0.42f, 0.42f));
            DrawCardBack(rect);
        }
        GUI.Label(new Rect(rect.x, rect.y + rect.height + 16f, rect.width, 34f), mDiscardCount.ToString(), mLabelStyle);
    }

    // 绘制移动中的牌，进入弃牌堆时由正面翻为灰色背面。
    private void DrawMovingCard()
    {
        if (mMovingCard == null)
        {
            return;
        }

        float progress = Mathf.SmoothStep(0f, 1f, mMovingCard.Progress);
        float x = Mathf.Lerp(mMovingCard.StartRect.x, mMovingCard.EndRect.x, progress);
        float y = Mathf.Lerp(mMovingCard.StartRect.y, mMovingCard.EndRect.y, progress) - Mathf.Sin(progress * Mathf.PI) * 36f;
        float angle = Mathf.Lerp(mMovingCard.StartAngle, mMovingCard.EndAngle, progress);
        Rect rect = new Rect(x, y, kCardWidth, kCardHeight);
        Matrix4x4 oldMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, rect.center);

        if (mMovingCard.FlipToDiscard)
        {
            float widthScale = Mathf.Max(0.04f, Mathf.Abs(Mathf.Cos(progress * Mathf.PI)));
            Rect flipRect = new Rect(rect.center.x - kCardWidth * widthScale * 0.5f, rect.y, kCardWidth * widthScale, kCardHeight);
            if (progress < 0.5f)
            {
                DrawCard(flipRect, mMovingCard.Type);
            }
            else
            {
                DrawCardBack(flipRect);
            }
        }
        else
        {
            DrawCard(rect, mMovingCard.Type);
        }

        GUI.matrix = oldMatrix;
    }

    // 依次展示手牌、触发保留效果，再将其放入弃牌堆。
    private IEnumerator ResolveRetainedCardsRoutine()
    {
        mIsAnimating = true;

        while (mHand.Count > 0)
        {
            CardType cardType = mHand[0];
            Rect startRect = GetHandCardRect(0, mHand.Count);
            float startAngle = GetHandCardAngle(0, mHand.Count);
            mHand.RemoveAt(0);
            yield return AnimateCard(cardType, startRect, GetDisplayRect(), startAngle, 0f, false, 0.22f);
            mDisplayedCard = cardType;
            ApplyRetainEffect(cardType);
            yield return new WaitForSeconds(0.55f);
            mDisplayedCard = null;
            yield return AnimateCard(cardType, GetDisplayRect(), GetDiscardRect(), 0f, 0f, true, 0.18f);
            mDiscardCount++;
            yield return new WaitForSeconds(0.08f);
        }

        mTurnEnded = true;
        mIsAnimating = false;
    }

    // 根据展示的牌改变对应属性。
    private void ApplyRetainEffect(CardType cardType)
    {
        if (cardType == CardType.PublicOpinion)
        {
            mPublicOpinion++;
        }
        else
        {
            mMilitaryStrength--;
        }
    }

    // 播放单张牌在两个位置之间的弧线移动。
    private IEnumerator AnimateCard(CardType cardType, Rect startRect, Rect endRect, float startAngle, float endAngle, bool flipToDiscard, float duration)
    {
        mMovingCard = new MovingCard { Type = cardType, StartRect = startRect, EndRect = endRect, StartAngle = startAngle, EndAngle = endAngle, Progress = 0f, FlipToDiscard = flipToDiscard };
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            mMovingCard.Progress = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        mMovingCard = null;
    }

    // 绘制白色保留效果牌。
    private void DrawCard(Rect rect, CardType cardType)
    {
        Rect innerRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f);
        DrawRect(rect, Color.black);
        DrawRect(innerRect, Color.white);
        GUI.Label(innerRect, GetCardText(cardType), mCardStyle);
    }

    // 返回不同保留牌显示的效果文字。
    private string GetCardText(CardType cardType)
    {
        return cardType == CardType.PublicOpinion ? "RETAIN\n\nPO +1" : "RETAIN\n\nMS -1";
    }

    // 绘制灰色卡牌背面。
    private void DrawCardBack(Rect rect)
    {
        DrawRect(rect, Color.black);
        DrawRect(new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f), new Color(0.58f, 0.58f, 0.58f));
    }

    // 返回中央展示牌的位置。
    private Rect GetDisplayRect()
    {
        return new Rect(565f, 170f, kCardWidth, kCardHeight);
    }

    // 返回右侧弃牌堆的位置。
    private Rect GetDiscardRect()
    {
        return new Rect(1030f, 270f, kCardWidth, kCardHeight);
    }

    // 根据手牌数量计算相互遮挡的扇形位置。
    private Rect GetHandCardRect(int index, int layoutCount)
    {
        const float cardSpacing = 100f;
        float totalWidth = kCardWidth + Mathf.Max(0, layoutCount - 1) * cardSpacing;
        float startX = (kDesignWidth - totalWidth) * 0.5f;
        float centerOffset = index - (layoutCount - 1) * 0.5f;
        float y = 430f - Mathf.Abs(centerOffset) * 10f;
        return new Rect(startX + index * cardSpacing, y, kCardWidth, kCardHeight);
    }

    // 根据手牌位置计算向外展开的角度。
    private float GetHandCardAngle(int index, int layoutCount)
    {
        float centerOffset = index - (layoutCount - 1) * 0.5f;
        return centerOffset * 8f;
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
