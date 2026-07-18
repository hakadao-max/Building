# Unity 基础：场景组件、物理、渲染与打包 - Design Spec

> Human-readable design narrative. The execution contract is `spec_lock.md`.

## I. Project Information

| Item | Value |
| ---- | ----- |
| **Project Name** | unity_scene_components_build_ppt169_20260718 |
| **Canvas Format** | PPT 16:9 (1280×720) |
| **Page Count** | 13 |
| **Design Style** | Instructional + sketch-notes |
| **Target Audience** | Unity 初学者、职业教育或高校游戏开发入门课程学员 |
| **Use Case** | 课堂讲授、投影演示、课后复习 |
| **Delivery Purpose** | presentation |
| **Content Strategy** | balanced default：重组长文为“对象模型→物理→渲染→复用→打包”的教学路径，事实与操作步骤均来自源文档 |
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
- **Tone**: 温暖、可操作、强调系统关系与排查思路

### Color Scheme

| Role | HEX | Purpose |
| ---- | --- | ------- |
| **Background** | `#FFFDF6` | 温暖纸张底色 |
| **Secondary bg** | `#F4F0E8` | 分区底色 |
| **Primary** | `#2457A6` | 标题、结构关系、关键组件 |
| **Accent** | `#F2A93B` | 操作步骤、提示与输出 |
| **Secondary accent** | `#55A68B` | 正确路径、复用与流程 |
| **Body text** | `#1F2937` | 正文 |
| **Secondary text** | `#596273` | 注释与解释 |
| **Tertiary text** | `#8A8F98` | 页脚与弱信息 |
| **Border/divider** | `#D8D2C5` | 手绘边框与分隔 |
| **Ink** | `#1A1A1A` | 手绘轮廓 |
| **Surface** | `#FFF9E9` | 便签卡片 |
| **Grid** | `#E9E3D8` | 轻量网格线 |
| **Success** | `#2F8F6A` | 正确配置 |
| **Warning** | `#D85C45` | 风险、排查项 |

### Gradient Scheme

不使用渐变。页面依靠纸张底色、手绘线条与柔和色块建立层次。

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
| ---- | ----: |
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

- **Header area**: 46–126，左侧标题，右侧可放单个概念图标。
- **Content area**: 138–632，按信息重量使用树状、流程、左右非对称或大图解结构。
- **Footer area**: 660–710，统一课程名、分隔线与页码。

### Layout Pattern Library

- 封面与结尾采用负空间驱动的单焦点构图。
- 概念关系优先用树、层、流程和因果线，而非重复卡片网格。
- 对照型内容使用 3 列或左右非对称结构；操作步骤使用连续路径。
- 代码只保留核心片段，放入明确的等宽文本区域。

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
| 场景对象 | `tabler-outline/cube` | P01–P03 |
| 层级 | `tabler-outline/hierarchy-3` | P02–P03 |
| 变换 | `tabler-outline/transform` | P04 |
| 碰撞形状 | `tabler-outline/box`, `circle`, `capsule` | P05 |
| 触发区域 | `tabler-outline/ghost` | P06–P07 |
| 材质 | `tabler-outline/palette` | P08 |
| 光照 | `tabler-outline/sun`, `bulb` | P09 |
| 摄像机输出 | `tabler-outline/camera`, `device-tv` | P10 |
| 复用与角色 | `tabler-outline/package`, `walk` | P11 |
| 综合实践 | `tabler-outline/hammer`, `checklist` | P12 |
| 构建导出 | `tabler-outline/file-export`, `package-export` | P13 |

---

## VII. Visualization Reference List

Catalog read: 76 templates

