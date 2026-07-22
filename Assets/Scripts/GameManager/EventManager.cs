using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventManager : MonoBehaviour
{
    [Header("Event data (one pool per turn; element 0 = turn 1, element 1 = turn 2, ...)")]
    [SerializeField] private TurnEventPool[] eventPoolsByTurn;

    [Header("Wiring")]
    [SerializeField] private EventPanelUI eventPanel;
    [SerializeField] private MissionManager missionManager;
    [SerializeField] private StatManager stats;
    [SerializeField] private ZebraGameController cards;
    [SerializeField] private TurnController turns;

    private EventSO currentActiveEvent;

    // 每回合一个取事件方法；用 turns.GetTurnCount() 作为下标调用（在 Awake 中构建——委托无法在 Inspector 赋值）。
    private System.Func<List<EventSO>, EventSO>[] eventPickers;

    // True while the event panel is open and waiting for the player to pick an option.
    private bool awaitingChoice = false;
    public bool IsAwaitingChoice() { return awaitingChoice; }

    private void Awake()
    {
        // Fallbacks so the system still works if a reference was left unassigned.
        if (stats == null) stats = FindAnyObjectByType<StatManager>();
        if (missionManager == null) missionManager = FindAnyObjectByType<MissionManager>();
        if (cards == null) cards = FindAnyObjectByType<ZebraGameController>();
        if (turns == null) turns = FindAnyObjectByType<TurnController>();
        BuildEventPickers();
    }

    /// <summary>Pick a random event and open the event panel. Called on the Turn Phase 0 button.</summary>
    public void TriggerRandomEvent()
    {
        List<EventSO> pool = GetPoolForCurrentTurn();
        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning("EventManager: this turn's event pool is empty — skipping the event.");
            return;
        }

        currentActiveEvent = PickEventForCurrentTurn(pool);
        if (currentActiveEvent == null)
        {
            Debug.LogWarning("EventManager: the picker returned no event this turn — skipping.");
            return;
        }

        awaitingChoice = true;
        if (eventPanel != null) eventPanel.Show(currentActiveEvent, this);
    }

    // 按当前回合选择对应的取事件方法；下标与 eventPoolsByTurn 对齐，超范围时用最后一个方法。
    private EventSO PickEventForCurrentTurn(List<EventSO> pool)
    {
        if (eventPickers == null || eventPickers.Length == 0) return PickRandom(pool);
        int turn = turns != null ? turns.GetTurnCount() : 1;
        int index = Mathf.Clamp(turn - 1, 0, eventPickers.Length - 1);
        var picker = eventPickers[index];
        return picker != null ? picker(pool) : PickRandom(pool);
    }

    // 取当前回合对应的事件池：第 1 回合用元素 0，以此类推；回合超出数组范围时沿用最后一个池。
    private List<EventSO> GetPoolForCurrentTurn()
    {
        if (eventPoolsByTurn == null || eventPoolsByTurn.Length == 0) return null;
        int turn = turns != null ? turns.GetTurnCount() : 1;         // 回合从 1 开始计数
        int index = Mathf.Clamp(turn - 1, 0, eventPoolsByTurn.Length - 1);
        return eventPoolsByTurn[index] != null ? eventPoolsByTurn[index].events : null;
    }

    // 构建每回合的取事件方法数组（委托不能在 Inspector 赋值，必须在代码中构建）。
    // 顺序需与 eventPoolsByTurn 对齐：PickTurn1 对应第 1 回合，以此类推。
    private void BuildEventPickers()
    {
        eventPickers = new System.Func<List<EventSO>, EventSO>[]
        {
            PickTurn1, PickTurn2, PickTurn3, PickTurn4, PickTurn5,
            PickTurn6, PickTurn7, PickTurn8, PickTurn9, PickTurn10
        };
    }

    // 十个回合各自的取事件方法。目前都是简单随机，之后可逐个替换为不同逻辑。
    private EventSO PickTurn1(List<EventSO> pool)  { return PickRandom(pool); }
    private EventSO PickTurn2(List<EventSO> pool)  { return PickRandom(pool); }
    private EventSO PickTurn3(List<EventSO> pool)  { return PickRandom(pool); }
    private EventSO PickTurn4(List<EventSO> pool)  { return PickRandom(pool); }
    private EventSO PickTurn5(List<EventSO> pool)  { return PickByStat(stats != null ? stats.GetPO() : 5, 6, 4, pool); }
    private EventSO PickTurn6(List<EventSO> pool)  { return PickByStat(stats != null ? stats.GetMS() : 5, 6, 4, pool); }
    private EventSO PickTurn7(List<EventSO> pool)  { return PickByStat(stats != null ? stats.GetAL() : 5, 6, 4, pool); }
    private EventSO PickTurn8(List<EventSO> pool)  { return PickByRelation(stats != null ? stats.AristocratRel() : 3, pool); }
    private EventSO PickTurn9(List<EventSO> pool)  { return PickByRelation(stats != null ? stats.ChurchRel() : 3, pool); }
    private EventSO PickTurn10(List<EventSO> pool) { return PickByRelation(stats != null ? stats.KingRel() : 3, pool); }

    // 依据数值取事件：value > high 用 pool[0]，value < low 用 pool[1]，介于两者之间时随机。
    // 对 pool[1] 做越界保护，避免该回合事件池不足两个时报错。
    private EventSO PickByStat(int value, int high, int low, List<EventSO> pool)
    {
        if (pool == null || pool.Count == 0) return null;
        if (value > high) return pool[0];
        if (value < low) return pool.Count > 1 ? pool[1] : pool[0];
        return PickRandom(pool);
    }

    // 关系值版本：>3 用 pool[0]，<3 用 pool[1]，=3 随机。
    private EventSO PickByRelation(int rel, List<EventSO> pool)
    {
        return PickByStat(rel, 3, 3, pool);
    }

    // 从事件池中等概率随机取一个。
    private EventSO PickRandom(List<EventSO> pool)
    {
        if (pool == null || pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }

    /// <summary>Called by EventPanelUI when the player clicks one of the option buttons.</summary>
    public void OnOptionSelected(int optionIndex)
    {
        if (currentActiveEvent == null) return;
        if (optionIndex < 0 || optionIndex >= currentActiveEvent.availableOptions.Count) return;

        EventOption option = currentActiveEvent.availableOptions[optionIndex];

        // 1. Apply the immediate stat change.
        if (stats != null) option.immediateEffect.ApplyTo(stats);

        // 2. Close the event panel.
        awaitingChoice = false;
        if (eventPanel != null) eventPanel.Hide();

        // 3. Hand the linked mission (if any) to the MissionManager, which opens the mission panel.
        if (option.linkedMission != null && missionManager != null)
            missionManager.SetCurrentMission(option.linkedMission);

        currentActiveEvent = null;

        // 4. The event is resolved (Turn Phase 1): let the player play the hand drawn at turn start.
        if (cards != null) cards.EnableCardPlay();
    }
}

// 每回合一个事件池。Unity 无法在 Inspector 中直接序列化二维数组（EventSO[,] 或 EventSO[][]），
// 因此用可序列化的包装类：一个数组元素代表一个回合，内部再放该回合的事件列表。
[System.Serializable]
public class TurnEventPool
{
    [Tooltip("本回合可能触发的事件。")]
    public List<EventSO> events = new List<EventSO>();
}
