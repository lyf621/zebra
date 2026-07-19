using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewMission", menuName = "Game/Mission")]
public class MissionSO : ScriptableObject
{
    [Header("English")]
    [TextArea] public string missionTitle;
    [TextArea] public string missionDescription;
    [Header("Chinese")]
    [TextArea] public string missionTitleChinese;
    [TextArea] public string missionDescriptionChinese;
    public List<MissionResolution> possibleResolutions;

    // 按当前语言返回任务标题；旧测试资产未填写中文时也能直接使用。
    public string GetTitle(bool chinese)
    {
        if (!chinese) return missionTitle;
        if (!string.IsNullOrEmpty(missionTitleChinese)) return missionTitleChinese;
        if (missionTitle == "Appease") return "安抚民众";
        if (missionTitle == "Suppress") return "武力镇压";
        return missionTitle;
    }

    // 按当前语言返回任务说明；旧测试资产未填写中文时也能直接使用。
    public string GetDescription(bool chinese)
    {
        if (!chinese) return missionDescription;
        if (!string.IsNullOrEmpty(missionDescriptionChinese)) return missionDescriptionChinese;
        if (missionTitle == "Appease") return "减免税收并提供救济，或许足以让民众恢复平静。";
        if (missionTitle == "Suppress") return "军队将捍卫王国权威，并迅速击溃叛乱者。";
        return missionDescription;
    }
}

[System.Serializable]
public class MissionResolution
{
    public string buttonText;
    public string buttonTextChinese;
    public StatModifier resolutionEffect;
    public bool isFailure;   // 可选标记

    // 按当前语言返回任务选项；未知的新内容仍安全回退到英文。
    public string GetButtonText(bool chinese)
    {
        if (!chinese) return buttonText;
        if (!string.IsNullOrEmpty(buttonTextChinese)) return buttonTextChinese;
        if (buttonText == "Accept") return "接受";
        if (buttonText == "Reject") return "拒绝";
        return buttonText;
    }
}
