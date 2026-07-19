Event、Mission、Cards已经被完全封装成scriptable object（定义在后缀为SO的脚本里），在对应的文件夹里右键create—game创建即可创建，填数据不需要改脚本了。填完数据需要在Hierarchy里做一些关联，不过不做也没什么大不了的，这个关联很简单。

Location改成了场景中固定不动的物体，而不是在UI中动态生成。新增了Diplomac地块类型，可用考虑做一些允许Diplomatic型地块的卡牌。地块具体效果仍然需要通过脚本实现，目前只有一个用来测试的地块，我会写更多地块。

Canvas下的EventPanel和MissionPanel需要修改：在Image里加贴图，调整Title和Description的大小、字体之类的。另外派系声望还没有显示出来，可以加上。

新增了Majesty和Fight两种通过卡牌展示获得的资源，只在展示阶段生效，过回合清0. Majesty用来买牌/删牌，Fight用来抵消任务扣的Military Strength。这两个也还没做UI显示，可以考虑做一下。然后造新牌的时候也要填这两个数据。

我不会再动场景和已有脚本，只会添加新的Event、Mission、Card，以及写一些location effect脚本。所以如果你要修改场景或任何现有脚本，务必告诉我。在合并的时候，新增脚本直接保留，而凡是你改过的旧脚本我都会直接用你的。

----------------------------------------
版本记录
----------------------------------------

2026-07-19 UI 与十回合结局版（a4a47b3）

- 在不改变原有游戏流程的基础上，统一 EventPanel、MissionPanel 和按钮的视觉样式。
- 顶部 HUD 增加回合、大臣、金币、Majesty、Fight、三项资源与三项派系声望显示。
- 最大回合数设为 10；第十回合结束时按照提案中的五组胜利条件结算。
- 结局界面显示“胜利”或“失败”，点击“返回”重新载入场景开始下一局。
- UI 素材来源与许可证记录在 CREDITS.md。
- 修改前版本保存在 Git 标签 main-before-ui-ending-20260719。

2026-07-19 WebGL 发布版

- 使用 Unity 6000.5.3f1 构建 MainMap 场景。
- WebGL 访问地址：https://lyf621.github.io/zebra/game/

----------------------------------------
脚本对接分支说明：script-integration-20260719
----------------------------------------

本分支只修改流程相关脚本，不修改 MainMap 场景、Location prefab、Event/Mission/Card 数据资产或项目设置。
队友在场景中摆放的方块只要带有 ClickOnLocation 组件，就会在游戏启动时被自动收集。

一、需要覆盖的已有脚本

1. Assets/Scripts/Events/EventSO.cs

- EventSO 新增 eventTitleChinese、eventDescriptionChinese。
- EventOption 新增 buttonTextChinese。
- 新增 GetTitle(bool)、GetDescription(bool)、GetButtonText(bool)。
- 修改位置：EventSO 和 EventOption 的字段定义及类末尾。
- 用法：以后创建 EventSO 时在 Inspector 同时填写 English 和 Chinese；中文为空时回退英文。现有 Uprising 测试事件不修改 asset 也有中文回退。

2. Assets/Scripts/Missions/MissionSO.cs

- MissionSO 新增 missionTitleChinese、missionDescriptionChinese。
- MissionResolution 新增 buttonTextChinese。
- 新增 GetTitle(bool)、GetDescription(bool)、GetButtonText(bool)。
- 修改位置：MissionSO 和 MissionResolution 的字段定义及类末尾。
- 用法：以后创建 MissionSO 时在 Inspector 同时填写 English 和 Chinese；中文为空时回退英文。现有 Appease/Suppress 测试任务不修改 asset 也有中文回退。

3. Assets/Scripts/UI/EventPanelUI.cs

- 新增 currentEvent、currentManager、cards，用来保存正在显示的事件。
- 新增 Start()、OnDestroy()，订阅/取消订阅 ZebraGameController.LanguageChanged。
- 修改 Show()、Hide()、BuildOptions()，改为调用 EventSO 的双语读取函数。
- 新增 RefreshLanguage(bool)、ApplyTexts()、SetVisibleForReview(bool)。
- 修改位置：成员变量区域、Show/Hide/BuildOptions，以及 ClearButtons() 前。
- SetVisibleForReview() 只临时隐藏面板，不清除尚未选择的事件按钮。

4. Assets/Scripts/UI/MissionPanelUI.cs

