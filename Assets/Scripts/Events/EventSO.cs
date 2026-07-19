using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEvent", menuName = "Game/Event")]
public class EventSO : ScriptableObject
{
    [Header("English")]
    [TextArea] public string eventTitle;
    [TextArea] public string eventDescription;
    [Header("Chinese")]
    [TextArea] public string eventTitleChinese;
    [TextArea] public string eventDescriptionChinese;
    public List<EventOption> availableOptions;

    // 按当前语言返回事件标题；旧测试资产未填写中文时也能直接使用。
    public string GetTitle(bool chinese)
    {
        if (!chinese) return eventTitle;
        if (!string.IsNullOrEmpty(eventTitleChinese)) return eventTitleChinese;
        if (eventTitle == "Test: Uprising!") return "事件：民众起义";
        return eventTitle;
    }

    // 按当前语言返回事件说明；旧测试资产未填写中文时也能直接使用。
    public string GetDescription(bool chinese)
    {
        if (!chinese) return eventDescription;
        if (!string.IsNullOrEmpty(eventDescriptionChinese)) return eventDescriptionChinese;
        if (eventTitle == "Test: Uprising!") return "长期积累的不满最终演变为大规模起义。你必须选择安抚民众，或依靠军队恢复秩序。";
        return eventDescription;
    }
}

[System.Serializable]
public class EventOption
{
    public string buttonText;
    public string buttonTextChinese;
    public StatModifier immediateEffect;
    public MissionSO linkedMission;   // 可为空

    // 按当前语言返回事件选项；旧测试资产未填写中文时也能直接使用。
    public string GetButtonText(bool chinese)
    {
        if (!chinese) return buttonText;
        if (!string.IsNullOrEmpty(buttonTextChinese)) return buttonTextChinese;
        if (buttonText == "Appease") return "安抚民众";
        if (buttonText == "Use force") return "武力镇压";
        return buttonText;
    }
}
