using System.Collections.Generic;
using UnityEngine;

public class CardBuyDeletePrototype : MonoBehaviour
{
    private const float kDesignWidth = 1280f;
    private const float kDesignHeight = 720f;
    private const float kCardWidth = 120f;
    private const float kCardHeight = 168f;

    private readonly List<int> mDeck = new List<int> { 1, 2, 3 };
    private readonly List<string> mDiscardPile = new List<string>();
    private bool mRoyalCardAvailable = true;
    private bool mRoyalCardSelected = false;
    private bool mShowDeckView = false;
    private int mSelectedDeckCard = -1;
    private GUIStyle mLabelStyle;
    private GUIStyle mNumberStyle;
    private GUIStyle mRoyalStyle;

    // 绘制主界面，删除牌时切换到牌组查看界面。
    private void OnGUI()
    {
        CreateStyles();
        float scale = Mathf.Min(Screen.width / kDesignWidth, Screen.height / kDesignHeight);
        float offsetX = (Screen.width - kDesignWidth * scale) * 0.5f;
        float offsetY = (Screen.height - kDesignHeight * scale) * 0.5f;
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3(offsetX, offsetY, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));
        DrawRect(new Rect(0f, 0f, kDesignWidth, kDesignHeight), Color.white);

        if (mShowDeckView)
        {
            DrawDeckView();
        }
        else
        {
            DrawMainView();
        }

        GUI.matrix = oldMatrix;
    }

    // 创建界面使用的文字样式。
    private void CreateStyles()
    {
        if (mLabelStyle != null)
        {
            return;
        }

        mLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.black } };
        mNumberStyle = new GUIStyle(GUI.skin.label) { fontSize = 52, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.black } };
        mRoyalStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.42f, 0.28f, 0.02f) } };
    }

    // 绘制牌库、弃牌堆和可购买的皇家牌。
    private void DrawMainView()
    {
        GUI.Label(new Rect(490f, 30f, 300f, 36f), "Royal Decrees", mLabelStyle);

        if (mRoyalCardAvailable)
        {
            Rect royalRect = new Rect(580f, mRoyalCardSelected ? 82f : 98f, kCardWidth, kCardHeight);
            if (DrawRoyalCard(royalRect, true))
            {
                mRoyalCardSelected = !mRoyalCardSelected;
            }
        }

        DrawPile(new Rect(200f, 360f, kCardWidth, kCardHeight), "Deck", mDeck.Count);
        DrawDiscardPile(new Rect(960f, 360f, kCardWidth, kCardHeight));
        DrawActionButton(new Rect(470f, 330f, 160f, 52f), "Buy", mRoyalCardSelected, BuyRoyalCard);
        DrawActionButton(new Rect(650f, 330f, 160f, 52f), "Delete", mDeck.Count > 0, OpenDeckView);
    }

    // 打开类似牌组查看页的界面，显示牌库中的全部三张牌。
    private void DrawDeckView()
    {
        GUI.Label(new Rect(390f, 50f, 500f, 40f), "Deck", mLabelStyle);
        float totalWidth = kCardWidth + Mathf.Max(0, mDeck.Count - 1) * 160f;
        float startX = (kDesignWidth - totalWidth) * 0.5f;

        for (int i = 0; i < mDeck.Count; i++)
        {
            Rect rect = new Rect(startX + i * 160f, mSelectedDeckCard == mDeck[i] ? 180f : 202f, kCardWidth, kCardHeight);
            if (DrawNumberCard(rect, mDeck[i]))
            {
                mSelectedDeckCard = mDeck[i];
            }
        }

        DrawActionButton(new Rect(470f, 470f, 160f, 52f), "Delete", mSelectedDeckCard >= 0, DeleteSelectedCard);
        DrawActionButton(new Rect(650f, 470f, 160f, 52f), "Back", true, CloseDeckView);
    }

    // 购买皇家牌并将它直接放入弃牌堆。
    private void BuyRoyalCard()
    {
        mDiscardPile.Add("ROYAL");
        mRoyalCardAvailable = false;
        mRoyalCardSelected = false;
    }

    // 打开牌组查看界面以选择要删除的牌。
    private void OpenDeckView()
    {
        mSelectedDeckCard = -1;
        mShowDeckView = true;
    }

    // 删除选中的牌并返回主界面。
    private void DeleteSelectedCard()
    {
        mDeck.Remove(mSelectedDeckCard);
        CloseDeckView();
    }

    // 关闭牌组查看界面并清除选择。
    private void CloseDeckView()
    {
        mSelectedDeckCard = -1;
        mShowDeckView = false;
    }

    // 绘制灰色背面的牌堆及其数量。
    private void DrawPile(Rect rect, string title, int count)
    {
        GUI.Label(new Rect(rect.x - 50f, rect.y - 48f, rect.width + 100f, 34f), title, mLabelStyle);
        if (count > 0)
        {
            DrawRect(new Rect(rect.x + 8f, rect.y + 8f, rect.width, rect.height), new Color(0.42f, 0.42f, 0.42f));
            DrawRect(rect, Color.black);
            DrawRect(new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f), new Color(0.58f, 0.58f, 0.58f));
        }
        GUI.Label(new Rect(rect.x, rect.y + rect.height + 18f, rect.width, 34f), count.ToString(), mLabelStyle);
    }

    // 绘制弃牌堆，皇家牌在其中仍保持金色正面。
    private void DrawDiscardPile(Rect rect)
    {
        GUI.Label(new Rect(rect.x - 50f, rect.y - 48f, rect.width + 100f, 34f), "Discard Pile", mLabelStyle);
        if (mDiscardPile.Count > 0)
        {
            DrawRoyalCard(rect, false);
        }
        GUI.Label(new Rect(rect.x, rect.y + rect.height + 18f, rect.width, 34f), mDiscardPile.Count.ToString(), mLabelStyle);
    }

    // 绘制金色边框的皇家牌，并按需要接收点击。
    private bool DrawRoyalCard(Rect rect, bool clickable)
    {
        Rect innerRect = new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, rect.height - 10f);
        bool clicked = clickable && GUI.Button(innerRect, GUIContent.none, GUIStyle.none);
        DrawRect(rect, new Color(0.86f, 0.64f, 0.12f));
        DrawRect(innerRect, new Color(1f, 0.96f, 0.76f));
        GUI.Label(innerRect, "ROYAL", mRoyalStyle);
        return clicked;
    }

    // 绘制白色数字牌，并返回该牌是否被点击。
    private bool DrawNumberCard(Rect rect, int cardNumber)
    {
        Rect innerRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f);
        bool clicked = GUI.Button(innerRect, GUIContent.none, GUIStyle.none);
        DrawRect(rect, Color.black);
        DrawRect(innerRect, Color.white);
        GUI.Label(innerRect, cardNumber.ToString(), mNumberStyle);
        return clicked;
    }

    // 绘制只在条件满足时生效的操作按钮。
    private void DrawActionButton(Rect rect, string text, bool enabled, System.Action action)
    {
        Rect innerRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f);
        bool clicked = enabled && GUI.Button(innerRect, GUIContent.none, GUIStyle.none);
        DrawRect(rect, Color.black);
        DrawRect(innerRect, enabled ? Color.white : new Color(0.82f, 0.82f, 0.82f));
        GUI.Label(innerRect, text, mLabelStyle);

        if (clicked)
        {
            action();
        }
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
