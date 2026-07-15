# 玩家视角、固定路线漫游、固定视角与小地图传送

## 目标

将玩家控制拆成“调度器 + 具体视角实现”：

- `SimplePlayerController`：处理数字键切换、公共移动、交互、手电筒和出生点传送。
- `PlayerFirstPersonView`：处理第一人称相机位置、鼠标视角和第一人称移动方向。
- `PlayerThirdPersonView`：处理第三人称相机环绕、可视模型、动画同步和第三人称移动方向。
- `PlayerFixedRouteRoamView`：处理按固定曲线自动漫游、路线预览和运行时路径播放。
- `PlayerFixedCameraView`：处理底部固定视角图标栏、点击进入固定视角和小范围视角旋转。
- `PlayerMinimapTeleportView`：处理小地图覆盖显示、点击位置到世界坐标的映射和传送点计算。
- `PlayerDetailInspectView`：处理详情查看开关、屏幕十字标和准星射线检测，驱动 `WorldDescriptionUI` 在开启时显示说明。
- `PerspectivePickupObject`：处理刚体拾取、中心视线跟随、透视缩放和释放。
- `PlayerModeDisplay`：`UIRoot/Canvas` 下的模式 `UIPanel`，显示当前模式文字。
- `PlayerInteractionPromptDisplay`：`UIRoot/Canvas` 下的交互提示 `UIPanel`，显示最近 `InteractableArea` 的提示文本。

## 使用方式

把 `Assets/Prefab/First Third Person Controller.prefab` 拖入场景即可使用。默认视角是第一人称，运行后按 `1` 切换到第一人称，按 `2` 切换到第三人称，按 `3` 进入固定路线漫游，按 `4` 显示固定视角图标栏，按 `5` 显示小地图传送视图。只有第一人称下按 `6` 才会开关详情查看。

固定路线漫游在 `PlayerFixedRouteRoamView` 上配置。控制点是绝对世界坐标，运行时会按 `漫游速度` 沿这些世界坐标点组成的曲线移动。`首尾相连` 控制最后一个控制点是否连回第一个控制点，`循环播放` 只控制播放到终点后是否重新开始。选中该组件后，可在 Scene 视图拖拽黄色控制点编辑曲线。关闭 `沿路线方向旋转` 时，漫游中鼠标会变成自由视角控制。

固定视角在 `PlayerFixedCameraView` 上配置。给 `固定视角点` 添加多个条目，每个条目指定一个视角 Transform 和图标；运行后按 `4` 会立即把左上角提示更新为模式 4，并在屏幕底部显示图标栏。点击后进入对应固定视角。默认选中视角后保持图标栏显示并保留鼠标操作，此时按住鼠标右键拖动可在配置范围内小幅转动相机，左键仍用于选择 UI；勾选“选择后隐藏面板”后，面板收起且鼠标锁定，可直接转动视角。固定视角不会移动玩家，只会移动玩家相机。

小地图传送在 `PlayerMinimapTeleportView` 上配置。`小地图图片` 是显示在屏幕上的地图，`地图世界中心` 和 `地图世界尺寸` 决定图片坐标到世界 XZ 坐标的映射。运行后按 `5` 打开小地图，点击图片后会把玩家传送到对应世界位置，并按 `贴地检测层` 尝试修正到地面。

详情查看在 `PlayerDetailInspectView` 上配置。第一人称下按 `6` 开关详情查看，屏幕中央会出现或隐藏十字标；切换到其他视角时会自动关闭，其他视角按 `6` 无响应。十字标射线在 `检测距离` 内命中带 `WorldDescriptionUI` 的物体时，会通过 `UIManager` 从 `UIRoot/WorldCanvas/TipUI` 复制显示实例并应用该物体的标题和说明。场景物体上的 `WorldDescriptionUI` 默认勾选 `仅详情查看时显示`，平时不会自动弹出。

### 交互配置归属

`SimplePlayerController` 只保存交互输入键，不再保存交互半径、交互检测层、提示显示半径或提示持续时间。普通交互物的有效范围由该物体 `InteractableArea` 同对象上的 `SphereCollider` 决定；调整球形触发器的半径即可调整玩家靠近后可交互和显示按键提示的范围。

按 R 显示提示时，每个 `InteractableArea` 分别使用自己的“提示显示距离”和“提示持续时间”。`WorldDescriptionUI` 也保存自己的同名提示配置，因此不同物体可以使用不同范围和时长。交互物是否参与物理射线仍由物体自身的 Layer 和 Collider 决定。

`SimplePlayerController` Inspector 已按视角模式、按键、移动、手电筒和自动组件引用分区。视角与提示组件会自动查找或补齐，“自动组件引用（只读）”仅用于确认运行时绑定，无需手动拖拽。

### 透视拾取

给目标物体添加 Collider、Rigidbody 和 `PerspectivePickupObject`，在该拾取物上配置“可拾取距离”。先按 `7` 进入透视拾取模式，再对准距离内的物体按 E 拾取，再按 E 释放。模式 1 等其他模式不会拾取透视物体；切离模式 7 时会自动释放当前物体。物体沿当前相机中心视线移动：视线没有命中障碍时位于“最大跟随距离”，命中时位于最近障碍之前；同时按 `拾取时缩放 × 当前相机距离 ÷ 拾取时相机距离` 等比缩放，使屏幕视觉尺寸近似不变。释放后恢复非运动学刚体和重力，并限制最大下落速度。