| Page | Template | Path | Summary-quote (verbatim from `charts_index.json`) | Usage |
| ---- | -------- | ---- | ------------------------------------------------- | ----- |
| P02 | agenda_list | `templates/charts/agenda_list.svg` | "Pick for table of contents, meeting agendas, or presentation roadmap — numbered items + brief description + duration / owner per row. Skip for substantive content lists (use vertical_list) or single-page section dividers (use a cover layout)." | 四段学习路线 |
| P03 | top_down_tree | `templates/charts/top_down_tree.svg` | "Pick for hierarchical top-down tree 2-4 levels deep with parent→children reporting/decomposition lines — org charts (CEO → VPs → Directors), OKR cascades (Objective → Key Results → Initiatives), WBS decomposition. Skip for non-hierarchical brainstorm (use mind_map) or flat team showcase (use team_roster)." | Scene→GameObject→Component 层级 |
| P04 | labeled_card | `templates/charts/labeled_card.svg` | "Pick for 3-4 parallel aspects of one subject with per-aspect titles + short body (self-introduction, four-pillar overview, capability quadrant). Skip for plain feature lists (use icon_grid), sequential steps (use numbered_steps), or strategic quadrants (use quadrant_text_bullets / matrix_2x2)." | Position / Rotation / Scale 三要素 |
| P05 | icon_grid | `templates/charts/icon_grid.svg` | "Pick for 4-9 parallel features/capabilities/services as icon cards — feature grid, service lineup, benefits matrix, brand values, product highlights. Skip for sequential ordering (use numbered_steps) or hierarchical layers (use pyramid_chart)." | Collider 类型与适用对象 |
| P06 | vertical_pillars | `templates/charts/vertical_pillars.svg` | "Pick for 1×3 / 1×4 / 1×5 vertical column layout where each pillar = one independent category with title + bullets — PEST (Political/Economic/Social/Technological), four-pillar strategy overview, side-by-side independent categories. Skip for 2×2 quadrant (use quadrant_text_bullets), pricing tiers (use comparison_columns), or 2×2 parallel aspects (use labeled_card)." | 静态 / 动态 / 运动学刚体 |
| P07 | vertical_list | `templates/charts/vertical_list.svg` | "Pick for 3-6 numbered key points each with a short description — design principles, core tenets, action items, key takeaways, recommendations, executive summary points. Skip for icon-style cards (use icon_grid) or sequential steps (use numbered_steps)." | Trigger 不触发的排查清单 |
| P08 | layered_architecture | `templates/charts/layered_architecture.svg` | "Pick for 3-4 horizontal architecture layers (presentation/service/data), 2-4 module cards per layer, each card = title + 1-line description (description required, even if source brief). Skip if no per-module descriptions (use icon_grid) or no horizontal layering (use module_composition)." | Texture→Material→Shader→Renderer 渲染链 |
| P09 | chevron_process | `templates/charts/chevron_process.svg` | "Pick for 3-6 phase methodology with chunky arrow-chain progression and deliverables per phase. Skip for <=2 phases or non-linear flow (use process_flow), or chain ending in an aggregate outcome wedge (use chevron_chain_with_tail)." | 光照烘焙迭代 |
| P10 | pipeline_with_stages | `templates/charts/pipeline_with_stages.svg` | "Pick for 3-5 horizontal pipeline stages, each = title + 1-line description + output artifact, connected by arrows (data pipelines, ETL, build pipelines). Skip if any stage lacks an artifact (use process_flow or numbered_steps)." | Camera→Render Texture→RawImage |
| P11 | module_composition | `templates/charts/module_composition.svg` | "Pick for one parent container wrapping 3-N child module cards, each = title + 2-3 bullets — fits 'Feature X contains 3 parts, each with its own description'. Skip if source has only labels without descriptions (use numbered_steps or icon_grid)." | Prefab 与角色根节点组成 |
| P12 | snake_flow | `templates/charts/snake_flow.svg` | "Pick for 6-10 winding sequential steps fitting a long journey/lifecycle on one slide. Skip for <=5 steps (use numbered_steps)." | 小房间实践的六阶段路径 |
| P13 | numbered_steps | `templates/charts/numbered_steps.svg` | "Pick for 3-6 horizontal sequential steps with numeric emphasis — how-it-works section, getting-started guide, methodology overview, implementation phases. Skip if steps need connector arrows (use process_flow) or named output artifacts (use pipeline_with_stages)." | Windows 构建四步检查 |

**Runners-up considered**:

- `process_flow` | rejected for P09：烘焙强调阶段与交付物，粗箭头阶段链更合适。
- `comparison_table` | rejected for P05：Collider 是并列能力速览，不需要密集特征矩阵。
- `roadmap_vertical` | rejected for P12：实践强调一次课堂内的顺序流，不是按时间状态追踪。

---

