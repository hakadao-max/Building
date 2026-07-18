# AI Context

## 一句话接入

拖入 `Assets/Prefab/First Third Person Controller.prefab`、`Assets/Prefab/Settings Panel.prefab`，把 `Interactable Area.prefab` 放到可互动对象附近；玩家默认第一人称，按 `1/2/3/4/5` 分别进入第一人称、第三人称、固定路线漫游、显示固定视角图标栏或显示小地图传送，按 `6` 开关详情查看；需要场景角色自动走路时，创建 `SimplePatrolRoute` 路线并给角色 prefab 添加 `SimplePatrolAgent`。

## 关键类

- `SimplePlayerController`：基于 `CharacterController` 的玩家调度器，负责 `1/2/3/4/5/6` 视角与详情查看输入、公共移动、记录出生点、传送回出生点、按 `E` 互动、按 `R` 显示附近颜色提示、按 `F` 开关手电筒；第一/第三人称、固定路线漫游、固定视角、小地图传送和详情查看的具体相机/UI逻辑不放在这里。
- `PlayerFirstPersonView`：第一人称相机 rig、鼠标 yaw/pitch 和第一人称移动方向。
- `PlayerThirdPersonView`：第三人称相机 rig、可视模型显示隐藏、第三人称移动方向和动画同步；动画参数与视角参数用 Header 分区，`Animator` 直接从生成出的第三人称模型实例中查找。
- `PlayerFixedRouteRoamView`：固定路线漫游 Mono，保存 Catmull-Rom 曲线控制点、预览参数、漫游速度和结束策略，运行时直接推动玩家沿曲线移动。
- `PlayerFixedCameraView`：固定视角选择和播放组件，运行时创建屏幕底部图标栏，点击图标后把玩家相机切到配置的固定视角点，玩家不可移动，只可小范围旋转。
- `PlayerFixedCameraPoint`：固定视角配置项，保存显示名称、视角 Transform 和图标 Sprite。
- `PlayerMinimapTeleportView`：小地图传送组件，运行时显示小地图图片，点击图片位置会映射到世界 XZ 坐标，按地面射线修正高度后交给控制器传送。
- `PlayerDetailInspectView`：详情查看组件，运行时显示屏幕十字标，从相机中心射线检测范围内物体，并驱动 `WorldDescriptionUI` 仅在开启时显示说明。
- `MinimapGeneratorWindow`：菜单 `工具/小地图生成工具`，支持 Scene 视图框选区域、生成顶部正交小地图 PNG，并可直接写回 `PlayerMinimapTeleportView`。
- `PlayerFixedRouteRoamViewEditor`：固定路线漫游的自定义 Inspector 和 SceneView 控制点拖拽编辑器。
- `PlayerViewMode`：玩家视角枚举，包含 `FirstPerson`、`ThirdPerson`、`FixedRouteRoam`、`FixedCamera`、`MinimapTeleport`。
- `SimplePatrolRoute`：基础 AI 路线组件，默认使用自身子物体作为路线点，并在 Scene 视图绘制加粗路线点和连线；`首尾相连` 默认开启，控制辅助线是否闭合，也控制 `Loop` 模式是否允许末点回到起点。
- `SimplePatrolAgent`：可挂到任意角色或 prefab 上的基础巡逻组件，按路线移动、到点停留、循环/往返/单次移动，速度、停留时间、转向、重力、定时贴地、相遇交流和动画状态名均可在 Inspector 编辑。
- `SimplePatrolLoopMode`：巡逻循环方式枚举，包含 `Loop`、`PingPong`、`Once`。
- `InteractableArea`：可互动区域，默认用 SphereCollider 触发器标记范围，并向目标广播 `ObjectClicked` 以兼容 `Suburb.SimpleOpenClose`；`ShowHint` 默认让自身对象短暂变成 `提示颜色`，可通过 `提示目标` 指向具体模型。
- `SettingsPanelController`：从 `Resources.Load("UI/SettingsPanel")` 实例化设置面板 prefab，默认用 `Esc` 唤出，按钮调用玩家回出生点。
- `WorldDescriptionUI`：运行时创建或绑定世界空间 Canvas，显示标题和说明，并在 `LateUpdate` 中朝向 `Camera.main`；默认勾选 `仅详情查看时显示`，只有 `PlayerDetailInspectView` 开启且十字标命中时才显示；取消勾选后恢复按 `最大显示距离` 自动显示；提示默认作用到对应父物体，自身有模型时作用自身。
- `TemporaryColorHint`：运行时临时变色组件，会缓存目标自身 `MeshRenderer` 材质颜色和自身 uGUI `Graphic` 颜色，保持提示色一段时间后缓慢恢复；重复调用会重置提示时间。
- `SimplePatrolTestWindow`：Editor 测试窗口，菜单入口 `测试/基础AI巡逻测试窗口`，支持创建默认巡逻路线、生成 prefab 到路线起点、给选中场景对象批量添加并绑定 `SimplePatrolAgent`。
- `PlayerViewModeTestWindow`：Editor 测试窗口，菜单入口 `测试/玩家视角与漫游测试窗口`，支持给选中对象补齐并绑定玩家视角组件。
- `SceneMaintenanceTestWindow`：Editor 测试窗口，菜单入口 `测试/场景维护测试窗口`，支持清理当前打开场景 Missing Script、修复 `InteractableArea` 的 `SphereCollider.isTrigger`、按脚本批量挂载脚本。
- `SceneDependencyReportWindow`：Editor 工具窗口，菜单入口 `工具/场景依赖资源大小排行`，支持分析当前打开场景或指定 `SceneAsset` 的依赖资源，并按项目源文件磁盘大小降序展示、筛选、定位、打开和复制。
- `LabelTextAttribute` / `LabelTextDrawer`：在没有 Odin 的项目中提供中文 Inspector 标签兼容。

