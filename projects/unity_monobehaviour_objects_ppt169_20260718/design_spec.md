# Unity 语法基础：MonoBehaviour 与常用对象 - Design Spec

> Human-readable design narrative. The execution contract is `spec_lock.md`.

## I. Project Information

| Item | Value |
| ---- | ----- |
| **Project Name** | unity_monobehaviour_objects_ppt169_20260718 |
| **Canvas Format** | PPT 16:9 (1280×720) |
| **Page Count** | 13 |
| **Design Style** | Instructional + sketch-notes |
| **Target Audience** | Unity 初学者、职业教育或高校游戏开发入门课程学员 |
| **Use Case** | 课堂讲授、投影演示、课后复习 |
| **Delivery Purpose** | presentation |
| **Content Strategy** | balanced default：把长文重组为“脚本身份→生命周期→对象操作→角色移动→场景闭环”的教学路径，代码仅保留决策性片段 |
| **Created Date** | 2026-07-18 |

---

## II. Canvas Specification

| Property | Value |
| -------- | ----- |
| **Format** | PPT 16:9 |
| **Dimensions** | 1280×720 |
| **viewBox** | `0 0 1280 720` |
| **Margins** | 左右 58，上 46，下 48 |
| **Content Area** | 1164×598；标题带 92，正文区 480，页脚 42 |

---

## III. Visual Theme

### Theme Style

- **Mode**: instructional
- **Visual style**: sketch-notes
- **Theme**: Light theme
- **Tone**: 温暖、可操作、强调函数选择与对象关系

### Color Scheme

| Role | HEX | Purpose |
| ---- | --- | ------- |
| **Background** | `#FFFDF6` | 温暖纸张底色 |
| **Secondary bg** | `#F4F0E8` | 分区底色 |
| **Primary** | `#2457A6` | 标题、生命周期与主结构 |
| **Accent** | `#F2A93B` | 操作、输入与重要提示 |
| **Secondary accent** | `#55A68B` | 正确路径、移动与完成状态 |
| **Body text** | `#1F2937` | 正文 |
| **Secondary text** | `#596273` | 注释与解释 |
| **Tertiary text** | `#8A8F98` | 页脚与弱信息 |
| **Border/divider** | `#D8D2C5` | 手绘边框与分隔 |
| **Ink** | `#1A1A1A` | 手绘轮廓 |
| **Surface** | `#FFF9E9` | 便签卡片 |
| **Grid** | `#E9E3D8` | 轻量网格线 |
| **Success** | `#2F8F6A` | 正确配置 |
| **Warning** | `#D85C45` | 风险与常见误区 |

### Gradient Scheme

不使用渐变。以纸张底色、柔和色块、手绘箭头和原生 SVG 几何建立层次。

---

## IV. Typography System

### Font Plan

**Typography direction**: 手写感标题 + 清晰的人文无衬线正文 + 等宽代码。

| Role | Chinese | English | Fallback tail |
| ---- | ------- | ------- | ------------- |
| **Title** | KaiTi | Trebuchet MS | serif |
| **Body** | Microsoft YaHei | Arial | sans-serif |
| **Emphasis** | KaiTi | Trebuchet MS | serif |
| **Code** | — | Consolas, Courier New | monospace |

**Per-role font stacks**:

- Title: `KaiTi, "Trebuchet MS", serif`
- Body: `"Microsoft YaHei", Arial, sans-serif`
- Emphasis: `KaiTi, "Trebuchet MS", serif`
- Code: `Consolas, "Courier New", monospace`

### Font Size Hierarchy

| Role | Size |
| ---- | ---: |
| Cover title | 92 |
| Page title | 56 |
| Subtitle | 42 |
| Lead / Subheading | 36 |
| Body | 32 |
| Code / Annotation | 24 |
| Footnote / Page number | 18 |

---

## V. Layout Principles

### Page Structure

- **Header area**: 46–126，左侧标题，右侧最多放一个概念图标。
- **Content area**: 138–632，按内容采用流程、三柱、左右非对称或对象容器图。
- **Footer area**: 660–710，统一课程名、分隔线与页码。

### Layout Pattern Library

