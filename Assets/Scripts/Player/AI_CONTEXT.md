# AI Context: 玩家视角与固定路线漫游

一句话接入：场景里放 `First Third Person Controller.prefab`；各 View 自己读取 `1/2/3/4/5/6/7` 并运行当前模式，`SimplePlayerController` 只协调模式生命周期与传送。

## 关键类

- `SimplePlayerController`：只负责组件初始化、模式进入/退出、控制权限、出生点和传送；Inspector 只保留起始模式与启动光标策略。
- `PlayerLocomotion`：公共移动、奔跑和重力实现，由第一/第三人称 View 调用。
- `PlayerFlashlight`：手电筒输入、参数与视角挂点切换。
- `PlayerInteractionHintInput`：R 键输入与 `InteractableArea` 提示请求。
- `PlayerFirstPersonView`：第一人称相机 rig、鼠标 yaw/pitch、第一人称移动方向。
- `PlayerThirdPersonView`：第三人称相机 rig、模型显示隐藏、第三人称移动方向和动画同步；动画参数与视角/模型参数用 Header 分区，`Animator` 直接从生成出的模型实例中查找。
- `PlayerFixedRouteRoamView`：Catmull-Rom 曲线漫游、世界坐标控制点预览、按曲线采样长度和 `漫游速度` 推进的运行时路径播放；`首尾相连` 控制曲线是否闭合，`循环播放` 只控制到终点后的播放行为。
- `PlayerFixedCameraView`：固定视角选择和播放组件，按 `4` 立即刷新模式提示并显示屏幕底部图标栏，点击图标后把玩家相机切到配置的固定视角点；初始化是幂等的，重复确保组件不会隐藏已打开的面板。面板显示时按住右键拖动小范围观察，左键操作 UI；面板隐藏时锁定鼠标直接观察。
- `PlayerFixedCameraPoint`：固定视角配置项，保存显示名称、视角 Transform 和图标 Sprite。
- `PlayerMinimapTeleportView`：小地图传送组件，按 `5` 显示小地图图片，点击图片后按配置的世界中心和尺寸映射成世界坐标，并通过射线贴地后传送。
- `PlayerDetailInspectView`：详情查看组件，仅在第一人称下响应 `6` 并开关屏幕十字标；离开第一人称会自动关闭。从相机中心射线检测 `检测距离` 内的 `WorldDescriptionUI`，后者通过 `UIManager` 添加/移除 `UIRoot/WorldCanvas/TipUI` 实例。
- `PlayerPerspectivePickupView`：透视拾取模式的输入与生命周期；负责中心射线查找、持有引用、逐帧跟随和退出模式时释放。
- `PerspectivePickupObject`：自身配置“可拾取距离”。中心 Raycast 决定基础距离，随后执行有限次数 `OverlapBoxNonAlloc`。若重叠则按固定步长向相机前移；每次检查必须先按新的候选距离重算 scaleMultiplier、Box 中心偏移和 halfExtents，保证检测尺寸始终与“近小远大”的最终缩放一致。位置平滑完成后，最终缩放必须直接按物体与相机的实际距离计算，不能再对缩放做独立插值，否则视觉尺寸会滞后于相机移动。
- `PlayerModeDisplay`：`UIRoot/Canvas/PlayerModePanel` 上的 `UIPanel`；保存各模式文字并更新已有 TMP_Text，不创建 UI。
- `PlayerInteractionPromptDisplay`：`UIRoot/Canvas/InteractionPromptPanel` 上的 `UIPanel`；由当前触发中的 `InteractableArea` 直接设置内容和显隐。
- `DetailInspectPanel`：`UIRoot/Canvas` 下预制的详情十字标面板；`PlayerDetailInspectView` 只控制显隐和射线检测。
- `FixedCameraPanel`：`UIRoot/Canvas` 下预制的固定视角面板；使用预先配置的按钮槽位，不动态创建按钮。
- TMP 默认字体由 `Assets/TextMesh Pro/Resources/TMP Settings.asset` 直接引用 `Assets/SourceHanSansSC-Medium SDF.asset`，运行时动态创建文本不要再用 `Resources.Load` 加载字体。
- `MinimapGeneratorWindow`：菜单 `工具/小地图生成工具`，支持在 Scene 视图框选区域、从上向下生成小地图 PNG，并把图片与世界范围写回 `PlayerMinimapTeleportView`。
- `PlayerFixedRouteRoamViewEditor`：自定义 Inspector 和 SceneView 控制点拖拽。
- `PlayerViewModeTestWindow`：`测试/玩家视角与漫游测试窗口`，用于给选中对象补齐并绑定组件。
- `RuntimeInput`：键鼠输入兼容层，新 Input System 下读取 `Keyboard.current` / `Mouse.current`，旧输入启用时才调用 `UnityEngine.Input`。

