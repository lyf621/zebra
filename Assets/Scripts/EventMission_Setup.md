# Event & Mission System — Unity Setup Guide

This system controls the Events and Missions in game. Your ScriptableObject layer
(`EventSO`, `MissionSO`, `StatModifier`) was **not changed**. Five scripts do the work:

| Script | Location | Role |
|--------|----------|------|
| `EventPanelUI.cs` | `Scripts/UI` | The event popup: title, description, one button per option |
| `MissionPanelUI.cs` | `Scripts/UI` | The mission panel: title/description, show-hide toggle, resolution buttons, result |
| `EventManager.cs` | `Scripts/GameManager` | Picks a random event, applies the option effect, hands off the linked mission |
| `MissionManager.cs` | `Scripts/GameManager` | Holds the active mission, reveals resolutions in Phase 2, applies the outcome |
| `TurnPhaseButton.cs` | `Scripts/UI` | Drives the flow off the single Turn Phase button, with guards |

## Flow

```
Phase 0  [Turn Phase Button] -> random Event panel opens
                              -> player picks an option
                              -> option effect applied, event closes,
                                 linked Mission panel opens (display only)
Phase 1  [Turn Phase Button] -> reveal step (advances only)
Phase 2  [Turn Phase Button] -> Mission resolution buttons appear
                              -> player reacts, outcome applied + result shown
Phase 3  [Turn Phase Button] -> End Turn (clears mission, hides panels, back to Phase 0)
```

The button will **not** advance while an event option or a mission resolution is still
pending — the player must choose first. The mission panel can be shown/hidden any time
via a separate button wired to `MissionPanelUI.ToggleWindow()`.

---

## 1. Build the option-button prefab (used by both panels)

1. In the Hierarchy, create a **UI > Button - TextMeshPro**. Name it `OptionButton`.
2. Make sure it has a `Button` component and a `TMP_Text` child (the default label works).
3. Drag it into `Assets/Scripts/Resources/Prefabs` (or any Prefabs folder) to make a prefab, then delete it from the scene.

You can use this one prefab for both event options and mission resolutions, or make two if you want different styling.

## 2. Build the Event panel

1. Under your Canvas, create an empty UI object `EventPanel` (this is the **window / visual root**).
2. Add a background `Image`, a `TMP_Text` for the **Title**, a `TMP_Text` for the **Description**.
3. Add an empty child `OptionContainer` and put a **Vertical Layout Group** on it (this is where option buttons spawn).
4. Add the **`EventPanelUI`** component to a manager object (e.g. an empty `EventPanel_Controller`, kept always active — not the window itself). Assign in the Inspector:
   - `Panel Root` = the `EventPanel` window object
   - `Title Text`, `Description Text` = the two TMP texts
   - `Option Container` = `OptionContainer`
   - `Option Button Prefab` = the `OptionButton` prefab

> Keep the `EventPanelUI` script on an object that stays active. `Panel Root` is the thing that gets shown/hidden.

## 3. Build the Mission panel

1. Under your Canvas, create `MissionPanel` (the window / visual root).
2. Add background + `TMP_Text` Title + `TMP_Text` Description + (optional) `TMP_Text` **Result**.
3. Add an empty child `ResolutionContainer` with a **Vertical Layout Group**.
4. Add the **`MissionPanelUI`** component to a manager object (e.g. `MissionPanel_Controller`, kept always active). Assign:
   - `Panel Root` = the `MissionPanel` window
   - `Title Text`, `Description Text`, `Result Text`
   - `Resolution Container` = `ResolutionContainer`
   - `Resolution Button Prefab` = the button prefab
5. **Show/Hide toggle:** add a UI Button anywhere (e.g. `ToggleMissionButton`). In its `OnClick`, drag the `MissionPanel_Controller` and select `MissionPanelUI.ToggleWindow`.

## 4. Wire the managers

1. On your `EventManager` object, assign:
   - `Event Pool` = your array of `EventSO` assets (unchanged from before)
   - `Event Panel` = the `EventPanelUI` component
   - `Mission Manager` = the `MissionManager` component
   - `Stats` = your `StatManager`
2. On your `MissionManager` object, assign:
   - `Mission Panel` = the `MissionPanelUI` component
   - `Stats` = your `StatManager`

(If you leave `Stats` / `Mission Manager` empty, they auto-find at runtime, but assigning is faster and safer.)

## 5. Wire the Turn Phase button

1. Select the object with `TurnPhaseButton`. Assign:
   - `Turns` = your `TurnController`
   - `Events` = the `EventManager`
   - `Missions` = the `MissionManager`
2. On the actual UI **Button**, in `OnClick`, keep/point it to `TurnPhaseButton.HandleTurnPhase`.

## 6. Test checklist

- Phase 0: click the Turn Phase button → event panel appears with option buttons.
- Click an option → event closes, mission panel appears; stats change.
- Try the toggle button → mission panel hides/shows.
- Advance to Phase 2, click the Turn Phase button → resolution buttons appear.
- Pick a resolution → result text shows, stats change.
- Phase 3 click → panels clear, turn ends, back to Phase 0.

## Notes for the future cards-system merge

- Both panel scripts are pure view logic (no rules), so they can be reused or swapped.
- `EventManager` / `MissionManager` are the single entry points; a card system can call
  `SetCurrentMission()` or `TriggerRandomEvent()` the same way the turn button does.
- `awaitingChoice` / `awaitingResolution` flags are the hooks that gate turn progression —
  reuse them if cards also need to block the phase button.
