# C# 语法基础：类、继承、方法、静态与参数 - Design Spec

> Human-readable design narrative — rationale, audience, style, color choices, content outline.
>
> Machine-readable execution contract: `spec_lock.md`.

## I. Project Information

| Item | Value |
| ---- | ----- |
| **Project Name** | C# 语法基础：类、继承、方法、静态与参数 |
| **Canvas Format** | PPT 16:9 (1280×720) |
| **Page Count** | 10 |
| **Design Style** | `instructional` × `sketch-notes`：循序教学、课堂手绘、代码示例驱动 |
| **Target Audience** | 正在学习 Unity 或 C# 的零基础到初级开发者 |
| **Use Case** | 课堂投屏、训练营讲解、入门课程知识分享 |
| **Delivery Purpose** | `presentation` — 单页一个概念、32 基准正文、强调远距离可读性 |
| **Content Strategy** | balanced default；重组为教学顺序，但所有事实、示例与结论均来自源文档 |
| **Created Date** | 2026-07-18 |

---

## II. Canvas Specification

| Property | Value |
| -------- | ----- |
| **Format** | PPT 16:9 |
| **Dimensions** | 1280×720 |
| **viewBox** | `0 0 1280 720` |
| **Margins** | 左右 56，上 42，下 36 |
| **Content Area** | 1168×620；标题区约 88，主体区约 492，页脚区约 40 |

---

## III. Visual Theme

### Theme Style

- **Mode**: `instructional` — 先用游戏角色示例，再抽象出语法规则；概念按类 → 对象 → 可见性 → 继承 → 方法 → static 递进。
- **Visual style**: `sketch-notes` — 奶油纸张底、轻微手绘抖动、粉彩知识块、少量星点与波浪箭头。
- **Theme**: Light theme
- **Tone**: 亲切、清晰、具象、开发者友好；保持代码的严谨，不做儿童化夸张。

### Color Scheme

| Role | HEX | Purpose |
| ---- | --- | ------- |
| **Background** | `#FFFDF6` | 温暖纸张背景 |
| **Secondary bg** | `#F4F0E8` | 示例区与代码区底色 |
| **Primary** | `#2457A6` | 标题、主线、结构节点 |
| **Accent** | `#F2A93B` | 关键语法、提示箭头、重点词 |
| **Secondary accent** | `#55A68B` | 次级知识块与正确用法 |
| **Body text** | `#1F2937` | 正文与代码说明 |
| **Secondary text** | `#596273` | 注释、补充说明 |
| **Tertiary text** | `#8A8F98` | 页码与弱提示 |
| **Border/divider** | `#D8D2C5` | 手绘边框与分隔线 |
| **Ink** | `#1A1A1A` | 手绘线条与图像墨线 |
| **Surface** | `#FFF9E9` | 轻量强调面 |
| **Grid** | `#E9E3D8` | 代码本辅助线与淡网格 |
| **Success** | `#2F8F6A` | 推荐做法 |
| **Warning** | `#D85C45` | 风险与谨慎使用 |

### Image Strategy

- **Image Usage**: `none` — 用户于 2026-07-18 明确要求忽略缺失图片并直接导出。
- **Fallback Direction**: 所有原图片槽位改为原生 SVG 路径与 `tabler-outline` 图标构成的手绘概念插图；最终 PPTX 不包含外部图片依赖。

### Gradient Scheme

- 不使用装饰性渐变；仅封面在图像与标题交界处使用单向透明度 scrim，颜色仍取自锁定色。

---

## IV. Typography System

### Font Plan

**Typography direction**: 友好手写感标题 × 清晰人文黑体正文 × 等宽代码。

| Role | Chinese | English | Fallback tail |
| ---- | ------- | ------- | ------------- |
| **Title** | `KaiTi` | `Trebuchet MS` | `serif` |
| **Body** | `Microsoft YaHei` | `Arial` | `sans-serif` |
| **Emphasis** | `KaiTi` | `Trebuchet MS` | `serif` |
| **Code** | — | `Consolas`, `Courier New` | `monospace` |

**Per-role font stacks**:

- Title: `KaiTi, "Trebuchet MS", serif`
- Body: `"Microsoft YaHei", Arial, sans-serif`
- Emphasis: `KaiTi, "Trebuchet MS", serif`
- Code: `Consolas, "Courier New", monospace`

### Font Size Hierarchy

**Baseline (unitless px)**: Body font size = 32.

| Role | Size | Use |
| ---- | ---: | --- |
| Cover title | 92 | 封面主标题 |
| Page title | 56 | 每页标题 |
| Subtitle | 42 | 封面副标题、章节提示 |
| Lead | 36 | 每页核心句 |
| Subheading | 36 | 内容块标签 |
| Body | 32 | 讲授正文 |
| Code | 24 | C# 代码块 |
| Annotation | 24 | 图注与语法标注 |
| Footnote / page number | 18 | 页脚与页码 |