## 数据流

1. 每个 View 的 `Update()` 读取自己的模式按键；命中后请求 `SimplePlayerController.ApplyViewMode()` 完成退出旧模式和进入新模式。
2. 第一/第三人称 View 在自己激活时读取视角输入，并调用 `PlayerLocomotion` 执行移动；第三人称 View 同时刷新模型动画。
3. 固定路线漫游 View 在自己激活时调用内部 `Tick()` 推进曲线，完成后请求控制器切回第一人称。
4. 固定视角选择栏显示期间，控制器释放鼠标并暂停移动；点击图标后进入 `FixedCamera`，只调用 `PlayerFixedCameraView.HandleLookInput()` 和 `RefreshCamera()`。
5. 小地图传送模式下，控制器释放鼠标并停止移动；点击地图后 `PlayerMinimapTeleportView.TryHandleTeleportClick()` 输出世界坐标，控制器调用 `TeleportTo()` 执行传送。
6. 第一人称详情查看开启时，`PlayerDetailInspectView.LateUpdate()` 从相机中心发射射线，命中带 `WorldDescriptionUI` 且勾选 `仅详情查看时显示` 的物体时调用 `SetDetailInspectHighlighted(true)` 显示说明牌；再次按 `6`、离开第一人称或切换目标时会关闭或清除高亮，其他视角不响应 `6`。
7. `PlayerPerspectivePickupView` 自己读取数字键 7 和 E；进入后复用第一人称 View 与 `PlayerLocomotion`，退出时自行释放物体。
8. 拾取时物体记录初始缩放和相机距离、关闭重力、设为运动学并忽略玩家 Collider；`LateUpdate` 在相机刷新后平滑移动物体，再用移动后的实际相机距离立即更新比例缩放；再次 E 或控制器禁用时恢复碰撞和重力，物理帧限制最大下落速度。
9. 离开 `PerspectivePickup` 时自动释放当前物体；每次模式变化或详情查看开关时刷新 `PlayerModeDisplay`。
10. 漫游结束时，如果 `结束后回到第一人称` 为 true，控制器切回 `PlayerViewMode.FirstPerson`。
11. 所有玩家和设置面板按键都通过 `RuntimeInput` 读取，避免项目只启用 Input System 时触发 `UnityEngine.Input` 异常。
12. `InteractableArea.OnTriggerEnter/Exit` 维护范围内玩家；进入时取得提示焦点并打开 `InteractionPromptPanel`，停留期间由交互区自身读取 E 并调用交互方法，退出时隐藏或把焦点交给仍覆盖玩家的其他区域。玩家的多个 Collider 使用重叠计数，避免单个 Collider 退出就提前关闭。
13. `PlayerInteractionHintInput` 读取 R 并请求 `InteractableArea` 显示提示；Trigger 范围内的 `WorldDescriptionUI` 直接读取同一玩家能力组件暴露的按键，不做全场景查找。

## 扩展点

- 需要新视角时，新增独立 Mono，让它自行读取激活输入和执行 Tick，只向 `SimplePlayerController` 请求模式生命周期切换。
- 需要更复杂漫游时，优先扩展 `PlayerFixedRouteRoamView` 的曲线评估、速度曲线或结束策略，不要改回控制器内部实现。
- 需要更多固定视角按钮样式时，优先扩展 `PlayerFixedCameraView` 的 UI 创建逻辑，不要把 UI 细节放回 `SimplePlayerController`。
- 需要改变地图点击到世界坐标的映射时，优先扩展 `PlayerMinimapTeleportView`；需要改变图片生成方式时，扩展 `MinimapGeneratorWindow`。
- 需要扩展详情查看命中规则或屏幕 UI 时，优先扩展 `PlayerDetailInspectView`；需要改变说明牌内容或布局时，扩展 `WorldDescriptionUI`。
- 需要严格防止透视拾取物体穿模时，把中心射线升级为依据模型包围体的 SphereCast 或多点检测。
- 调整普通交互范围时修改 `InteractableArea` 的 `SphereCollider`；调整 R 键提示范围或时长时修改对应交互物/世界说明组件，不要把参数加回玩家控制器。
- 固定路线编辑器只依赖 UnityEditor `Handles`，没有 Odin 依赖。
- 玩家 UI 和 EventSystem 必须预置；不要在玩家运行时代码中恢复 Canvas、TMP、Image、Button、十字标或 EventSystem 创建逻辑。
