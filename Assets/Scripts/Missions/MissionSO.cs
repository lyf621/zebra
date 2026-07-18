using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewMission", menuName = "Game/Mission")]
public class MissionSO : ScriptableObject
{
    [TextArea] public string missionTitle;
    [TextArea] public string missionDescription;
    public List<MissionResolution> possibleResolutions;
}

[System.Serializable]
public class MissionResolution
{
    public string buttonText;
    public StatModifier resolutionEffect;
    public bool isFailure;   // 可选标记
}