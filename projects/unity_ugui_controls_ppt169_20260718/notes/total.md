# P01_封面

这一单元聚焦 Unity 的 UGUI 系统。目标不只是把控件摆出来，而是让界面在不同屏幕上稳定、能够接收输入，并把玩家操作可靠地转成游戏行为。

---

# P02_UGUI四层结构

Canvas 决定 UI 的空间与布局，EventSystem 负责输入事件，控件保存视觉和交互状态，脚本或 Inspector 事件执行真正的业务行为。理解这四层后，排错时就能快速判断问题属于哪一层。

---

# P03_Canvas两种模式

Overlay 直接覆盖在游戏画面上，最适合菜单和 HUD；World Space 是 3D 世界中的真实平面，适合头顶血条和场景面板。选择 World Space 时，尺寸、朝向、Event Camera 和遮挡都需要额外检查。

---

# P04_RectTransform与适配

RectTransform 用 Anchor 描述相对父矩形的位置关系，用 Pivot 描述旋转、缩放与位置参考点。Canvas Scaler 再根据基准分辨率和 Match 值，让同一套布局适配不同屏幕比例。

---

# P05_EventSystem输入管线

输入先由设备产生，再经过与项目匹配的输入模块交给 EventSystem。Graphic Raycaster 判断指针命中哪个 UI Graphic，最后控件接收点击或导航事件；任意一环缺失都可能导致交互失败。

---

# P06_UI点击排错清单

排错要从全局基础设施开始，再检查 Canvas 和控件，最后检查遮挡与摄像机。按照清单顺序逐项确认，可以避免在复杂层级里反复猜测。

---

# P07_Button状态与事件

Button 至少要让玩家分辨普通、悬停、按下和禁用状态。事件既可以在 Inspector 中连接，也可以在脚本中监听；如果在 OnEnable 添加监听，就应在 OnDisable 移除。

---

# P08_Image四种类型

Simple 适合普通图标，Sliced 适合可缩放的按钮和面板，Tiled 用于重复纹理，Filled 用于血条和冷却。纯装饰 Image 通常可以关闭 Raycast Target，避免透明图片挡住后面的控件。

---

# P09_RawImage纹理管线

RawImage 直接显示 Texture，常与 RenderTexture 和 Camera 组合。Image 更适合 Sprite、九宫格和 Filled，RawImage 更适合摄像机、视频或运行时纹理，不应为了普通图标大量替代 Image。

---

# P10_Toggle与ToggleGroup

Toggle 表示开关或是非状态，多个 Toggle 放进 ToggleGroup 后可以形成单选。恢复保存值时使用 SetIsOnWithoutNotify，可以只更新显示而不误触发业务监听器。

---

# P11_Dropdown工作流

Dropdown 的 Template、Caption 和 Item 之间有固定引用关系，修改层级后要重新检查。显示选项索引不一定等于内部质量等级，实际项目应建立明确映射并成对管理监听生命周期。

---

# P12_设置面板综合实践

设置面板用 Image 作为背景，用 RawImage 显示预览，用 Toggle、Dropdown 和 Button 分别管理状态、选择与提交。每个控件都应有明确的数据来源、当前值和输出行为。

---

# P13_UI交付五项检查

交付前检查适配、交互、状态、事件和性能。一个成熟 UI 不仅能显示，还要在多种输入方式和屏幕比例下保持清楚、稳定、可操作。