- 封面与结尾使用负空间驱动的单焦点构图。
- 生命周期与移动算法用连续路径，突出“何时调用”和“状态如何变化”。
- 对比型内容用三柱或左右结构；对象模型使用外层容器包裹组件。
- 代码只保留核心语句，不把完整源码铺满页面。

### Spacing Specification

| Element | Current Project |
| ------- | --------------- |
| Safe margin | 58 |
| Content block gap | 30 |
| Icon-text gap | 12 |
| Card gap | 24 |
| Card padding | 26 |
| Card radius | 14 |

---

## VI. Icon Usage Specification

- **Built-in icon library**: `tabler-outline`
- **Stroke width**: 2
- **Usage method**: `<use data-icon="tabler-outline/icon-name" .../>`

| Purpose | Icon Path | Page |
| ------- | --------- | ---- |
| 脚本与组件 | `tabler-outline/code`, `components`, `cube` | P01–P03 |
| 时间与生命周期 | `tabler-outline/clock`, `repeat`, `player-play` | P04–P06 |
| 空间变换 | `tabler-outline/transform`, `arrows-move`, `hierarchy-3` | P07 |
| 对象操作 | `tabler-outline/search`, `plus`, `trash`, `box` | P08–P09 |
| 角色移动 | `tabler-outline/walk`, `jump-rope`, `arrow-big-up` | P10–P12 |
| 摄像机与场景闭环 | `tabler-outline/camera`, `check`, `package` | P13 |

---

## VII. Visualization Reference List

Catalog read: 76 templates

| Page | Template | Path | Summary-quote | Usage |
| ---- | -------- | ---- | ------------- | ----- |
| P02 | agenda_list | `templates/charts/agenda_list.svg` | "Pick for table of contents, meeting agendas, or presentation roadmap — numbered items + brief description + duration / owner per row. Skip for substantive content lists (use vertical_list) or single-page section dividers (use a cover layout)." | 五段学习路线 |
| P03 | module_composition | `templates/charts/module_composition.svg` | "Pick for one parent container wrapping 3-N child module cards, each = title + 2-3 bullets — fits 'Feature X contains 3 parts, each with its own description'. Skip if source has only labels without descriptions (use numbered_steps or icon_grid)." | GameObject 容器与组件组成 |
| P04 | chevron_process | `templates/charts/chevron_process.svg` | "Pick for 3-6 phase methodology with chunky arrow-chain progression and deliverables per phase. Skip for <=2 phases or non-linear flow (use process_flow), or chain ending in an aggregate outcome wedge (use chevron_chain_with_tail)." | 生命周期函数主序列 |
| P05 | vertical_list | `templates/charts/vertical_list.svg` | "Pick for 3-6 numbered key points each with a short description — design principles, core tenets, action items, key takeaways, recommendations, executive summary points. Skip for icon-style cards (use icon_grid) or sequential steps (use numbered_steps)." | 生命周期函数选择规则 |
| P06 | vertical_pillars | `templates/charts/vertical_pillars.svg` | "Pick for 1×3 / 1×4 / 1×5 vertical column layout where each pillar = one independent category with title + bullets — PEST (Political/Economic/Social/Technological), four-pillar strategy overview, side-by-side independent categories. Skip for 2×2 quadrant (use quadrant_text_bullets), pricing tiers (use comparison_columns), or 2×2 parallel aspects (use labeled_card)." | Update / FixedUpdate / LateUpdate 对照 |
| P07 | labeled_card | `templates/charts/labeled_card.svg` | "Pick for 3-4 parallel aspects of one subject with per-aspect titles + short body (self-introduction, four-pillar overview, capability quadrant). Skip for plain feature lists (use icon_grid), sequential steps (use numbered_steps), or strategic quadrants (use quadrant_text_bullets / matrix_2x2)." | Transform 三要素与层级 |
| P08 | module_composition | `templates/charts/module_composition.svg` | 同 P03 | GameObject、组件引用与激活状态 |
| P09 | pipeline_with_stages | `templates/charts/pipeline_with_stages.svg` | "Pick for 3-5 horizontal pipeline stages, each = title + 1-line description + output artifact, connected by arrows (data pipelines, ETL, build pipelines). Skip if any stage lacks an artifact (use process_flow or numbered_steps)." | 引用→创建→运行→销毁路径 |
| P10 | icon_grid | `templates/charts/icon_grid.svg` | "Pick for 4-9 parallel features/capabilities/services as icon cards — feature grid, service lineup, benefits matrix, brand values, product highlights. Skip for sequential ordering (use numbered_steps) or hierarchical layers (use pyramid_chart)." | CharacterController 属性与能力 |
| P11 | snake_flow | `templates/charts/snake_flow.svg` | "Pick for 6-10 winding sequential steps fitting a long journey/lifecycle on one slide. Skip for <=5 steps (use numbered_steps)." | 移动、转向、重力和跳跃算法 |
| P12 | vertical_pillars | `templates/charts/vertical_pillars.svg` | 同 P06 | CharacterController 与 Rigidbody 对照 |
| P13 | numbered_steps | `templates/charts/numbered_steps.svg` | "Pick for 3-6 horizontal sequential steps with numeric emphasis — how-it-works section, getting-started guide, methodology overview, implementation phases. Skip if steps need connector arrows (use process_flow) or named output artifacts (use pipeline_with_stages)." | 角色与摄像机的场景闭环 |