组件以中心射线为主：先从相机中心沿 forward 发射 Raycast，得到最近命中距离或最大跟随距离；随后在该候选位置执行多次 `OverlapBoxNonAlloc`。每一次 Box 检测都会根据候选距离重新计算 `拾取时缩放 × 当前距离 ÷ 拾取时距离`，因此检测盒与最终物体一样近处小、远处大。发生场景重叠时，候选位置按“Box前移步长”沿视线向相机方向移动，再用缩小后的新 Box 复检。玩家与物体自身 Collider 会被过滤。物体位置仍使用指数平滑跟随；位置更新后，缩放直接按物体与相机的实际距离计算，不再独立插值，避免缩放比相机移动慢一拍。

`SimplePlayerController` 通过 `UIManager` 获取 `PlayerModePanel`，不会在玩家对象上补组件或创建模式文字。模式 1 至模式 7 的显示信息配置在 `PlayerModeDisplay`；颜色、字号、偏移和布局直接维护在面板的 TMP 与 RectTransform 上。

### 普通交互提示

`InteractableArea` 的“提示文本”会在玩家进入交互范围时显示在屏幕下方，多个区域重叠时显示距离玩家最近的一项。离开范围、文本为空、输入被锁定或进入固定路线、固定视角、小地图模式时自动隐藏。控制器统一把内容交给 `UIRoot/Canvas` 下的 `InteractionPromptPanel`；交互物和玩家对象不再挂专用提示 Canvas。

模式提示、交互提示、详情十字标、固定视角按钮和 EventSystem 都必须预置在 `UIRoot/Canvas` 或场景中。玩家代码不会运行时创建任何 UI 层级。世界说明牌统一通过 `UIManager` 复制 `UIRoot/WorldCanvas/TipUI` 下预制的 TMP 模板。

项目 TMP 默认字体统一设置为 `Assets/SourceHanSansSC-Medium SDF.asset`。运行时创建的 TMP 文本通过 `TMP Settings` 的直接资产引用获得该字体，不使用 `Resources.Load` 重复加载；没有单独指定字体的 TMP 组件都会继承此默认字体。

## 测试入口

Unity 菜单 `测试/玩家视角与漫游测试窗口` 可给选中对象补齐并绑定玩家视角组件。`PlayerFixedRouteRoamView` Inspector 中提供“追加控制点”和“重置默认路线”，Scene 视图提供曲线控制点拖拽。Unity 菜单 `工具/小地图生成工具` 可框选区域、生成小地图 PNG，并直接应用到 `PlayerMinimapTeleportView`。透视拾取依赖玩家相机和物理场景，直接使用 Play Mode 测试。

## 验证建议

- 进入 Play Mode 后确认默认是第一人称。
- 按 `1`、`2`、`3`、`4`、`5`、`6` 分别确认第一人称、第三人称、固定路线漫游、固定视角图标栏、小地图传送和详情查看。
- 第三人称下移动时确认模型出现并播放 `idle1`、`walk`、`run`。
- 选中 `PlayerFixedRouteRoamView` 拖拽 Scene 控制点，确认预览曲线实时更新。
- 切换 `首尾相连`，确认关闭时曲线停在最后一个控制点，不再连接回第一个控制点。
- 漫游结束后默认回到第一人称；如果需要循环播放，勾选 `循环播放`。
- 关闭 `沿路线方向旋转` 后进入漫游，确认玩家仍沿路线移动，但鼠标可以自由控制视角。
- 给 `PlayerFixedCameraView` 添加固定视角点后按 `4`，点击底部图标进入固定视角，确认不能移动且只能小范围旋转。
- 用 `工具/小地图生成工具` 生成并应用地图后按 `5`，点击小地图确认玩家传送到对应区域。
- 第一人称下按 `6` 开启详情查看，再按一次确认十字标关闭；切换到其他视角确认自动关闭且按 `6` 无响应。
- 给场景物体添加 `WorldDescriptionUI` 后，在第一人称下开启详情查看并用十字标对准物体，确认说明牌只在开启时出现。
- 模式 1 对准透视拾取物体按 E，确认不会拾取；按 7 后再按 E，移动和转动视角，确认其屏幕尺寸近似不变；再次按 E，确认物体受重力下落。
- 切换 1 至 7 的功能时，确认左上角显示对应的自定义模式信息。

## 限制

固定路线漫游直接移动玩家 Transform，并临时关闭 `CharacterController` 来设置位置，不做 NavMesh 避障。路线高度来自控制点，是否贴地需要通过控制点本身或外部地形逻辑处理。

玩家输入通过 `RuntimeInput` 兼容新 Input System 和旧版 Input Manager。新 Input System 下移动只映射 WASD 和方向键；如果后续需要手柄或自定义 InputAction，需要扩展 `RuntimeInput` 或替换为正式输入绑定。

透视拾取的障碍检测使用相机中心射线，不计算任意模型完整包围体；特别大的模型边缘仍可能穿模。非均匀初始缩放会保持原有各轴比例。
