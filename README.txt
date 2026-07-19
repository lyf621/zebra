Event、Mission、Cards已经被完全封装成scriptable object（定义在后缀为SO的脚本里），在对应的文件夹里右键create—game创建即可创建，填数据不需要改脚本了。填完数据需要在Hierarchy里做一些关联，不过不做也没什么大不了的，这个关联很简单。

Location改成了场景中固定不动的物体，而不是在UI中动态生成。新增了Diplomac地块类型，可用考虑做一些允许Diplomatic型地块的卡牌。地块具体效果仍然需要通过脚本实现，目前只有一个用来测试的地块，我会写更多地块。

Canvas 下的 EventPanel 和 MissionPanel 已统一为羊皮纸风格，并调整 Title、Description、选项按钮的尺寸和字体。顶部 HUD 已显示派系声望。

新增了 Majesty 和 Fight 两种通过卡牌展示获得的资源，只在展示阶段生效，过回合清 0。Majesty 用来买牌/删牌，Fight 用来抵消任务扣的 Military Strength。顶部 HUD 已显示这两项数据，造新牌时仍需填写对应数据。

最大回合数为 10。第十回合结束时会按照提案中的资源和派系声望条件判断胜负，并显示结局界面；点击“返回”会重新载入场景并开始下一局。

UI 素材来源与许可证记录在 `CREDITS.md`。修改前的 main 版本保存在 Git 标签 `main-before-ui-ending-20260719`。

我不会再动场景和已有脚本，只会添加新的Event、Mission、Card，以及写一些location effect脚本。所以如果你要修改场景或任何现有脚本，务必告诉我。在合并的时候，新增脚本直接保留，而凡是你改过的旧脚本我都会直接用你的。