- 新增 currentMission、currentManager、currentResult、showingResult、cards。
- 新增 Start()、OnDestroy()，监听语言切换。
- 修改 Show()、ShowResolutions()、ShowResult()、Hide()、ToggleWindow()。
- 新增 RefreshLanguage(bool)、ApplyTexts()、ApplyResultText()、SetVisibleForReview(bool)。
- 修改位置：成员变量区域，以及上述函数原位置。
- 事件/任务正在等待选择时禁止普通 ShowMission 按钮隐藏决策面板；半返回必须走 DecisionReviewController。

5. Assets/Scripts/GameManager/ZebraGameController.cs

- 在语言与状态字段附近新增 mDecisionReviewMode、LanguageChanged、IsDecisionReviewMode。
- Start() 中必须在 BuildInterface() 前设置 mIntegrated = mTurns != null。
- 在 StartTurnHand() 后新增 SetDecisionReviewMode(bool)、SetLocations(ClickOnLocation[])、CanAdvanceTurnPhase()。
- 修改 OnHandCardClicked()、CanHoverCard()、TryPlayCardOnLocation()，查看模式下禁止卡牌和地块操作。
- BuildInterface() 中，整合模式不再启用全屏 Paper Background 和 Background Shade，避免挡住世界空间地块。
- 修改 CanOpenOverlay()，半返回查看模式仍可查看总牌库、抽牌堆和弃牌堆。
- OpenSettings() 创建的 Settings Overlay 新增独立 Canvas，sortingOrder 为 500，保证设置页面位于最上层。
- SetLanguage() 最后触发 LanguageChanged。
- RefreshInterface() 中，查看模式只能使用查看按钮和设置，买牌、删牌、出牌均禁用。

6. Assets/Scripts/GameManager/TurnController.cs

- 新增 Cards 字段。
- Start() 中查找 ZebraGameController、调用 DecisionReviewController.EnsureExists() 和 RegisterSceneLocations()。
- 新增 SetLocations(ClickOnLocation[])、RegisterSceneLocations()。
- RegisterSceneLocations() 自动收集场景中所有启用的 ClickOnLocation，不改变方块的位置、数量和外观。
- EndTurn()、MovesRunOut() 增加 LocationList 和空对象保护。
- UpdateTurnPhase() 改为根据 ZebraGameController.UseChinese 返回中英文阶段文字。

7. Assets/Scripts/UI/TurnPhaseButton.cs

- 新增 UnityEngine.UI 命名空间和 PhaseButton 字段。
- Awake() 中取得 Button。
- 新增 Update()，根据事件选择、任务选择、卡牌动画、卡牌弹窗、设置、选地块、半返回和游戏结局动态设置 interactable。
- HandleTurnPhase() 开头增加同样的状态保护，避免通过代码绕过按钮禁用状态。

8. Assets/Scripts/UI/MainMapUIController.cs

- 新增 mEvents、mMissions、mShowMissionButton。
- Start() 中查找 EventManager 和 MissionManager。
- Update() 中根据当前任务、决策界面、半返回模式及结局状态设置 ShowMissionButton.interactable。
- StyleMainButtons() 中保存 ShowMissionButton 的 Button 引用。

二、需要新增的脚本

9. Assets/Scripts/UI/DecisionReviewController.cs

- 新增文件，不需要手动挂到场景对象。
- TurnController.Start() 会通过 DecisionReviewController.EnsureExists() 自动创建。
- 事件或任务等待选择时显示“暂时返回查看 / Review Cards and Map”。
- 点击后临时隐藏决策面板，只允许查看卡牌、牌堆、地块和设置；再次点击恢复原决策面板及未完成的选项。
- 半返回按钮 Canvas sortingOrder 为 400；设置页面为 500。

三、场景与 Inspector 操作

- 不需要修改 MainMap 场景。
- 不需要把 DecisionReviewController 手动挂到任何对象。
- 不需要手动维护 TurnController.LocationList；启动时会自动收集全部 ClickOnLocation。
- 以后新增事件或任务，只需在对应 ScriptableObject 的 Inspector 填写新增的 Chinese 字段。
- 如果只手动复制脚本：覆盖以上 8 个已有 .cs，并新增 DecisionReviewController.cs。Unity 会自动生成新脚本的 .meta。
- 如果直接合并本分支：DecisionReviewController.cs 和它的 .meta 会一起进入项目。

四、验证结果

- 使用 Unity 6000.5.3f1 编译通过，无 C# 错误。
- 本地场景临时放入 4 个带 ClickOnLocation 的方块，4 个均被 TurnController 自动注册。
- 方块占用和回合重置测试通过。
- 现有测试事件在不修改 .asset 的情况下可以取得中文回退。
- 临时测试脚本、场景、Prefab、Library、Logs 和构建文件均未提交。