**Runners-up considered**:

- `timeline` | rejected for P04：生命周期不仅是时间点，还需要强调可重复阶段与退出阶段。
- `comparison_columns` | rejected for P12：用课程现有三柱语法加入“选择规则”比密集对照表更易讲授。
- `process_flow` | rejected for P11：完整移动算法有 7 个状态步骤，蛇形路径更适合单页展开。

---

## VIII. Image Resource List

No external image resources. All illustrative content is authored as native SVG geometry and approved icons. Image generation is skipped per user instruction.

---

## IX. Content Outline

### Part 1: 脚本如何成为组件

#### Slide 01 - Cover

- **Cover impact**: 以 C# 脚本卡插入 GameObject 组件槽为视觉钩子，旁边用循环箭头暗示生命周期。
- **Layout**: 左侧大标题，右侧原生 SVG 对象容器、代码卡与生命周期环。
- **Title**: Unity 语法基础
- **Subtitle**: MonoBehaviour、生命周期与常用对象

#### Slide 02 - 学习路线

- **Title**: 从“脚本挂上去”走到“角色跑起来”
- **Core message**: 本课沿着脚本身份、生命周期、对象操作、角色移动和场景闭环推进。
- **Visualization**: agenda_list
- **Content**: ①脚本与组件；②生命周期与更新循环；③Transform / GameObject；④CharacterController；⑤角色与摄像机。

#### Slide 03 - Unity 脚本是组件

- **Title**: MonoBehaviour 让代码进入场景
- **Core message**: GameObject 是容器，Transform 是必备组件，自定义脚本只有挂载后才参与运行。
- **Visualization**: module_composition
- **Content**: Rotator.cs ↔ public class Rotator；transform、gameObject、GetComponent<T>()；类型名与实例名的大小写区别。

### Part 2: 在正确的时机做正确的事

#### Slide 04 - 生命周期主序列

- **Title**: 生命周期函数由 Unity 按时机调用
- **Core message**: 从加载、启用、启动、循环更新到禁用和销毁，每个入口都有明确职责。
- **Visualization**: chevron_process
- **Content**: Awake→OnEnable→Start→Update / FixedUpdate / LateUpdate→OnDisable→OnDestroy；对象再次启用会再次 OnEnable，Start 通常只一次。

#### Slide 05 - 生命周期函数选择

- **Title**: 先问“这件事何时发生、会发生几次”
- **Core message**: 自身引用放 Awake，订阅和取消成对，依赖外部初始化放 Start，清理按实例寿命选择 OnDisable 或 OnDestroy。
- **Visualization**: vertical_list
- **Content**: 获取自身组件；订阅/取消；依赖其他对象；每帧逻辑；实例销毁前清理。强调不同对象 Awake 顺序不应被默认。

#### Slide 06 - 三个更新循环

