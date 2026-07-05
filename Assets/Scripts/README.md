# 拖拽式控制器与 UI

## 目标

提供以下拖到场景即可使用的基础功能：

- 第一人称、第三人称、固定路线漫游、固定视角、小地图传送和详情查看玩家控制器。
- 可唤出的设置面板，当前只提供回到出生点。
- 可挂到场景物体上的世界空间说明 UI。
- 靠近后按 `E` 触发的可互动区域。
- 按 `F` 开关的手电筒灯光。

## 目录结构

- `Assets/Prefab/First Third Person Controller.prefab`：玩家控制器 prefab。
- `Assets/Prefab/Interactable Area.prefab`：可互动区域 prefab。
- `Assets/Prefab/Settings Panel.prefab`：设置面板控制器 prefab。
- `Assets/Resources/UI/SettingsPanel.prefab`：运行时通过 `Resources.Load("UI/SettingsPanel")` 加载的设置面板 UI prefab，面板尺寸和布局在这里调整。
- `Assets/Prefab/World Description UI.prefab`：世界说明 UI prefab。
- `Assets/Scripts/Player`：玩家移动调度、第一人称视角、第三人称视角、固定路线漫游、固定视角、小地图传送、详情查看和出生点逻辑。
- `Assets/Scripts/AI`：基础巡逻 AI、可编辑路线和循环方式枚举。
- `Assets/Scripts/Interaction`：可互动区域逻辑。
- `Assets/Scripts/UI`：设置面板逻辑。
- `Assets/Scripts/WorldUI`：场景物体说明牌逻辑。
- `Assets/Scripts/Editor`：场景维护测试窗口、场景依赖资源大小排行窗口。
- `Assets/Scripts/Common`：项目缺少 Odin 时使用的 `LabelText` 兼容属性。

## 使用方式

1. 将 `First Third Person Controller.prefab` 拖入场景。`SimplePlayerController` 只负责调度输入和公共移动，第一人称由 `PlayerFirstPersonView` 实现，第三人称由 `PlayerThirdPersonView` 实现，固定路线漫游由 `PlayerFixedRouteRoamView` 实现，固定视角由 `PlayerFixedCameraView` 实现，小地图传送由 `PlayerMinimapTeleportView` 实现，详情查看由 `PlayerDetailInspectView` 实现。默认第三人称模型使用 `Assets/Citizens PRO/People/casual01_m_highpoly.prefab`，移动时会按状态名播放 `idle1`、`walk`、`run` 动画。
2. 将 `Interactable Area.prefab` 拖到可互动对象下，或直接给对象添加 `InteractableArea`。如果对象上已有 `Suburb.SimpleOpenClose`，保持默认 `ObjectClicked` 消息即可。
3. 将 `Settings Panel.prefab` 拖入场景。运行后控制器会从 `Resources/UI/SettingsPanel.prefab` 实例化面板，按 `Esc` 打开或关闭设置面板，点击“回到出生点”会把玩家传回启动位置；如需指定出生点，在 Inspector 中给 `出生点` 赋值。
4. 将 `World Description UI.prefab` 拖到目标物体下，或直接给目标物体添加 `WorldDescriptionUI` 组件，填写 `标题` 和 `说明内容`。
   默认勾选 `仅详情查看时显示`，说明牌只在玩家按 `6` 开启详情查看并用十字标对准时出现。取消勾选后可恢复按 `最大显示距离` 自动显示。如只希望说明牌水平转向相机，勾选 `只绕Y轴旋转`。
