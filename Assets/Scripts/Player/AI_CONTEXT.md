# AI Context: 玩家视角与固定路线漫游

一句话接入：场景里放 `First Third Person Controller.prefab`，运行后 `SimplePlayerController` 调度 `1/2/3/4/5/7` 切换模式；仅在第一人称下，`6` 开关详情查看；只有模式 7 才能用 E 拾取和释放 `PerspectivePickupObject`。

## 关键类

- `SimplePlayerController`：只负责调度输入、公共移动、交互、手电筒、出生点传送。不要把第一/第三人称相机细节再塞回这里。
- `PlayerFirstPersonView`：第一人称相机 rig、鼠标 yaw/pitch、第一人称移动方向。
- `PlayerThirdPersonView`：第三人称相机 rig、模型显示隐藏、第三人称移动方向和动画同步；动画参数与视角/模型参数用 Header 分区，`Animator` 直接从生成出的模型实例中查找。
- `PlayerFixedRouteRoamView`：Catmull-Rom 曲线漫游、世界坐标控制点预览、按曲线采样长度和 `漫游速度` 推进的运行时路径播放；`首尾相连` 控制曲线是否闭合，`循环播放` 只控制到终点后的播放行为。
- `PlayerFixedCameraView`：固定视角选择和播放组件，按 `4` 立即刷新模式提示并显示屏幕底部图标栏，点击图标后把玩家相机切到配置的固定视角点；初始化是幂等的，重复确保组件不会隐藏已打开的面板。面板显示时按住右键拖动小范围观察，左键操作 UI；面板隐藏时锁定鼠标直接观察。
- `PlayerFixedCameraPoint`：固定视角配置项，保存显示名称、视角 Transform 和图标 Sprite。
- `PlayerMinimapTeleportView`：小地图传送组件，按 `5` 显示小地图图片，点击图片后按配置的世界中心和尺寸映射成世界坐标，并通过射线贴地后传送。
- `PlayerDetailInspectView`：详情查看组件，仅在第一人称下响应 `6` 并开关屏幕十字标；离开第一人称会自动关闭。从相机中心射线检测 `检测距离` 内的 `WorldDescriptionUI`，只在开启时高亮显示说明牌。
- `PerspectivePickupObject`：中心 Raycast 决定基础距离，随后执行有限次数 `OverlapBoxNonAlloc`。若重叠则按固定步长向相机前移；每次检查必须先按新的候选距离重算 scaleMultiplier、Box 中心偏移和 halfExtents，保证检测尺寸始终与“近小远大”的最终缩放一致。位置平滑完成后，最终缩放必须直接按物体与相机的实际距离计算，不能再对缩放做独立插值，否则视觉尺寸会滞后于相机移动。
- `PlayerModeDisplay`：左上角模式提示；各模式文字和显示样式均可在 Inspector 配置，也可绑定已有 TMP_Text。
- `PlayerInteractionPromptDisplay`：屏幕下方普通交互提示，消费最近 `InteractableArea.PromptText`，支持自动创建或绑定已有 TMP_Text；挂在玩家上作为共享回退，挂在 `InteractableArea` 同对象上时作为该区域优先使用的专用显示。
- TMP 默认字体由 `Assets/TextMesh Pro/Resources/TMP Settings.asset` 直接引用 `Assets/SourceHanSansSC-Medium SDF.asset`，运行时动态创建文本不要再用 `Resources.Load` 加载字体。
- `MinimapGeneratorWindow`：菜单 `工具/小地图生成工具`，支持在 Scene 视图框选区域、从上向下生成小地图 PNG，并把图片与世界范围写回 `PlayerMinimapTeleportView`。
- `PlayerFixedRouteRoamViewEditor`：自定义 Inspector 和 SceneView 控制点拖拽。
- `PlayerViewModeTestWindow`：`测试/玩家视角与漫游测试窗口`，用于给选中对象补齐并绑定组件。
- `RuntimeInput`：键鼠输入兼容层，新 Input System 下读取 `Keyboard.current` / `Mouse.current`，旧输入启用时才调用 `UnityEngine.Input`。

