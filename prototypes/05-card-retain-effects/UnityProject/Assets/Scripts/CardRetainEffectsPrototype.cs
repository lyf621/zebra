using UnityEngine;

public class CardRetainEffectsPrototype : MonoBehaviour
{
    private const float kDesignWidth = 1280f;
    private const float kDesignHeight = 720f;
    private int mPublicOpinion = 5;
    private int mMilitaryStrength = 5;
    private int mAuthorityLevel = 5;
    private bool mTurnEnded = false;
    private GUIStyle mLabelStyle;
    private GUIStyle mCardStyle;

    // 绘制三项属性、两张保留效果牌和结束回合按钮。
    private void OnGUI()
    {
        CreateStyles();
        float scale = Mathf.Min(Screen.width / kDesignWidth, Screen.height / kDesignHeight);
        float offsetX = (Screen.width - kDesignWidth * scale) * 0.5f;
        float offsetY = (Screen.height - kDesignHeight * scale) * 0.5f;
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3(offsetX, offsetY, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));
        DrawRect(new Rect(0f, 0f, kDesignWidth, kDesignHeight), Color.white);

        GUI.Label(new Rect(90f, 42f, 350f, 40f), "Public Opinion (PO)  " + mPublicOpinion, mLabelStyle);
        GUI.Label(new Rect(465f, 42f, 350f, 40f), "Military Strength (MS)  " + mMilitaryStrength, mLabelStyle);
        GUI.Label(new Rect(840f, 42f, 350f, 40f), "Authority Level (AL)  " + mAuthorityLevel, mLabelStyle);
        DrawCard(new Rect(470f, 230f, 150f, 210f), "RETAIN\n\nPO +1");
        DrawCard(new Rect(660f, 230f, 150f, 210f), "RETAIN\n\nMS -1");
        DrawButton(new Rect(560f, 510f, 160f, 52f), "End Turn", !mTurnEnded, EndTurn);

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
        mCardStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true, normal = { textColor = Color.black } };
    }

    // 结束回合并结算仍保留在手中的两张牌。
    private void EndTurn()
    {
        mPublicOpinion++;
        mMilitaryStrength--;
        mTurnEnded = true;
    }

    // 绘制一张白色保留效果牌。
    private void DrawCard(Rect rect, string text)
    {
        Rect innerRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f);
        DrawRect(rect, Color.black);
        DrawRect(innerRect, Color.white);
        GUI.Label(innerRect, text, mCardStyle);
    }

    // 绘制只允许执行一次的结束回合按钮。
    private void DrawButton(Rect rect, string text, bool enabled, System.Action action)
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