5. 创建一个空物体并添加 `SimplePatrolRoute`，再在它下面创建多个子物体作为路线点；运行前可直接在 Scene 视图拖拽这些子物体编辑路线。路线的 `首尾相连` 默认开启，会让辅助线闭合，并允许 `Loop` 模式从末点回到起点；关闭后 `Loop` 到末点会停下。给需要自动行走的角色或 prefab 添加 `SimplePatrolAgent`，把 `巡逻路线` 指向该路线，并按需要调整 `行走速度`、`到点停留时间`、`循环方式`、`待机状态名`、`行走状态名`。
6. 在 Unity 菜单打开 `测试/基础AI巡逻测试窗口`，可快速创建默认路线，把指定 prefab 生成到路线起点，或给当前选中对象批量添加并绑定 `SimplePatrolAgent`。
7. 选中玩家对象上的 `PlayerFixedRouteRoamView` 后，可在 Inspector 调整控制点、漫游速度、首尾相连、是否循环、预览颜色等参数，并在 Scene 视图直接拖拽黄色控制点预览和编辑曲线；运行后按 `3` 进入固定路线漫游。关闭 `沿路线方向旋转` 后，漫游中可用鼠标自由转动视角。
8. 在 `PlayerFixedCameraView` 的 `固定视角点` 中配置多个视角点 Transform 和图标。运行后按 `4` 会在屏幕下方显示一排相机图标，点击图标进入对应固定视角；固定视角下玩家不可移动，只能在 `最大水平旋转` 和 `最大垂直旋转` 范围内小幅转动视角。
9. 在 `PlayerMinimapTeleportView` 上配置 `小地图图片`、`地图世界中心` 和 `地图世界尺寸`。运行后按 `5` 会显示小地图，点击图片位置后会映射到对应世界坐标并传送玩家。
10. 在 Unity 菜单打开 `工具/小地图生成工具`，可在 Scene 视图框选区域并从上往下生成小地图 PNG；选择玩家上的 `PlayerMinimapTeleportView` 后，点击“生成并应用到组件”会自动写入图片和对应世界范围。
11. 在 `PlayerDetailInspectView` 上可调整 `检测距离`、`检测层` 和十字标样式。运行后按 `6` 开关详情查看，屏幕中央出现十字标，对准带 `WorldDescriptionUI` 的物体会显示标题和说明。
12. 在 Unity 菜单打开 `测试/玩家视角与漫游测试窗口`，可给当前选中对象补齐并绑定 `SimplePlayerController`、`PlayerFirstPersonView`、`PlayerThirdPersonView`、`PlayerFixedRouteRoamView`、`PlayerFixedCameraView`、`PlayerMinimapTeleportView`、`PlayerDetailInspectView`。
13. 在 Unity 菜单打开 `测试/场景维护测试窗口`，可一键清理当前打开场景中的丢失脚本、修复 `InteractableArea` 的 `SphereCollider.isTrigger`，也可按“含有这个脚本”批量挂载“自动挂载脚本”。
14. 在 Unity 菜单打开 `工具/场景依赖资源大小排行`，选择当前打开场景或拖入任意 `SceneAsset`，点击“分析依赖资源”后可按磁盘文件大小查看场景依赖资源，并复制当前筛选结果。

## 默认操作

- `WASD`：移动。
- 鼠标：第一人称转向和抬头低头；第三人称只环绕转动镜头，不直接旋转角色根节点。
- `Left Shift`：奔跑。
- `1`：切换到第一人称。
- `2`：切换到第三人称。
- `3`：进入固定路线漫游。
- `4`：显示固定视角相机图标栏。
- `5`：显示小地图传送视图。
- `6`：开关详情查看（屏幕十字标 + 指向物体说明）。
- `E`：靠近可互动区域后互动。
- `R`：让附近可互动区域和未显示的 `WorldDescriptionUI` 场景元素短暂变成提示颜色，然后缓慢恢复。
- `F`：开关手电筒灯光，未指定灯光时会自动创建一个默认关闭的 Spot Light。
- `Esc`：打开或关闭设置面板。

## 验证建议