## 数据流

1. `SimplePlayerController.Awake` 补齐并初始化 `PlayerFirstPersonView`、`PlayerThirdPersonView`、`PlayerFixedRouteRoamView`、`PlayerFixedCameraView`、`PlayerMinimapTeleportView`、`PlayerDetailInspectView`，创建默认关闭的手电筒 Spot Light，并记录出生点。
2. `SimplePatrolRoute` 默认把子物体顺序作为路线点；编辑路线时直接移动这些子物体即可，运行时 `SimplePatrolAgent` 通过路线索引读取世界坐标；`首尾相连` 关闭时，路线 debug 线不闭合。
3. `SimplePatrolAgent.StartPatrol` 读取起点索引，可选把角色移动到起点；`Update` 中先处理停留计时，再向目标路线点移动，到点后按 `SimplePatrolLoopMode` 选择下一个点；`Loop` 模式会尊重路线的 `首尾相连` 开关。
4. `SimplePatrolAgent` 优先使用同物体上的 `CharacterController.Move`，没有 `CharacterController` 时直接移动 `Transform`；默认不依赖 NavMesh。
5. `SimplePatrolAgent` 默认开启 `定时贴地`，按 `贴地检测间隔` 从角色上方向下射线检测 `地面检测层`，忽略自身子物体碰撞体后把角色 Y 修正到命中的地面点加 `贴地高度偏移`。
6. `SimplePatrolAgent` 启用时注册到静态 `ActiveAgents` 列表；默认开启 `启用相遇交流` 和 `交流后反向离开`，每帧按 `交流检测半径` 和 `交流最大高度差` 查找其他可交流 NPC，进入交流后双方暂停路线、播放 idle、可选互相看向，`交流时长` 结束后反转 `moveDirectionSign` 并重设目标点：移动中会先返回上一个路点，停点中会选择反方向的下一个路点；`交流冷却时间` 和 `再次交流分离距离` 会防止双方未分开时重复交流。
7. `SimplePatrolAgent` 默认从子节点查找 `Animator`，移动和停留时分别尝试播放 `walk` 与 `idle1`，不同 prefab 可在 Inspector 修改状态名或关闭自动动画。
8. `InteractableArea` 通过 Trigger 进入/退出事件保存范围状态，只在状态有效时于 `Update` 轮询 E 并调用 `Interact`；不维护全局区域集合。
9. 玩家按 `R` 时，`PlayerInteractionHintInput` 调用 `PlayerRangeColorScanner`，以玩家为球心按配置的半径和 LayerMask 执行物理扫描，并让命中的交互物临时变色。
10. `InteractableArea.Interact` 向目标广播 `ObjectClicked`，目标可以是自身、父物体或 Inspector 指定对象。
11. `SettingsPanelController` 运行时加载 `Assets/Resources/UI/SettingsPanel.prefab`，绑定其中名为 `Return Spawn Button` 的按钮，并在场景缺少 uGUI `EventSystem` 时自动创建；打开面板时锁住所有 `SimplePlayerController` 输入并释放鼠标；点击按钮后调用 `TeleportTo` 或 `ReturnToSpawn`。
12. `WorldDescriptionUI.Awake` 自动创建世界空间 Canvas；`LateUpdate` 更新位置、距离显示和朝向。
13. `SimplePatrolTestWindow` 使用 Undo 创建场景对象或添加组件，并通过 `SetRoute` 绑定路线；只用于编辑器测试，不进入运行时构建。
14. `SceneMaintenanceTestWindow` 只遍历当前已加载场景，使用 Undo/场景脏标记支持可撤销编辑；Trigger 修复只处理与 `InteractableArea` 同对象的 `SphereCollider`。
15. `SceneDependencyReportWindow` 通过 `AssetDatabase.GetDependencies(scenePath, true)` 获取场景递归依赖，再用依赖资源源文件的磁盘大小排序；默认不计入脚本和程序集文件，可在窗口中勾选包含。
16. `SimplePlayerController.HandleLookInput` 按当前模式把鼠标输入委托给 `PlayerFirstPersonView` 或 `PlayerThirdPersonView`；相机位置由当前视角组件在 `LateUpdate` 刷新。
17. `SimplePlayerController.GetMoveDirection` 按当前模式委托给第一/第三人称组件；第三人称直接使用 `yaw` 计算移动方向，避免依赖同一帧内尚未更新完成的相机 Transform。
18. `SimplePlayerController.HandleMoveInput` 完成 `CharacterController.Move` 后读取实际水平速度，只在第三人称下调用 `PlayerThirdPersonView.SetMovementState`；第一人称会让第三人称模型保持 idle，避免隐藏模型转向影响镜头稳定。默认玩家 prefab 的 `PlayerThirdPersonView.visualPrefab` 指向 `Assets/Citizens PRO/People/casual01_m_highpoly.prefab`。
19. `SimplePlayerController` 进入 `FixedRouteRoam` 后跳过手动移动和交互输入，每帧调用 `PlayerFixedRouteRoamView.TickRoam` 按 `漫游速度` 推进曲线；`首尾相连` 只控制曲线最后一点是否连回第一点，`循环播放` 只控制到终点后是否重新开始；`沿路线方向旋转` 关闭时仍会处理鼠标自由视角；漫游结束后默认切回第一人称。
20. 玩家按 `4` 时，`SimplePlayerController` 调用 `PlayerFixedCameraView.ToggleSelectionPanel` 并释放鼠标；点击固定视角图标后进入 `FixedCamera`，控制器停止移动和交互输入，只允许 `PlayerFixedCameraView` 在配置的最大角度内旋转相机；按 `1/2/3` 可离开固定视角。
21. 玩家按 `5` 时进入 `MinimapTeleport` 并释放鼠标；`PlayerMinimapTeleportView` 全屏显示小地图，点击图片会按 `地图世界中心` 和 `地图世界尺寸` 计算世界坐标，再通过地面射线得到传送点，控制器调用 `TeleportTo()`；进入小地图时会自动关闭详情查看。
22. 玩家按 `6` 时切换 `PlayerDetailInspectView`；开启后屏幕中央显示十字标，`LateUpdate` 从相机中心射线检测 `检测距离` 内带 `WorldDescriptionUI` 的碰撞体，命中后显示该物体的说明牌。

