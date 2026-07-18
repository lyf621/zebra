# Zebra 整合版游戏

Unity 版本：`6000.5.3f1`

在 Unity Hub 中将 `UnityProject` 作为项目根目录打开。

## 整合版游戏 V1

- 开始回合后抽取五张牌，手牌上限为十张。
- 抽牌堆为空时，将弃牌堆重新洗入抽牌堆。
- 在行动阶段查看总牌库、抽牌堆和弃牌堆。
- 购买金色皇家牌并加入弃牌堆。
- 在可滚动的总牌库视图中，从任意持有区域删除选中的卡牌。
- 选中一张手牌，再次点击后选择匹配的地块。
- 放置大臣并结算经济、军事或行政地块效果。
- 结束回合时逐张展示保留的卡牌，结算保留效果，将其弃置并开始下一回合。

`integrated-game-v1` Git 标签标记了中英文与鼠标交互升级前、已经测试的基础版本。

## 整合版游戏 V2

- 点击齿轮按钮打开设置面板，并切换中英文。
- 鼠标悬停在手牌上时，卡牌会抬起、放大并显示在相邻卡牌上方。
- 点击一次手牌后，卡牌会跟随鼠标。
- 再次点击已选中的卡牌会停止跟随鼠标并进入地块选择。
- 选择打出地点时，高亮显示可用地块。

`integrated-game-v2` Git 标签标记了已经测试的中英文与鼠标交互升级版本。

## 整合版游戏 V3

- 将手牌排成平滑向下的扇形，并确保十张牌都完整保留在游戏画面中。
- 玩家点击已被占用的地块时，取消本次出牌并将选中的卡牌返回手牌。

`integrated-game-v3` Git 标签标记了手牌布局与占用地块问题的修复版本。

## 整合版游戏 V4

- 右键点击已选中的卡牌可取消选择并恢复手牌布局。
- 当大臣用完、没有匹配的空地块，或玩家选择了无效地块时，将卡牌返回手牌。

`integrated-game-v4` Git 标签标记了取消选牌与无可用地块问题的修复版本。

## 队友对接清单

1. 在 `Assets/Scripts/IntegrationPlaceholderMode.cs` 中将 `Enabled` 改为 `false`。
2. 在 `ZebraGameController.CreateInitialCards()` 中替换临时初始牌库和可购买的皇家牌数据。
3. 在 `ZebraGameController.BuildInterface()` 的三个 `LocationView.Create(...)` 调用中替换地块名称、位置、视觉和说明。
4. 在 `ZebraGameController.ApplyLocationEffect()` 中接入地块触发的资源变化。
5. 在 `ZebraGameController.ApplyRetainEffect()` 中接入手牌保留时的资源变化。