## 数据流

1. `SimplePlayerController.Update()` 先处理 `1/2/3/4/5/6` 视角与详情查看输入。
2. 第一/第三人称模式下，控制器通过 `RuntimeInput` 读取键鼠输入，再把看向与移动方向计算委托给当前视角组件。
3. 固定路线漫游模式下，控制器跳过手动移动，调用 `PlayerFixedRouteRoamView.TickRoam()` 按速度自动推进曲线；如果 `沿路线方向旋转` 关闭，控制器仍会把鼠标输入交给漫游组件做自由视角。
4. 固定视角选择栏显示期间，控制器释放鼠标并暂停移动；点击图标后进入 `FixedCamera`，只调用 `PlayerFixedCameraView.HandleLookInput()` 和 `RefreshCamera()`。
5. 小地图传送模式下，控制器释放鼠标并停止移动；点击地图后 `PlayerMinimapTeleportView.TryHandleTeleportClick()` 输出世界坐标，控制器调用 `TeleportTo()` 执行传送。
6. 第一人称详情查看开启时，`PlayerDetailInspectView.LateUpdate()` 从相机中心发射射线，命中带 `WorldDescriptionUI` 且勾选 `仅详情查看时显示` 的物体时调用 `SetDetailInspectHighlighted(true)` 显示说明牌；再次按 `6`、离开第一人称或切换目标时会关闭或清除高亮，其他视角不响应 `6`。
7. 数字键 7 进入 `PerspectivePickup`，它复用第一人称相机和移动；只有该模式下 E 才会从相机中心 Raycast 查找 `PerspectivePickupObject`，其他模式的 E 只保留原有 `InteractableArea` 行为。
8. 拾取时物体记录初始缩放和相机距离、关闭重力、设为运动学并忽略玩家 Collider；`LateUpdate` 在相机刷新后平滑移动物体，再用移动后的实际相机距离立即更新比例缩放；再次 E 或控制器禁用时恢复碰撞和重力，物理帧限制最大下落速度。
9. 离开 `PerspectivePickup` 时自动释放当前物体；每次模式变化或详情查看开关时刷新 `PlayerModeDisplay`。
10. 漫游结束时，如果 `结束后回到第一人称` 为 true，控制器切回 `PlayerViewMode.FirstPerson`。
11. 所有玩家和设置面板按键都通过 `RuntimeInput` 读取，避免项目只启用 Input System 时触发 `UnityEngine.Input` 异常。
12. 玩家每帧在允许普通交互的模式中查找交互半径内最近的 `InteractableArea`，把非空 `PromptText` 交给 `PlayerInteractionPromptDisplay`；无可用目标或交互受限时隐藏。

## 扩展点

- 需要新视角时，新增独立 Mono，并让 `SimplePlayerController` 只调用进入、退出、看向、移动方向或 Tick 接口。
- 需要更复杂漫游时，优先扩展 `PlayerFixedRouteRoamView` 的曲线评估、速度曲线或结束策略，不要改回控制器内部实现。
- 需要更多固定视角按钮样式时，优先扩展 `PlayerFixedCameraView` 的 UI 创建逻辑，不要把 UI 细节放回 `SimplePlayerController`。
- 需要改变地图点击到世界坐标的映射时，优先扩展 `PlayerMinimapTeleportView`；需要改变图片生成方式时，扩展 `MinimapGeneratorWindow`。
- 需要扩展详情查看命中规则或屏幕 UI 时，优先扩展 `PlayerDetailInspectView`；需要改变说明牌内容或布局时，扩展 `WorldDescriptionUI`。
- 需要严格防止透视拾取物体穿模时，把中心射线升级为依据模型包围体的 SphereCast 或多点检测。
- 固定路线编辑器只依赖 UnityEditor `Handles`，没有 Odin 依赖。
