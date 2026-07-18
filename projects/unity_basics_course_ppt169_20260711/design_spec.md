# Unity 基础脚本与 UGUI - Design Spec

## I. Project Information

| Item | Value |
| ---- | ----- |
| **Project Name** | Unity 基础脚本与 UGUI |
| **Canvas Format** | PPT 16:9 (1280×720) |
| **Page Count** | 18 |
| **Design Style** | instructional + soft-rounded |
| **Target Audience** | Unity 初学者、从 Godot 转向 Unity 的学习者、基础课程教师与培训者 |
| **Use Case** | 课堂投影、入门培训、配合 Pong 实作的课程讲解 |
| **Delivery Purpose** | presentation |
| **Content Strategy** | balanced default；重组两份讲义为“概念→操作→代码→练习”课程线，事实与示例均来自源材料 |
| **Created Date** | 2026-07-11 |

## II. Canvas Specification

| Property | Value |
| -------- | ----- |
| **Format** | PPT 16:9 |
| **Dimensions** | 1280×720 |
| **viewBox** | `0 0 1280 720` |
| **Margins** | 左右 56，上 44，下 36 |
| **Content Area** | 1168×620 |

## III. Visual Theme

### Theme Style

- **Mode**: instructional
- **Visual style**: soft-rounded
- **Theme**: Light theme
- **Tone**: 现代、友好、技术清晰、课堂互动

### Color Scheme

| Role | HEX | Purpose |
| ---- | --- | ------- |
| Background | `#F7F9FC` | 页面背景 |
| Secondary bg | `#EAF0FF` | 卡片与代码区域 |
| Primary | `#3157D5` | 标题、关键结构 |
| Accent | `#7C4DFF` | 重点、事件与练习 |
| Secondary accent | `#22A6B3` | UI、流程与辅助连接 |
| Body text | `#172033` | 正文 |
| Secondary text | `#56627A` | 注释 |
| Tertiary text | `#8490A8` | 页码与来源 |
| Border/divider | `#C9D4F2` | 边框与分隔 |
| Success | `#2E9B6F` | 正确与完成 |
| Warning | `#E45858` | 注意与错误 |
| Surface | `#FFFFFF` | 浮层卡片 |
| Grid | `#DCE4F7` | 轻量网格 |

### Native SVG Illustration Direction

- 封面与章节过渡使用原生可编辑 SVG：圆润几何、轻量网格、代码括号、Pong 轨迹和 UGUI 控件轮廓。
- 所有文字、组件、流程与示意图均保持为 PowerPoint 可编辑对象。

## IV. Typography System

### Font Plan

**Typography direction**: 现代友好无衬线，代码使用等宽字体。

| Role | Chinese | English | Fallback tail |
| ---- | ------- | ------- | ------------- |
| Title | Microsoft YaHei | Segoe UI | sans-serif |
| Body | Microsoft YaHei | Segoe UI | sans-serif |
| Emphasis | Microsoft YaHei | Segoe UI | sans-serif |
| Code | — | Consolas, Courier New | monospace |

- Title: `'Segoe UI','Microsoft YaHei',sans-serif`
- Body: `'Segoe UI','Microsoft YaHei',sans-serif`
- Emphasis: `'Segoe UI','Microsoft YaHei',sans-serif`
- Code: `Consolas,'Courier New',monospace`

### Font Size Hierarchy

- Cover title: 88
- Page title: 56
- Subtitle: 42
- Lead: 36
- Subheading: 36
- Body: 32
- Annotation: 24
- Footnote: 18
- Code: 24

## V. Layout Principles

- Header 92：章节标签 + 页面标题；内容页标题左对齐。
- Content 540：根据教学逻辑使用单焦点、非对称分栏、流程、代码/解释并置。
- Footer 32：章节名与自动页码。
- 安全边距 56；内容块间距 28；卡片圆角 16；卡片内边距 24；图标文字间距 12。
- `anchor` 页用大标题与单一视觉；`dense` 页允许多块信息；`breathing` 页保持大留白与单一结论。

## VI. Icon Usage Specification

- **Library**: `tabler-filled`
- **Usage**: `<use data-icon="tabler-filled/icon-name" .../>`