## 依赖

- Unity 2022.3。
- `UnityEngine.UI` / uGUI。
- `RuntimeInput` 兼容新 Input System 与旧版 Input Manager；只启用新 Input System 时，WASD/方向键、数字键、鼠标 delta 会从 `Keyboard.current` / `Mouse.current` 读取。
- Citizens PRO 男性 Animator Controller：默认状态名 `idle1`、`walk`、`run`。
- `SimplePatrolAgent` 不依赖 NavMesh；需要避障时应替换或扩展为 NavMeshAgent 方案。
- `SimplePatrolTestWindow`、`PlayerViewModeTestWindow`、`PlayerFixedRouteRoamViewEditor`、`MinimapGeneratorWindow` 和 `SceneDependencyReportWindow` 仅依赖 UnityEditor API，必须放在 `Assets/Scripts/Editor` 下。

## 扩展点

- 替换 `PlayerThirdPersonView` 的 `第三人称可视模型` 或 `第三人称模型预制体` 可使用正式角色模型；若 Animator 状态名不同，需要同步修改 `PlayerThirdPersonView` 动画参数区的状态名字段。
- 扩展玩家视角时新增独立 Mono，并让 `SimplePlayerController` 只负责调度，不要把具体相机、地图或 UI 逻辑重新塞回控制器。
- 给不同 prefab 添加 `SimplePatrolAgent` 并绑定同一个或不同的 `SimplePatrolRoute`，即可复用基础巡逻逻辑；不同角色的速度、停留时间、循环方式和动画状态名分别在各自 Inspector 中调整。
- 如路线需要更复杂编辑，可扩展 `SimplePatrolTestWindow` 或为 `SimplePatrolRoute` 增加自定义 Inspector/SceneView 手柄。
- 给 `SimplePlayerController` 指定 `手电筒灯光` 可替换默认运行时创建的 Spot Light；第一人称和固定路线漫游默认挂到当前相机，第三人称会挂到玩家根节点并使用 `第三人称手电筒前方偏移` / `第三人称手电筒前方角度`。
- `InteractableArea` 的 `交互方法名` 可改成其他 `SendMessage` 入口，`交互目标` 可指定到真正处理互动的对象。
- 给 `SettingsPanelController` 指定 `出生点` 可覆盖玩家启动位置；调整面板尺寸或文案时修改 `Assets/Resources/UI/SettingsPanel.prefab`，不要在控制器里改运行时布局。
- `WorldDescriptionUI.SetDescription` 可在运行时动态改标题和说明。