- 进入 Play Mode 后确认玩家默认第一人称并可移动，按 `1` 回到第一人称，按 `2` 切到第三人称，按 `3` 进入固定路线漫游；第一人称左右移动时镜头应保持稳定，不被隐藏的第三人称模型转向影响；切到第三人称后，鼠标转动只改变镜头方向，不让角色原地跟着旋转，静止播放 `idle1`，移动播放 `walk`，按住 `Left Shift` 移动播放 `run`。
- 选中 `PlayerFixedRouteRoamView` 后拖拽 Scene 视图中的黄色控制点，确认曲线预览同步变化；切换 `首尾相连` 确认关闭时曲线不再闭合；运行后按 `3` 确认玩家按 `漫游速度` 沿曲线路径自动漫游，到达终点后默认回到第一人称；关闭 `沿路线方向旋转` 后确认漫游中鼠标可自由转动视角。
- 给 `PlayerFixedCameraView` 配置至少一个固定视角点和图标，运行后按 `4` 确认底部出现相机图标栏；点击图标后确认相机切到配置点，WASD 不再移动，只能小幅旋转视角；按 `1/2/3` 可离开固定视角。
- 打开 `工具/小地图生成工具` 框选区域并生成 PNG，应用到 `PlayerMinimapTeleportView` 后运行按 `5`，确认小地图覆盖屏幕；点击地图不同位置，玩家应传送到对应世界坐标并贴到地面。
- 给场景物体添加 `WorldDescriptionUI` 并填写说明，运行后按 `6` 开启详情查看，用十字标对准物体确认说明牌出现；再按 `6` 关闭后说明牌应隐藏。
- 把 `InteractableArea` 放到带 `SimpleOpenClose` 的门附近，靠近后按 `E` 确认门可开关。
- 按 `R` 确认指定范围内的可互动区域和未显示的场景说明对象会短暂变成绿色，然后缓慢恢复原色。颜色可在 Inspector 的 `提示颜色` 中调整；`WorldDescriptionUI` 默认提示对应父物体，自身有模型时提示自身，也可用 `提示目标` 指定具体模型。
- 打开 `测试/场景维护测试窗口`，分别验证 Missing Script 清理、`InteractableArea` Trigger 修复和按脚本批量挂载组件。
- 打开 `测试/基础AI巡逻测试窗口` 创建路线，把 `Assets/Citizens PRO/People/casual01_m_highpoly.prefab` 或其他角色 prefab 生成到路线起点，运行后确认角色沿路线移动，到点后按 `到点停留时间` 停留，并按 `循环方式` 继续移动或停止；切换路线的 `首尾相连`，确认 debug 线闭合状态和 `Loop` 回到起点行为一致；确认 NPC 行走时会按 `贴地检测间隔` 定时修正到地面；放置多个 NPC 路线交叉，确认进入 `交流检测半径` 后会停下交流，等待 `交流时长` 后按 `交流后反向离开` 返回相反方向，并且离开 `再次交流分离距离` 前不会反复交流。
- 打开 `工具/场景依赖资源大小排行`，对已保存场景执行分析，确认列表按大小降序排列，筛选、定位、打开和复制结果可用。
- 运行后按 `F` 确认手电筒灯光开关；切到第三人称后确认灯光出现在角色前方，而不是第三人称相机后方。也可在玩家控制器 Inspector 中指定自定义 `手电筒灯光`。
- 按 `Esc` 打开面板，确认 `Resources/UI/SettingsPanel.prefab` 被实例化；点击“回到出生点”后玩家位置复位。调整面板尺寸时，直接修改该 UI prefab 根节点的 RectTransform。
- 给场景物体添加 `WorldDescriptionUI`，确认在详情查看开启且十字标对准时说明牌显示在物体上方并朝向玩家相机。

## 限制

- 当前控制器通过 `RuntimeInput` 兼容新 Input System 和旧版 Input Manager；如果只启用新 Input System，键鼠输入会读取 `Keyboard.current` 和 `Mouse.current`。
- `SimplePatrolAgent` 是基础路线移动逻辑，不依赖 NavMesh，也不会自动绕开障碍；需要避障时应改为 NavMeshAgent 或接入更完整的 AI 系统。
- `SimplePatrolAgent` 默认开启 `定时贴地`，会按 `地面检测层` 向下检测并修正高度；如果路线需要楼梯、跳台或空中路径，可关闭该开关，或调整 `贴地高度偏移` 和检测层。
- `SimplePatrolAgent` 默认开启 `启用相遇交流` 和 `交流后反向离开`，多个启用的巡逻 NPC 会按距离检测，不依赖碰撞体；交流结束后有 `交流冷却时间` 和 `再次交流分离距离`，防止 NPC 尚未分开时马上重复触发。
- 巡逻动画默认尝试播放 `idle1` 和 `walk`，不同 prefab 的 Animator 状态名不一致时，需要在 `SimplePatrolAgent` Inspector 中改状态名，或关闭 `自动播放动画`。
- 第三人称角色外观由 `PlayerThirdPersonView` 的 `第三人称模型预制体` 运行时实例化；未指定时才回退生成胶囊体。动画同步参数已经合并在 `PlayerThirdPersonView` 的动画参数区，组件会直接从生成出的模型实例中查找 `Animator`。
- 世界 UI 使用 uGUI `Text`，未引入 TextMeshPro 依赖。
- 可互动区域默认通过 `SendMessage/BroadcastMessage` 触发 `ObjectClicked`，便于兼容资源包现有脚本。
- 场景依赖排行窗口统计的是项目源文件的磁盘大小，不等同于打包后的压缩体积或运行时内存占用。
