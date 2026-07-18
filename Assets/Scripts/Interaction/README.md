# 预制体交换交互

## 目标

玩家进入交互范围后显示“按 E 交互”，按 `E` 打开固定 UI Prefab，点击横向排列的配置项后，替换指定父物体下的内容。

## 结构与使用

- `InteractableObj`：所有交互行为的抽象基类；交互脚本继承它并重写 `ObjectClicked()`。
- `InteractableArea`：与交互脚本挂在同一个游戏物体上，玩家按键后直接调用该物体的 `InteractableObj.ObjectClicked()`，不使用消息广播。
- `InteractableArea` 不维护全局实例集合。`OnTriggerEnter/Exit` 只记录玩家是否仍在范围内以及玩家 Collider 重叠数，`Update` 根据该状态轮询交互输入。
- R 键变色由玩家身上的 `PlayerRangeColorScanner` 负责。它按“检测半径”和“检测层级”执行物理范围扫描，只处理命中的 `InteractableArea`，不遍历全场景交互物。
- `PrefabSwapInteractable`：继承 `InteractableObj`，配置替换目标父物体、界面实例、可选预制体和预览相机参数。
- 每个“可选预制体”可单独填写“配置名称”，该名称会显示在选择按钮上；留空时使用 Prefab 资源名。
- 组件会复用同物体的 `InteractableArea`，交互范围由同物体的 `SphereCollider` 调整。
- 在 Unity 菜单选择 `工具/创建预制体交换面板 Prefab`，生成 `Assets/Prefab/UI/Prefab Swap Panel.prefab`。把它拖入场景，再将其 `PrefabSwapPanelView` 赋给交互组件。
- 面板无背景，6 个选择槽与退出按钮横向排列，退出按钮始终在最后。运行时不创建 UI 节点。
- `预览相机` 可指定玩家相机；留空时使用 `Camera.main`。将场景中的游戏物体赋给 `预览机位物体`，组件会使用该物体的世界位置和旋转作为交互面板打开后的预览机位；退出时恢复交互前的相机姿态。

## 替换规则

选择后会删除目标父物体的现有子物体，实例化选中的 Prefab，并把局部坐标、旋转和缩放标准化为 `(0,0,0)`、单位旋转和 `(1,1,1)`。

## 验证

- 靠近交互物体，确认出现“按 E 交互”。
- 在玩家的 `PlayerRangeColorScanner` 配置“检测层级”，按 R 确认范围内该层级上的交互物及其子级 Renderer 变色。
- 按 `E` 确认玩家输入锁定且鼠标释放。
- 确认相机切到预览机位物体的位置和旋转，移动或旋转该物体时相机同步跟随。
- 确认按钮显示配置名称，点击后正确替换并标准化子物体变换。
- 点击最右侧“退出”，确认面板关闭、相机恢复且重新启用玩家输入。

## 限制

默认界面提供 6 个预制体选择槽；超出部分不会显示。如需更多选项，调整生成器的 `OptionSlotCount` 后重新生成 Prefab。
