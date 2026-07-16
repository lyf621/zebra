using System.Collections.Generic;
using UnityEngine;

public class PileViewPrototype : MonoBehaviour
{
    private const float kDesignWidth = 1280f;
    private const float kDesignHeight = 720f;
    private const float kCardWidth = 120f;
    private const float kCardHeight = 168f;

    private readonly List<int> mDeck = new List<int> { 1, 2, 3, 4 };
    private readonly List<int> mDiscardPile = new List<int> { 5, 6, 7 };
    private List<int> mViewedPile;
    private string mViewedPileName = "";
    private GUIStyle mLabelStyle;
    private GUIStyle mNumberStyle;

    // 绘制牌堆主界面，点击牌堆后切换到对应的查看页。
    private void OnGUI()
    {
        CreateStyles();
        float scale = Mathf.Min(Screen.width / kDesignWidth, Screen.height / kDesignHeight);
        float offsetX = (Screen.width - kDesignWidth * scale) * 0.5f;
        float offsetY = (Screen.height - kDesignHeight * scale) * 0.5f;
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3(offsetX, offsetY, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));
        DrawRect(new Rect(0f, 0f, kDesignWidth, kDesignHeight), Color.white);

        if (mViewedPile == null)
        {
            DrawMainView();
        }
        else
        {
            DrawPileView();
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

        mLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.black } };
        mNumberStyle = new GUIStyle(GUI.skin.label) { fontSize = 52, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.black } };
    }

    // 绘制可点击的牌库和弃牌堆。
    private void DrawMainView()
    {
        if (DrawPile(new Rect(260f, 250f, kCardWidth, kCardHeight), "Deck", mDeck.Count))
        {
            OpenPile(mDeck, "Deck");
        }

        if (DrawPile(new Rect(900f, 250f, kCardWidth, kCardHeight), "Discard Pile", mDiscardPile.Count))
        {
            OpenPile(mDiscardPile, "Discard Pile");
        }
    }

    // 绘制当前牌堆中的全部卡牌，并提供返回按钮。
    private void DrawPileView()
    {
        GUI.Label(new Rect(390f, 64f, 500f, 40f), mViewedPileName, mLabelStyle);
        float spacing = 150f;
        float totalWidth = kCardWidth + Mathf.Max(0, mViewedPile.Count - 1) * spacing;
        float startX = (kDesignWidth - totalWidth) * 0.5f;

        for (int i = 0; i < mViewedPile.Count; i++)
        {
            DrawNumberCard(new Rect(startX + i * spacing, 220f, kCardWidth, kCardHeight), mViewedPile[i]);
        }

        DrawButton(new Rect(560f, 480f, 160f, 52f), "Back", ClosePile);
    }

    // 记录要查看的牌堆和标题。
    private void OpenPile(List<int> pile, string pileName)
    {
        mViewedPile = pile;
        mViewedPileName = pileName;
    }

    // 关闭牌堆查看页并返回主界面。
    private void ClosePile()
    {
        mViewedPile = null;
        mViewedPileName = "";
    }

    // 绘制灰色牌背及数量，并返回是否被点击。
    private bool DrawPile(Rect rect, string title, int count)
    {
        GUI.Label(new Rect(rect.x - 60f, rect.y - 52f, rect.width + 120f, 36f), title, mLabelStyle);
        Rect clickRect = new Rect(rect.x, rect.y, rect.width + 8f, rect.height + 8f);
        bool clicked = GUI.Button(clickRect, GUIContent.none, GUIStyle.none);
        DrawRect(new Rect(rect.x + 8f, rect.y + 8f, rect.width, rect.height), new Color(0.42f, 0.42f, 0.42f));
        DrawRect(rect, Color.black);
        DrawRect(new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f), new Color(0.58f, 0.58f, 0.58f));
        GUI.Label(new Rect(rect.x, rect.y + rect.height + 18f, rect.width, 36f), count.ToString(), mLabelStyle);
        return clicked;
    }

    // 绘制查看页中的白色数字牌。
    private void DrawNumberCard(Rect rect, int cardNumber)
    {
        Rect innerRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f);
        DrawRect(rect, Color.black);
        DrawRect(innerRect, Color.white);
        GUI.Label(innerRect, cardNumber.ToString(), mNumberStyle);
    }

    // 绘制返回按钮并执行对应操作。
    private void DrawButton(Rect rect, string text, System.Action action)
    {
        Rect innerRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f);
        bool clicked = GUI.Button(innerRect, GUIContent.none, GUIStyle.none);
        DrawRect(rect, Color.black);
        DrawRect(innerRect, Color.white);
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