## VIII. Image Resource List

No external image resources. All illustrative content is authored as native SVG geometry and approved icons. Image generation is skipped by user-authorized fallback.

---

## IX. Content Outline

### Part 1: 建立 Unity 对象模型

#### Slide 01 - Cover

- **Cover impact**: 以“Hierarchy 节点树最终装入 Windows 构建盒”为视觉钩子，使用一条从场景到可执行程序的手绘流水线贯穿封面。
- **Layout**: 左侧大标题，右侧原生 SVG 场景树、组件齿轮与打包盒组成单一英雄图。
- **Title**: Unity 基础
- **Subtitle**: 场景、组件、物理、渲染与打包

#### Slide 02 - 一条完整的学习路线

- **Layout**: 四段纵向议程 + 右侧终点标记。
- **Title**: 从“场景里有什么”走到“玩家拿到什么”
- **Core message**: 本课把 Unity 基础串成对象、物理、画面与交付四段链路。
- **Visualization**: agenda_list
- **Content**: 对象模型与 Transform / 物理与 Trigger / 渲染与光照 / Prefab、实践与打包。

#### Slide 03 - GameObject 是容器，Component 提供能力

- **Layout**: 上下树状分解，右侧用一句口诀收束。
- **Title**: 先读懂 Scene 的对象树
- **Core message**: Scene 由 GameObject 组成，而每个 GameObject 通过 Component 获得具体能力。
- **Visualization**: top_down_tree
- **Content**: Scene→Environment、Characters、Cameras、Lighting、Systems；每个对象必有 Transform，其余能力由 Collider、Renderer、Script 等组件添加。

#### Slide 04 - Transform 与父子层级

- **Layout**: 左侧三轴手绘对象，右侧 Position / Rotation / Scale 三块说明，下方父子跟随例子。
- **Title**: Transform 决定“在哪里、朝哪里、多大”
- **Core message**: 父物体控制整体变化，子物体保留局部偏移，层级因此成为稳定的空间组织工具。
- **Visualization**: labeled_card
- **Content**: 世界空间与局部空间；PlayerRoot→Model、InteractionPoint、NameCanvas；Pivot/Center 只改变手柄位置；用空父物体修正模型轴心。

### Part 2: 让对象参与物理世界

#### Slide 05 - Collider 是物理形状，不是可视模型

- **Layout**: 五种形状图标矩阵 + 底部复合碰撞体原则。
- **Title**: 碰撞体越简单，通常越稳定
- **Core message**: 复杂外观应由简单 Collider 组合近似，动态物体尤其避免直接依赖复杂 Mesh Collider。
- **Visualization**: icon_grid
- **Content**: Box 用于墙箱；Sphere 用于球与范围；Capsule 用于直立角色；Mesh 用于复杂静态环境；Terrain/Character Controller 用于专用场景。Physics Material 控制摩擦与弹性。

#### Slide 06 - Collider 与 Rigidbody 的三种组合

- **Layout**: 三柱对照 + 中央接触条件线。
- **Title**: 先判断对象属于静态、动态还是运动学
- **Core message**: 是否拥有 Rigidbody、是否开启 Is Kinematic，决定对象如何运动与接收碰撞。
- **Visualization**: vertical_pillars
- **Content**: 静态碰撞体=Collider 无 Rigidbody；动态刚体=Collider+非 Kinematic Rigidbody；运动学刚体=脚本或动画驱动。两 Collider 接触时至少一方通常需要 Rigidbody 才能稳定收到回调。

#### Slide 07 - Trigger 不触发时怎么查

- **Layout**: 左侧进入/停留/离开回调，右侧六项排查路径。
- **Title**: Trigger 只检测区域，不负责阻挡
- **Core message**: 触发器问题优先检查“维度、组件、层、函数签名、激活状态”五类前置条件。
- **Visualization**: vertical_list
- **Content**: 3D 与 2D 组件不能混用；Is Trigger 必须开启；至少一方参与物理检测；Layer Collision Matrix 不能关闭接触；函数名和参数必须准确；对象与脚本需激活。

### Part 3: 从网格到最终画面

#### Slide 08 - 渲染链：Texture、Material、Shader、Renderer