---

## V. Layout Principles

### Page Structure

- **Header area**: 高约 88；页标题左对齐，右上可放小型概念标签。
- **Content area**: 高约 492；优先单一主图解或一个示例 + 一个原则，不堆叠无关卡片。
- **Footer area**: 高约 40；左侧课程短标识，右侧自动页码。

### Layout Pattern Library

| Pattern | Suitable Scenarios |
| ------- | ----------------- |
| **Negative-space-driven** | 封面、类的核心定义、继承主关系 |
| **Asymmetric split (3:7 / 2:8)** | 代码示例 + 解释、对象对比 |
| **Three-column parallel blocks** | `public` / `private` / `protected` |
| **Top-down hierarchy** | 父类与子类关系 |
| **Module composition** | 方法签名与方法体拆解 |
| **Table + handwritten callout** | 方法重载的调用匹配 |
| **Hero claim + restrained two-zone comparison** | `static` 适用与谨慎使用 |

### Spacing Specification

| Element | Current Project |
| ------- | --------------- |
| Safe margin from canvas edge | 56 |
| Content block gap | 32 |
| Icon-text gap | 12 |
| Card gap | 24 |
| Card padding | 24 |
| Card border radius | 14（配合轻微手绘 path 抖动） |

- 非卡片页依靠 40–72 的留白建立节奏。
- 代码块行高约 1.45×；讲授正文约 1.45–1.55×。
- 阴影禁用；纸张层次仅用底色、边框与轻微错位填色表达。

---

## VI. Icon Usage Specification

### Source

- **Built-in icon library**: `tabler-outline`
- **Stroke width**: 2
- **Usage method**: `<use data-icon="tabler-outline/icon-name" .../>`

### Recommended Icon List

| Purpose | Icon Path | Page |
| ------- | --------- | ---- |
| 类 / 类型 | `tabler-outline/cube` | P02–P03 |
| 多个对象 | `tabler-outline/box-multiple` | P04 |
| 私有访问 | `tabler-outline/lock` | P05 |
| 受保护访问 | `tabler-outline/shield` | P05 |
| 继承层次 | `tabler-outline/hierarchy-3` | P06 |
| 方法 / 代码块 | `tabler-outline/braces` | P07–P08 |
| 提前返回 | `tabler-outline/arrow-back-up` | P08 |
| 方法重载 | `tabler-outline/copy` | P09 |
| 类型级成员 | `tabler-outline/world-code` | P10 |
| 快速工具方法 | `tabler-outline/bolt` | P10 |

---

## VII. Visualization Reference List

Catalog read: 76 templates

| Page | Template | Path | Summary-quote (verbatim from `charts_index.json`) | Usage |
| ---- | -------- | ---- | ------------------------------------------------- | ----- |
| P02 | agenda_list | `templates/charts/agenda_list.svg` | "Pick for table of contents, meeting agendas, or presentation roadmap — numbered items + brief description + duration / owner per row. Skip for substantive content lists (use vertical_list) or single-page section dividers (use a cover layout)." | 五个概念的学习路线图 |
| P03 | labeled_card | `templates/charts/labeled_card.svg` | "Pick for 3-4 parallel aspects of one subject with per-aspect titles + short body (self-introduction, four-pillar overview, capability quadrant). Skip for plain feature lists (use icon_grid), sequential steps (use numbered_steps), or strategic quadrants (use quadrant_text_bullets / matrix_2x2)." | 类、字段、方法三层概念拆解 |
| P04 | no-template-match | — | — | `player` 与 `enemy` 是两个并列实例，但既非产品比较也非数值镜像；采用自定义双实例代码舞台 |
| P05 | labeled_card | `templates/charts/labeled_card.svg` | "Pick for 3-4 parallel aspects of one subject with per-aspect titles + short body (self-introduction, four-pillar overview, capability quadrant). Skip for plain feature lists (use icon_grid), sequential steps (use numbered_steps), or strategic quadrants (use quadrant_text_bullets / matrix_2x2)." | 三种访问修饰符的平行说明 |
| P06 | top_down_tree | `templates/charts/top_down_tree.svg` | "Pick for hierarchical top-down tree 2-4 levels deep with parent→children reporting/decomposition lines — org charts (CEO → VPs → Directors), OKR cascades (Objective → Key Results → Initiatives), WBS decomposition. Skip for non-hierarchical brainstorm (use mind_map) or flat team showcase (use team_roster)." | `Character` 到 `Player` 的父子继承关系 |
| P07 | module_composition | `templates/charts/module_composition.svg` | "Pick for one parent container wrapping 3-N child module cards, each = title + 2-3 bullets — fits 'Feature X contains 3 parts, each with its own description'. Skip if source has only labels without descriptions (use numbered_steps or icon_grid)." | 一个方法拆成访问修饰符、返回类型、方法名、参数与方法体 |
| P08 | no-template-match | — | — | `void`、返回值、参数校验和提前 `return` 需要代码与解释的自定义上下分区 |
| P09 | basic_table | `templates/charts/basic_table.svg` | "Pick for plain tabular text/number grid, 3-8 columns. Skip if cells need visual bars (use consulting_table) or qualitative scores (use harvey_balls_table)." | 调用形式、参数列表、匹配版本、输出结果 |
| P10 | pros_cons_chart | `templates/charts/pros_cons_chart.svg` | "Pick for bilateral pros/cons list, 2-5 items per side. Skip for full feature comparison (use comparison_table) or numeric A/B mirror data (use butterfly_chart)." | `static` 的适用场景与谨慎使用边界 |

