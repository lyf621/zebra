using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEvent", menuName = "Game/Event")]
public class EventSO : ScriptableObject
{
    [TextArea] public string eventTitle;
    [TextArea] public string eventDescription;
    public List<EventOption> availableOptions;
}

[System.Serializable]
public class EventOption
{
    public string buttonText;
    public StatModifier immediateEffect;
    public MissionSO linkedMission;   // 可为空
}