- **Layout**: 四层横向架构 + 右侧 Skinned Mesh Renderer 对照提示。
- **Title**: 材质不是贴图，Renderer 也不是模型本身
- **Core message**: Shader 定义计算方式，Material 保存参数，Texture 提供输入，Renderer 把结果显示在对象上。
- **Visualization**: layered_architecture
- **Content**: URP Lit 的 Base Map、Metallic、Smoothness、Normal、Emission；Skinned Mesh Renderer 额外依赖骨骼、权重、Bounds；粉红模型通常表示 Shader 不兼容或丢失。

#### Slide 09 - 天空盒、灯光与 Bake

- **Layout**: 上部环境光关系图，下部四阶段烘焙箭头链。
- **Title**: 先定主光，再用烘焙保存静态环境的光
- **Core message**: 天空盒、灯光模式、Lightmap 与 Probe 共同决定场景氛围和运行时成本。
- **Visualization**: chevron_process
- **Content**: Realtime 灵活但成本高；Baked 适合静态环境；Mixed 结合两者。低质量快速烘焙→检查曝光与漏光→修正 UV/几何/灯光→最终质量烘焙。

#### Slide 10 - Camera 与 Render Texture

- **Layout**: 三阶段输出管线 + 透视/正交和裁剪面边注。
- **Title**: Camera 把三维世界变成玩家看到的画面
- **Core message**: 第二台 Camera 输出到 Render Texture，再由 RawImage 显示，就能得到监控画面、小地图或角色预览。
- **Visualization**: pipeline_with_stages
- **Content**: Camera 决定 Projection、FOV、Clipping、Culling Mask 与背景；MainCamera 标签供 Camera.main 查找；近远裁剪面与 Layer 直接影响显示与成本。

### Part 4: 复用、实践与交付

#### Slide 11 - Prefab 与角色根节点

- **Layout**: 左侧 Prefab 资产与实例关系，右侧 PlayerRoot 组件组成。
- **Title**: 把稳定配置做成 Prefab，把职责拆到子节点
- **Core message**: Prefab 保存层级、组件、参数与资源引用，角色根节点则分离控制、模型与摄像机锚点。
- **Visualization**: module_composition
- **Content**: Apply 把实例修改写回 Prefab，Revert 放弃覆盖；Variant 保存差异；PlayerRoot 包含 CharacterController、MovementScript、ModelRoot、CameraTarget；移动用速度×Time.deltaTime，并自行维护重力。

#### Slide 12 - 综合课堂实践：可探索的小房间

- **Layout**: 六段蛇形路径，终点是可运行场景。
- **Title**: 把知识点放进同一个可探索场景
- **Core message**: 一次完整实践应把场景、材质、光照、角色、Trigger、摄像机与 Prefab 串成可测试闭环。
- **Visualization**: snake_flow
- **Content**: ①搭建地面墙门；②配置材质天空盒主灯；③烘焙环境；④创建玩家；⑤门前 Trigger 驱动动画；⑥第二 Camera→Render Texture→RawImage，并把玩家和门制成 Prefab。

#### Slide 13 - 构建 Windows 版本

- **Closing impact**: 观众带走“场景正确只是开始，交付必须连同场景列表、Player 设置与完整输出目录一起验证”的结论；用四步检查路径最终落到一个完整构建文件夹。
- **Layout**: 四个大步骤从检查、Profile、Build 到交付，右侧绘制 exe 与 Data 文件夹成对出现。
- **Title**: 最后一公里：Build 不是只生成一个 .exe
- **Core message**: 可靠构建来自编译无错、场景顺序正确、平台设置完整，并交付整个输出目录。
- **Visualization**: numbered_steps
- **Content**: 保存并清空 Console 错误→在 Build Profiles 选择 Windows 并配置 Scene List→设置 Player 与 Development Build→输出到空目录。发布时一起交付 MyGame.exe、MyGame_Data、UnityPlayer.dll 与运行库文件。

---

## X. Speaker Notes Requirements

- **Filename**: match SVG basename, e.g. `01_cover.md`.
- **Total duration**: 35–45 minutes.
- **Style**: conversational and instructional, each page 3–5 sentences.
- **Purpose**: explain concepts, reinforce diagnostic thinking, and connect the workflow to a buildable classroom practice.