**Runners-up considered**:

- `icon_grid` | rejected for P03/P05: 图标网格会弱化类内层次与三种访问边界的解释深度。
- `numbered_steps` | rejected for P07: 方法组成部分是并列结构，不是按时间执行的步骤。
- `process_flow` | rejected for P06: 继承表达父子层次，不是线性业务流程。

---

## VIII. Image Resource List

No external image resources. Former AI image slots are resolved as native SVG vector illustrations by explicit user override.

---

## IX. Content Outline

### Part 1: 从类到对象

#### Slide 01 - Cover

- **Cover impact**: 用“角色说明书展开后变成玩家与史莱姆两个独立对象”作为具体钩子；采用全幅手绘主视觉 + 左侧浮动标题，直观建立“类是说明书，对象是实例”。
- **Layout**: 图像全幅；左侧 46% 为安静纸张与文字，右侧为说明书 → 对象的视觉转化。
- **Title**: C# 语法基础
- **Subtitle**: 类、继承、方法、`static` 与参数
- **Info**: 从游戏角色示例建立面向对象思维

#### Slide 02 - 先看全图：五个概念怎样连起来

- **Layout**: 横向学习路线；每个节点用编号、图标和一句说明，末端落到“写出可理解、可复用的代码”。
- **Title**: 先看全图：五个概念怎样连起来
- **Core message**: 类定义对象，对象通过方法行动，继承复用共同能力，`static` 则把成员提升到类型层级。
- **Visualization**: agenda_list
- **Content**:
  - 01 类：把数据和行为组织在一起。
  - 02 对象：按照类创建具体实例，每个实例保存独立数据。
  - 03 访问与继承：控制谁能访问，并从通用类型得到具体类型。
  - 04 方法与参数：给行为起名字，把输入交给行为处理。
  - 05 `static`：成员属于类型，而不是某个对象。

#### Slide 03 - 类就是“对象说明书”

- **Layout**: 中央手绘 `Character` 轮廓；下方两个模块分别承载字段和方法，右下放小型立方体点缀。
- **Title**: 类就是“对象说明书”
- **Core message**: 一个类同时描述对象拥有什么数据，以及对象能够执行什么行为。
- **Visualization**: labeled_card
- **Content**:
  - 类：`Character` 是角色对象的统一说明书。
  - 字段：`Name` 与 `Health` 保存每个角色自己的数据。
  - 方法：`TakeDamage()` 改变生命值，`PrintStatus()` 输出当前状态。
  - 代码示例完整保留 `public class Character` 的字段与两个方法。

#### Slide 04 - 同一个类，可以创建互不影响的对象

- **Layout**: 左侧代码逐行创建 `player` 与 `enemy`；右侧两个角色状态板并列，受伤箭头只落到 `enemy`。
- **Title**: 同一个类，可以创建互不影响的对象
- **Core message**: `player` 和 `enemy` 都属于 `Character`，但它们保存的是两套相互独立的数据。
- **Content**:
  - `player.Name = "小明"`，`player.Health = 100`。
  - `enemy.Name = "史莱姆"`，`enemy.Health = 30`。
  - 调用 `enemy.TakeDamage(10)` 后，变化只发生在 `enemy` 的生命值。

#### Slide 05 - 访问修饰符决定“谁能碰这些成员”

- **Layout**: 三个平行手绘知识块；`public` 开放、`private` 上锁、`protected` 连接子类。
- **Title**: 访问修饰符决定“谁能碰这些成员”
- **Core message**: 访问修饰符不是装饰，而是类对外暴露与内部保护的边界。
- **Visualization**: labeled_card
- **Content**:
  - `public`：任何能获得对象的代码都可访问；常用于对外提供的方法。
  - `private`：只有当前类内部可访问；用于保护内部数据。
  - `protected`：当前类和它的子类可访问；为继承留下扩展点。
  - 推荐：把数据设为 `private`，再通过方法控制数据如何改变。

