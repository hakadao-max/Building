# AI Context: 预制体交换交互

一句话接入：给交互物体挂 `PrefabSwapInteractable`，配置 `targetParent`、`PrefabSwapOption` 列表、`PrefabSwapPanelView` 实例以及提供预览位置与旋转的场景游戏物体。

## 关键类

- `InteractableObj`：交互行为抽象基类，派生类通过重写 `ObjectClicked()` 实现具体行为。
- `InteractableArea`：用 Trigger 进入/退出事件保存范围状态，在 `Update` 轮询按键并直接调用同物体上的 `InteractableObj`；无静态实例集合或焦点字典。
- `PlayerRangeColorScanner`：位于玩家对象上的公共范围变色组件，使用 `OverlapSphereNonAlloc` 和可配置 `LayerMask` 扫描附近交互物。
- `PrefabSwapInteractable`：处理范围提示、`ObjectClicked` 交互、玩家输入锁定、预览相机姿态和预制体替换。执行顺序为 1000，确保其 `LateUpdate` 在玩家相机刷新后保持预览机位。
- `PrefabSwapOption`：一个可配置的按钮名称与 Prefab 引用。
- `PrefabSwapPanelView`：固定 UI Prefab 的引用集合，不创建运行时 UI。
- `PrefabSwapPanelPrefabCreator`：编辑器菜单生成无背景、横向排列、退出在末尾的 `Assets/Prefab/UI/Prefab Swap Panel.prefab`。

## 数据流

1. `InteractableArea.OnTriggerEnter/Exit` 保存玩家范围状态；仅在范围状态有效时于 `Update` 检查 E，并直接调用同物体上 `InteractableObj.ObjectClicked()`。
2. 玩家按 R 时，`PlayerInteractionHintInput` 调用 `PlayerRangeColorScanner.ScanAndShowColorHints()`，对指定层级和半径进行物理扫描，命中交互物后由 `TemporaryColorHint` 为目标及其子级 Renderer 变色。
3. `PrefabSwapInteractable` 显示预先实例化在场景中的面板，锁定玩家、释放鼠标，保存当前相机姿态，并让相机持续跟随 `previewPoseObject` 的世界位置与旋转。
4. 按钮使用 `PrefabSwapOption.DisplayName`；留空时回退到 Prefab 资源名。
5. 选择后清空 `targetParent` 子物体，实例化目标 Prefab 并归零局部位置/旋转，缩放设为一。
6. 最后一个退出按钮关闭面板，恢复交互前相机姿态与玩家输入。