| Purpose | Icon Path | Page |
| ------- | --------- | ---- |
| Unity 对象/组件 | `tabler-filled/assembly` | P04 |
| 脚本与知识 | `tabler-filled/article` | P06-P10 |
| 播放与生命周期 | `tabler-filled/player-play` | P05 |
| 游戏输入 | `tabler-filled/device-gamepad` | P07 |
| 调试 | `tabler-filled/bug` | P11 |
| UI 层级 | `tabler-filled/apps` | P13 |
| UI 窗口 | `tabler-filled/app-window` | P12-P16 |
| 布局 | `tabler-filled/layout` | P13/P16 |
| 设置 | `tabler-filled/settings` | P15 |
| 参数调节 | `tabler-filled/adjustments` | P15 |
| 鼠标交互 | `tabler-filled/mouse` | P14 |
| 点击事件 | `tabler-filled/click` | P14 |

## VII. Visualization Reference List

Catalog read: 76 templates

| Page | Template | Path | Summary-quote (verbatim) | Usage |
| ---- | -------- | ---- | ------------------------ | ----- |
| P02 | agenda_list | `templates/charts/agenda_list.svg` | "Pick for table of contents, meeting agendas, or presentation roadmap — numbered items + brief description + duration / owner per row. Skip for substantive content lists (use vertical_list) or single-page section dividers (use a cover layout)." | 两大单元课程路线 |
| P04 | hub_spoke | `templates/charts/hub_spoke.svg` | "Pick for 1 core capability + 4-8 surrounding capabilities (platform/ecosystem); each spoke = title or title + 1-2 line description. Skip if center is a system containing parts with their own descriptions (use module_composition), or surroundings exert inward pressure on the center (use hub_inward_arrows)." | GameObject 与组件关系 |
| P05 | process_flow | `templates/charts/process_flow.svg` | "Pick for 3-8 sequential steps connected by simple arrows — approval workflows, customer onboarding, request handling, lifecycle stages. Skip if cyclical (use circular_stages) or stages produce named outputs (use pipeline_with_stages)." | Awake→Start→Update/FixedUpdate |
| P10 | process_flow | `templates/charts/process_flow.svg` | "Pick for 3-8 sequential steps connected by simple arrows — approval workflows, customer onboarding, request handling, lifecycle stages. Skip if cyclical (use circular_stages) or stages produce named outputs (use pipeline_with_stages)." | 碰撞到事件再到计分 UI |
| P13 | layered_architecture | `templates/charts/layered_architecture.svg` | "Pick for 3-4 horizontal architecture layers (presentation/service/data), 2-4 module cards per layer, each card = title + 1-line description (description required, even if source brief). Skip if no per-module descriptions (use icon_grid) or no horizontal layering (use module_composition)." | Canvas/Panel/Control 层级 |
| P17 | comparison_table | `templates/charts/comparison_table.svg` | "Pick for 2-4 plans/products compared across many feature rows (dense matrix). Skip for pricing-tier marketing layout (use comparison_columns)." | Godot 到 Unity 概念对照 |

**Runners-up considered**

- `numbered_steps` | rejected for P05：生命周期存在分叉（Update 与 FixedUpdate），箭头流程更准确。
- `module_composition` | rejected for P13：UGUI 结构强调层级而非单一父容器的同级模块。
- `basic_table` | rejected for P17：对照表需要突出两套框架的逐项映射，而非一般数据表。

## VIII. Image Resource List

无外部图片。封面、章节页、编辑器结构、组件关系和界面层级全部由原生可编辑 SVG 构建。

## IX. Content Outline

### Part 1: 课程入口

#### Slide 01 - Cover
- **Cover impact**: 用“脚本驱动 Pong、UGUI 连接玩家”的手绘场景作为视觉钩子；全画幅插画 + 左侧浮动标题。
- **Layout**: 全画幅背景，左侧标题栈，右侧 Pong 与 UI 主体。
- **Title**: Unity 基础脚本与 UGUI
- **Subtitle**: 从 C# 组件逻辑到可交互游戏界面