### Part 2: 复用与行为

#### Slide 06 - 继承表达“是一种”关系

- **Layout**: 上方 `Character` 父类，向下连接 `Player` 子类；共享成员放在连接主干，子类独有方法放在分支末端。
- **Title**: 继承表达“是一种”关系
- **Core message**: 把名字、生命值和受伤行为放在父类，子类即可复用共同能力并增加自己的行为。
- **Visualization**: top_down_tree
- **Content**:
  - `public class Player : Character` 中的冒号表示继承。
  - `Character` 是父类或基类；`Player` 是子类或派生类。
  - `Player` 自动拥有父类中允许它访问的成员。
  - `Player` 还能增加 `UseItem()` 等独有方法。
  - 构造器通过 `: base(health)` 把初始生命值交给父类。

#### Slide 07 - 一个方法由哪些部分组成

- **Layout**: 以 `public int Add(int a, int b)` 为父容器；五个标注节点分别指向访问修饰符、返回类型、方法名、参数列表和方法体。
- **Title**: 一个方法由哪些部分组成
- **Core message**: 方法把一次行为的名称、输入、处理过程和输出组合成可重复调用的单元。
- **Visualization**: module_composition
- **Content**:
  - 访问修饰符：决定谁能调用。
  - 返回类型：说明结果是什么类型；不返回结果时写 `void`。
  - 方法名：给行为一个可读的名字。
  - 参数列表：`int a, int b` 是调用者交给方法的输入。
  - 方法体：执行计算；`return result` 把结果交还调用者。

#### Slide 08 - `void`、返回值与提前结束

- **Layout**: 上方左右对比 `void` 与 `bool` 返回值；下方完整展示 `Heal(int amount)`，用回转箭头标出 `amount <= 0` 时的提前 `return`。
- **Title**: `void`、返回值与提前结束
- **Core message**: 返回类型决定方法是否交回结果，而参数校验可以用 `return` 提前结束无效操作。
- **Content**:
  - `void`：只执行行为，不把结果交给调用者。
  - `bool IsAlive()`：通过 `return health > 0` 返回布尔结果。
  - `Heal(int amount)`：`amount` 是输入参数。
  - 当 `amount <= 0` 时直接 `return`，避免继续修改生命值。

#### Slide 09 - 方法重载：同名，但参数列表不同

- **Layout**: 左侧两段 `Attack` 代码；右侧表格把调用形式映射到对应版本与输出。
- **Title**: 方法重载：同名，但参数列表不同
- **Core message**: 编译器根据调用时提供的参数，选择同名方法中参数列表匹配的版本。
- **Visualization**: basic_table
- **Content**:
  - `Attack()`：无参数，输出“普通攻击”。
  - `Attack(int damage)`：接收伤害参数，输出“造成 {damage} 点伤害”。
  - 同一个类中可以有同名方法，前提是参数列表不同。

#### Slide 10 - `static` 属于类型，不是调用捷径

- **Closing impact**: 让观众带走“对象状态属于实例，真正共享的能力或数据才属于类型”；以左侧超大 `static` 关键字 + 右侧适合/谨慎两区 + 共享类型地球点缀落下最后判断。
- **Layout**: 大标题与类型级共享视觉为主；两块精简列表只保留决策边界，不做满页卡片网格。
- **Title**: `static` 属于类型，不是调用捷径
- **Core message**: 只有无状态工具或确实全局共享的类型级信息适合静态化，不能为了调用方便把大量逻辑都设为 `static`。
- **Visualization**: pros_cons_chart
- **Content**:
  - 适合：无状态的计算工具；全局共享且只应存在一份的数据；创建数量统计等类型级信息。
  - 谨慎：场景切换后残留的可变状态；任何对象都能随意改写的全局数据；为了“调用方便”静态化大量逻辑。
  - Unity 示例：`Mathf.Clamp()` 是常用静态方法。

---

## X. Speaker Notes Requirements

- **Total duration**: 20–24 分钟，每页约 1.5–3 分钟。
- **Purpose**: instruct；先示例后规则，持续用 `Character` / `Player` / `enemy` 作为共同上下文。
- **Style**: 耐心、解释型、口语化；定义术语前先问一个学习者可能提出的问题。
- **Filename**: 与 SVG 文件名一致，如 `01_cover.md`。
- **Content**: 每页包含开场引导、代码讲解、常见误区、过渡句；拆分后的单页 notes 不保留 `#` 标题。
