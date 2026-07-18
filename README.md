# Zebra 数字原型

Zebra 期末游戏项目的独立可行性数字原型。

## Unity 版本

使用 Unity `6000.5.3f1`。

## 项目

1. `prototypes/01-card-turn-cycle/UnityProject`
   - 从七张牌的牌库中抽取三张。
   - 结束回合后弃掉手牌并再次抽牌。
   - 抽牌堆不足时，将弃牌堆洗回抽牌堆。

2. `prototypes/02-card-play-reveal/UnityProject`
   - 点击一次选中并抬起卡牌。
   - 再次点击已选中的卡牌即可打出。
   - 结束回合后逐张展示剩余手牌。

每个 `UnityProject` 文件夹只包含 `Assets`、`Packages` 和 `ProjectSettings`。请在 Unity Hub 中添加该准确文件夹；Unity 会自动重新生成 `Library`、`Logs`、`Temp` 与本地用户设置。

## WebGL

GitHub Pages 文件存放在 `docs/` 下。

- [整合版游戏](https://lyf621.github.io/zebra/game/)
- [原型 01：回合抽牌](docs/prototypes/01-card-turn-cycle/)
- [原型 02：打牌与展示](docs/prototypes/02-card-play-reveal/)
