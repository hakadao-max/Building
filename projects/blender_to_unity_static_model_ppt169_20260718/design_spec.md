# Blender 模型到 Unity 游戏物体 - Design Spec

> Human-readable design narrative. The execution contract is `spec_lock.md`.

## I. Project Information

| Item | Value |
| ---- | ----- |
| **Project Name** | blender_to_unity_static_model_ppt169_20260718 |
| **Canvas Format** | PPT 16:9 (1280×720) |
| **Page Count** | 13 |
| **Design Style** | Instructional + sketch-notes |
| **Target Audience** | Blender 与 Unity 初学者、游戏美术与技术美术入门课程学员 |
| **Use Case** | 课堂讲授、资产管线培训、导入故障排查 |
| **Delivery Purpose** | presentation |
| **Content Strategy** | balanced default：重组为“准备→导出→导入→材质→Prefab→验证”的静态模型交付链，保留关键设置与错误症状 |
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
- **Tone**: 温暖、清晰、强调交付边界与可复现设置

### Color Scheme

| Role | HEX | Purpose |
| ---- | --- | ------- |
| **Background** | `#FFFDF6` | 温暖纸张底色 |
| **Secondary bg** | `#F4F0E8` | 分区底色 |
| **Primary** | `#2457A6` | Unity 端、标题、结构关系 |
| **Accent** | `#F2A93B` | Blender 端、导出操作与提醒 |
| **Secondary accent** | `#55A68B` | 正确设置与验证完成 |
| **Body text** | `#1F2937` | 正文 |
| **Secondary text** | `#596273` | 注释与解释 |
| **Tertiary text** | `#8A8F98` | 页脚与弱信息 |
| **Border/divider** | `#D8D2C5` | 手绘边框与分隔 |
| **Ink** | `#1A1A1A` | 手绘轮廓 |
| **Surface** | `#FFF9E9` | 便签卡片 |
| **Grid** | `#E9E3D8` | 轻量网格线 |
| **Success** | `#2F8F6A` | 推荐设置 |
| **Warning** | `#D85C45` | 风险、异常与禁忌 |

### Gradient Scheme

不使用渐变。通过米白纸张底色、手绘箭头、轴向线和柔和色块建立层次。

---

## IV. Typography System

### Font Plan

**Typography direction**: 手写感标题 + 清晰的人文无衬线正文 + 等宽设置值。

| Role | Chinese | English | Fallback tail |
| ---- | ------- | ------- | ------------- |
| **Title** | KaiTi | Trebuchet MS | serif |
| **Body** | Microsoft YaHei | Arial | sans-serif |
| **Emphasis** | KaiTi | Trebuchet MS | serif |
| **Code / settings** | — | Consolas, Courier New | monospace |

### Font Size Hierarchy

| Role | Size |
| ---- | ---: |
| Cover title | 92 |
| Page title | 56 |
| Subtitle | 42 |
| Lead / Subheading | 36 |
| Body | 32 |
| Code / Annotation | 24 |
| Dense body / label | 23 / 22 |
| Microcopy | 20 |
| Footnote / Page number | 18 |

---

## V. Layout Principles

### Page Structure

- **Header area**: 46–126，左侧标题，右侧最多一个模型或导出图标。
- **Content area**: 138–632，按内容使用流程、三柱、设置面板、轴向图或对象容器。
- **Footer area**: 660–710，统一课程名、分隔线与页码。

### Layout Pattern Library

- Blender 使用橙色强调，Unity 使用蓝色强调，验证与正确结果使用青绿色。
- 复杂设置按“必选 / 推荐 / 禁用”分组，不照搬密集界面截图。
- 故障排查以“症状→原因→修复”表达，避免只列选项名称。
- 代码与路径只保留关键值，用等宽字体呈现。

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
| 模型与 FBX | `tabler-outline/cube`, `file-3d`, `file-export` | P01–P04 |
| 文件与贴图 | `tabler-outline/folder`, `files`, `photo` | P03–P05, P11 |
| 轴向与单位 | `tabler-outline/transform`, `axis-x`, `axis-y`, `ruler` | P06 |
| 几何与法线 | `tabler-outline/triangles`, `layers-subtract`, `sparkles` | P07 |
| Unity 导入 | `tabler-outline/download`, `settings`, `checklist` | P08–P10 |
| 材质与光照 | `tabler-outline/palette`, `sun` | P11 |
| Prefab 与职责 | `tabler-outline/hierarchy-3`, `box`, `hammer` | P12 |
| 风险与验收 | `tabler-outline/alert-triangle`, `checklist`, `package-export` | P13 |

---

## VII. Visualization Reference List

Catalog read: 76 templates

| Page | Template | Path | Usage |
| ---- | -------- | ---- | ----- |
| P02 | agenda_list | `templates/charts/agenda_list.svg` | 六段学习路线 |
| P03 | pipeline_with_stages | `templates/charts/pipeline_with_stages.svg` | `.blend` 到明确交付物的文件链 |
| P04 | labeled_card | `templates/charts/labeled_card.svg` | 静态模型导出核心预设 |
| P05 | vertical_pillars | `templates/charts/vertical_pillars.svg` | 路径、分批、包括三类设置 |
| P06 | labeled_card | `templates/charts/labeled_card.svg` | 单位、轴向、应用变换 |
| P07 | icon_grid | `templates/charts/icon_grid.svg` | 几何、法线、修改器与动画开关 |
| P08 | numbered_steps | `templates/charts/numbered_steps.svg` | 保存预设与导出前检查 |
| P09 | vertical_list | `templates/charts/vertical_list.svg` | Unity Model 页签检查 |
| P10 | vertical_pillars | `templates/charts/vertical_pillars.svg` | Rig、Animation 与异常回溯 |
| P11 | pipeline_with_stages | `templates/charts/pipeline_with_stages.svg` | 贴图→导入类型→材质→Renderer |
| P12 | module_composition | `templates/charts/module_composition.svg` | 外层 Prefab 与职责边界 |
| P13 | numbered_steps | `templates/charts/numbered_steps.svg` | 端到端验收闭环 |

