# AI Context: 世界说明 UI

一句话接入：在 `UIRoot/WorldCanvas/TipUI` 下准备名为 `Title`、`Description` 的 TMP 文本，禁用模板对象，场景物体挂 `WorldDescriptionUI` 并填写内容即可。

## 职责

- `WorldDescriptionUI` 是场景物体上的数据与显示控制组件。
- 脚本不会创建 UI 结构，只通过 `UIManager` 复制和移除现有模板实例。
- `TipUI` 模板负责 Canvas、背景、布局、字体、字号、颜色和缩放。

## 数据流

1. `UIManager` 查找 `UIRoot/WorldCanvas` 的直接子物体 `TipUI` 模板。
2. `WorldDescriptionUI` 需要显示时调用 `UIManager.AddWorldUI("TipUI")`，再从实例中取得 `Title` 和 `Description`。
3. `PlayerDetailInspectView` 命中说明物体后调用 `SetDetailInspectHighlighted(true)`。
4. 当前组件把标题与说明复制到实例，将实例移动到 `transform.TransformPoint(worldOffset)`，并按配置朝向相机。
5. 隐藏、失效或移出距离时调用 `UIManager.RemoveWorldUI(...)` 移除对应实例。

## 扩展约束

- 不要绕过 `UIManager` 自行查找、实例化或销毁 World UI。
- 调整视觉样式时直接修改 `TipUI` 模板。
- 修改固定节点名称或层级时同步更新模板查找规则。