- **Title**: Update、FixedUpdate、LateUpdate 各管一段节奏
- **Core message**: 输入与普通逻辑、物理时间步、跟随收尾应分开放置。
- **Visualization**: vertical_pillars
- **Content**: Update 读取输入并乘 deltaTime；FixedUpdate 对 Rigidbody 施力；LateUpdate 在角色更新后跟随；一次性触发不乘 deltaTime。

### Part 3: 操作 Transform 与 GameObject

#### Slide 07 - Transform

- **Title**: Transform 同时保存空间状态与父子层级
- **Core message**: 世界与局部坐标、方向向量、父子关系和坐标转换是同一套空间语义。
- **Visualization**: labeled_card
- **Content**: position / localPosition；forward / right / up；Rotate / LookRotation；SetParent(true/false)；TransformPoint 与 TransformDirection 不混用。

#### Slide 08 - GameObject 与组件引用

- **Title**: 引用先连好，运行时只做必要查找
- **Core message**: 通过 gameObject 操作激活状态，通过 GetComponent 获取能力，通过 Inspector 或初始化缓存引用。
- **Visualization**: module_composition
- **Content**: activeSelf 与 activeInHierarchy；GetComponent、TryGetComponent、InChildren、InParent；Awake 缓存；固定配置优先 Inspector / Prefab。

#### Slide 09 - 对象从引用到销毁

- **Title**: 创建对象容易，维护引用更重要
- **Core message**: 可靠流程是先获得稳定引用，再创建、使用并在合适时机销毁；名称查找不应进入每帧循环。
- **Visualization**: pipeline_with_stages
- **Content**: SerializeField / 初始化查找→Instantiate→组件交互与 CompareTag→Destroy；AddComponent 用于动态能力；Find 只在必要初始化时使用并缓存。

### Part 4: CharacterController 角色移动

#### Slide 10 - CharacterController 心智模型

- **Title**: Move 处理碰撞，但不会替你处理重力
- **Core message**: CharacterController 是脚本直接控制的胶囊角色，不是参与普通动力学的 Rigidbody。
- **Visualization**: icon_grid
- **Content**: height/radius/center、slopeLimit、stepOffset、skinWidth、isGrounded、velocity；Move 参数是位移，SimpleMove 参数是速度且自动重力。

#### Slide 11 - 完整移动算法

- **Title**: 一帧移动其实是七个连续决策
- **Core message**: 输入归一化、转向、贴地、跳跃、重力、合成速度、碰撞修正共同形成稳定移动。
- **Visualization**: snake_flow
- **Content**: 读 WASD→ClampMagnitude→LookRotation + Slerp→贴地速度 -2→跳跃初速度 sqrt(h×-2g)→重力积分→Move 后检查 Above。

#### Slide 12 - CharacterController vs Rigidbody

- **Title**: 先决定“直接控制”还是“物理响应”
- **Core message**: 玩家角色通常重视可控性，箱子、球与载具通常重视动力学；不要让两套系统争夺同一根节点。
- **Visualization**: vertical_pillars
- **Content**: CharacterController=脚本 Move、自管重力、坡度台阶友好；Rigidbody=力/速度、物理重力、可被其他刚体推动；中间柱给出选择规则和禁忌。

#### Slide 13 - 场景闭环

- **Closing impact**: 用 Player 先移动、Camera 后跟随的时间顺序收束全课，形成可运行的最小场景。
- **Title**: 把脚本放回场景：角色先走，摄像机后跟
- **Core message**: Player 在 Update 中移动，Camera 在 LateUpdate 中追随，target 通过 Inspector 连接，形成清晰且稳定的职责分工。
- **Visualization**: numbered_steps
- **Content**: ①Player 根节点加 CharacterController；②挂 SimpleCharacterMover；③地面有 Collider；④Main Camera 挂 SimpleCameraFollow 并连接 target；⑤播放测试 WASD、跳跃、碰撞与跟随。

---

## X. Speaker Notes Requirements

- **Filename**: match SVG basename, e.g. `01_cover.md`.
- **Total duration**: 35–45 minutes.
- **Style**: conversational and instructional, each page 3–5 sentences.
- **Purpose**: explain lifecycle decisions, object-reference discipline, and the complete character-movement reasoning chain.