**Runners-up considered**:

- `comparison_table` | rejected for P05：三类设置更适合并列讲解而非密集矩阵。
- `layered_architecture` | rejected for P12：重点是外层容器包裹导入模型，不是抽象软件层次。
- `snake_flow` | rejected for P13：验收只需五阶段，不必使用长流程。

---

## VIII. Image Resource List

No external image resources. All Blender panels, axis illustrations, model containers, textures and workflow diagrams are authored as native SVG geometry and approved icons. Image generation is skipped per user instruction.

---

## IX. Content Outline

### Part 1: 建立稳定交付边界

#### Slide 01 - Cover

- **Title**: Blender 模型到 Unity 游戏物体
- **Subtitle**: 静态模型的 FBX 导出、导入与 Prefab 管线
- **Cover impact**: Blender 模型经过 FBX 文件进入 Unity Prefab，形成一条手绘资产传送带。

#### Slide 02 - 学习路线

- **Title**: 从模型文件走到可用游戏物体
- **Core message**: 本课按准备、导出、导入、材质、Prefab、验收六段推进。
- **Visualization**: agenda_list

#### Slide 03 - 为什么交付 FBX

- **Title**: 提交明确导出的 FBX，不要让 Unity 依赖本机 Blender
- **Core message**: `.blend` 直导依赖本机版本，稳定交付应包含明确导出的 FBX 与独立贴图。
- **Visualization**: pipeline_with_stages
- **Content**: Blender 源文件→导出 FBX→独立贴图→Unity Assets；材质节点不能完整自动转换。

### Part 2: Blender 静态模型导出

#### Slide 04 - 一套可复用的导出预设

- **Title**: 普通静态模型先锁定这组核心值
- **Core message**: 选定对象、网格类型、1.00 缩放、-Z/Y 轴向、应用单位、关闭动画构成稳定起点。
- **Visualization**: labeled_card

#### Slide 05 - 路径、分批与包括

- **Title**: 只导出需要的对象，保持贴图独立可管理
- **Core message**: 路径模式推荐自动，分批关闭，限制到选定物体，类型只选网格并按需加入空物体。
- **Visualization**: vertical_pillars

#### Slide 06 - 单位、轴向与变换

- **Title**: 尺寸和朝向应在 Blender 源头正确
- **Core message**: Metric、Unit Scale 1.0、Rotation & Scale 已应用、-Z 向前 / Y 向上，避免在 Unity 用 0.01 长期补偿。
- **Visualization**: labeled_card

#### Slide 07 - 几何数据与动画开关

- **Title**: 导出最终 Mesh，但不要夹带无关数据
- **Core message**: 仅法线、应用修改器、默认不导出细分/松散边/切线空间，静态模型关闭骨架与动画。
- **Visualization**: icon_grid

#### Slide 08 - 保存预设与导出动作

- **Title**: 把正确设置保存成 Unity_静态模型
- **Core message**: 每次先套预设，再复核选定对象、类型、变换、几何与动画，降低漏改概率。
- **Visualization**: numbered_steps

### Part 3: Unity 导入与游戏化

#### Slide 09 - Model 页签

- **Title**: 尺寸、法线、切线、UV 与运行时成本逐项检查
- **Core message**: Scale Factor 1、Convert Units、Normals Import、MikkTSpace，Read/Write 默认关闭，碰撞体与压缩按用途选择。
- **Visualization**: vertical_list

#### Slide 10 - Rig、Animation 与异常回溯

- **Title**: 静态模型在 Unity 里也应保持“静态”
- **Core message**: Rig=None、Import Animation 关闭；如果仍有 Clip，应回 Blender 检查导出动画开关。
- **Visualization**: vertical_pillars

#### Slide 11 - 材质与贴图

- **Title**: 程序化材质不会自动还原，贴图类型必须明确
- **Core message**: Base Color 用 sRGB，Normal 设 Normal Map，数据贴图关闭 sRGB，Roughness 需反相为 Smoothness。
- **Visualization**: pipeline_with_stages

#### Slide 12 - 外层 Prefab

- **Title**: 不要直接修改 FBX 内部对象
- **Core message**: 外层 ObjectRoot 持有 Collider、Rigidbody 与脚本，Imported FBX 只负责模型表现，重导入更安全。
- **Visualization**: module_composition

#### Slide 13 - 快速检查与交付

- **Title**: 最终验收：Scale=1、朝向正确、材质正常、碰撞可用
- **Core message**: 在实际场景中完成 Blender 设置、Unity 导入、材质、Prefab 与运行验证，才算资产交付完成。
- **Visualization**: numbered_steps
- **Closing impact**: 以完整 Prefab 进入场景并亮起绿色检查标记收束。

---

## X. Speaker Notes Requirements

- **Filename**: match SVG basename, e.g. `P01_封面.md`.
- **Total duration**: 35–45 minutes.
- **Style**: conversational and instructional, each page 2–4 sentences.
- **Purpose**: explain why each setting exists, connect visible symptoms to pipeline causes, and reinforce the Blender / Unity ownership boundary.

