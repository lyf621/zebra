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