#### Slide 02 - 学习路线
- **Layout**: 两段课程路线 + 终点项目。
- **Title**: 一条 Pong 主线，串起脚本与界面
- **Core message**: 先让对象动起来，再让玩家看见并控制它。
- **Visualization**: agenda_list
- **Content**: 01 脚本基础（对象、生命周期、输入、碰撞、事件）｜02 UGUI（Canvas、控件、事件、布局、主题）｜成果：可运行、可调参数、可返回菜单的 Pong 原型。

### Part 2: Unity 脚本基础

#### Slide 03 - 脚本单元开场
- **Layout**: 插画场 + 左侧大标题。
- **Title**: Part 1｜让 GameObject 拥有行为
- **Core message**: Unity 的行为来自挂载在对象上的组件与脚本。

#### Slide 04 - GameObject 与 Component
- **Layout**: 中心对象 + 六个组件辐射。
- **Title**: GameObject 是容器，Component 才是能力
- **Core message**: Transform、渲染、物理、碰撞和脚本共同组成一个可工作的游戏对象。
- **Visualization**: hub_spoke
- **Content**: 中心 Ball；周围 Transform、SpriteRenderer、Rigidbody2D、Collider2D、BallController、Prefab/Scene。

#### Slide 05 - 生命周期
- **Layout**: 横向流程，Update 与 FixedUpdate 分支。
- **Title**: 把初始化、输入与物理放在正确的时机
- **Core message**: 生命周期函数决定代码何时执行，也决定移动是否稳定。
- **Visualization**: process_flow
- **Content**: Awake 缓存引用 → Start 初始化 → Update 每帧读取输入；FixedUpdate 固定间隔处理 Rigidbody。

#### Slide 06 - C# 脚本骨架
- **Layout**: 左侧代码，右侧逐行解剖。
- **Title**: 一个 MonoBehaviour，就是一个可挂载组件
- **Core message**: 文件名、类名、继承关系和方法结构是 Unity 脚本的最小骨架。
- **Content**: `using UnityEngine; public class BallController : MonoBehaviour { void Start(){ Debug.Log("Ball ready"); } }`；标注 using、class、继承、方法、日志。

#### Slide 07 - 变量、运算符与输入
- **Layout**: 三个纵向模块 + 底部练习条。
- **Title**: 变量保存状态，输入把玩家意图送进脚本
- **Core message**: 强类型变量配合 Input API，构成最基础的交互循环。
- **Content**: `int/float/string/bool`；`[SerializeField] private float speed`；`GetKey/GetKeyDown/GetAxisRaw`；练习：W/S 移动 Paddle。

#### Slide 08 - 移动 Paddle
- **Layout**: 代码编辑器主区域 + 右侧三步解释。
- **Title**: 输入 × 速度 × deltaTime = 稳定移动
- **Core message**: 读取轴输入、更新时间独立位移、再用 Clamp 限制范围。
- **Content**: PaddleController 核心代码；①读取 Vertical ②计算位移 ③Clamp(-limitY, limitY)。

#### Slide 09 - 函数与集合
- **Layout**: 上方函数输入/输出图，下方数组/List/Dictionary 对比。
- **Title**: 用函数封装动作，用集合管理多个对象
- **Core message**: 函数降低重复，集合把同类数据组织成可遍历结构。
- **Content**: `GetMoveAmount(input,speed)`；数组固定长度、List 可变、Dictionary 键值访问；for/foreach。

#### Slide 10 - 物理、碰撞与事件
- **Layout**: 从 Ball 到 Score UI 的事件链。
- **Title**: 碰撞负责“发生了什么”，事件负责“通知谁”
- **Core message**: Rigidbody2D/Collider2D 产生物理结果，C# event 或 UnityEvent 解耦计分与 UI。
- **Visualization**: process_flow
- **Content**: Ball 运动 → Collider2D 触发 → ScoreZone.OnTriggerEnter2D → Scored.Invoke → UI 更新；普通碰撞与 Trigger 区分。

#### Slide 11 - 调试与脚本练习
- **Layout**: 左侧调试清单，右侧 Pong 完成度阶梯。
- **Title**: 先读第一条错误，再逐帧验证假设
- **Core message**: Console、断点、Pause/Step 和 Gizmos 让问题从“猜”变成“证据”。
- **Content**: Debug.Log/Warning/Error；双击错误定位；Pause/Step；OnDrawGizmos；练习：随机发球、逐渐增速、击球角度、重启与计分。

### Part 3: Unity UGUI

#### Slide 12 - UGUI 单元开场
- **Layout**: 插画场 + 左侧大标题。
- **Title**: Part 2｜把游戏状态变成可见、可点、可调
- **Core message**: UGUI 把脚本状态转化为玩家能理解和操作的界面。

#### Slide 13 - Canvas 与层级
- **Layout**: 三层架构 + 右侧设置提示。
- **Title**: Canvas 是 UI 根，Panel 负责分组，Control 负责交互
- **Core message**: 清晰层级与 Canvas Scaler 是可维护、可适配界面的起点。
- **Visualization**: layered_architecture
- **Content**: Canvas + Graphic Raycaster；MenuPanel/SettingsPanel；Title、Button、Slider、Toggle；EventSystem；1920×1080、Scale With Screen Size、Match 0.5。

#### Slide 14 - Button 与 UIManager
- **Layout**: 左侧层级，右侧事件绑定代码与状态切换。
- **Title**: UIManager 集中管理按钮和面板状态
- **Core message**: Inspector 引用配合 onClick，让 Play、Settings、Quit 的行为清晰可追踪。
- **Content**: `[SerializeField]` 引用；`onClick.AddListener`；`SetActive(true/false)`；Application.Quit 在构建版本生效。

#### Slide 15 - Slider、Toggle 与数据绑定
- **Layout**: 左 Slider、右 Toggle，中间箭头连接游戏对象。
- **Title**: 控件的值，应该直接驱动可观察的游戏参数
- **Core message**: onValueChanged 将界面输入绑定到速度、全屏等状态。
- **Content**: Slider Min/Max/Whole Numbers；SetBallSpeed(value)；文本显示 `0.0`；Toggle→Screen.fullScreen；设置页默认隐藏。

#### Slide 16 - 布局、锚点与主题
- **Layout**: 一个界面从“散乱”到“系统化”的前后对比。
- **Title**: 用布局组件替代手工对齐，用状态系统替代重复调色
- **Core message**: Layout Group、Anchor、九宫格与 Prefab 让 UI 同时适配屏幕和保持一致。
- **Content**: Vertical/Horizontal/Grid Layout Group；Content Size Fitter/Layout Element；Anchor；Sliced Image；Button Normal/Highlighted/Pressed/Disabled；TMP；关闭无用 Raycast Target。

#### Slide 17 - Godot → Unity 对照
- **Layout**: 双列对照表，脚本与 UI 分组。
- **Title**: 名称不同，核心问题高度相似
- **Core message**: 从 Godot 迁移时，先建立概念映射，再学习 Unity 的组件化表达。
- **Visualization**: comparison_table
- **Content**: Node→GameObject+Component；Scene→Scene/Prefab；`_ready`→Start；`_process`→Update；`_physics_process`→FixedUpdate；Signal→event/UnityEvent；Control→RectTransform+Graphic；Container→Layout Group；pressed/value_changed/toggled→onClick/onValueChanged。

#### Slide 18 - 实作检查点
- **Closing impact**: 用一张可勾选的“Pong 可交付定义”收束课程；左侧完成清单、右侧成品结构剪影。
- **Layout**: 大号完成标记 + 六步检查清单。
- **Title**: 现在，把脚本与 UGUI 连接成一个可玩的原型
- **Core message**: 一个完整练习必须同时具备运动、碰撞、事件、菜单、参数绑定和一致视觉。
- **Content**: ①Ball/Paddle 可运动 ②边界与计分有效 ③事件更新 UI ④开始/设置/返回菜单 ⑤Slider/Toggle 生效 ⑥按钮状态与布局统一。

## X. Speaker Notes Requirements

- 总时长约 45–60 分钟。
- 每页注释包含：讲解目标、关键解释、演示动作、课堂提问或练习、过渡句。
- 语气：对初学者友好、清晰、可操作；代码页先讲意图再讲语法。
- `notes/total.md` 使用每页一个 `#` 标题；拆分后文件不含标题行。
