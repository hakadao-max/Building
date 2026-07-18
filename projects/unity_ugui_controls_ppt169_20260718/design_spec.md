# Design Specification — Unity UI 基础：UGUI 控件

## 1. 项目概述

- 来源：`04_UnityUI基础_UGUI控件.md`
- 输出：13 页、PPT 16:9、可编辑 DrawingML PPTX
- 受众：Unity 初学者与课堂学习者
- 模式：instructional
- 视觉风格：sketch-notes
- 图片策略：不依赖外部图片，仅使用 SVG 图形与统一线性图标

## 2. 核心叙事

从 Canvas 与 EventSystem 的职责出发，依次理解布局适配、输入事件和 Button、Image、RawImage、Toggle、Dropdown 等常用控件，最后组装一个可运行的设置面板。

## 3. 画布与版式

- 画布：1280 × 720
- 安全边距：左右 60px；标题基线约 76px；正文区从 150px 开始
- 内容页统一页脚：课程短名 + 页码
- 节奏：封面与总结为锚点页；架构与流程为呼吸页；控件属性与排错为密集页

## 4. 色彩系统

- 背景：`#FFFDF6`
- 次背景：`#F4F0E8`
- 主蓝：`#2457A6`
- 强调橙：`#F2A93B`
- 辅助绿：`#55A68B`
- 正文：`#1F2937`
- 辅文：`#596273`
- 边框：`#D8D2C5`
- 警示：`#D85C45`
- 成功：`#2F8F6A`
- 白：`#FFFFFF`
- 蓝浅色：`#EAF0FA`
- 橙浅色：`#FFF0D1`
- 绿浅色：`#E8F5F0`
- 橙深色：`#8C5A00`

## 5. 字体系统

- 标题：KaiTi，56pt；封面 92pt；副标题 42pt
- 正文：Microsoft YaHei，32pt
- 引导标题：36pt
- 属性、代码与注释：24pt
- 页脚与页码：18pt
- 代码：Consolas

## 6. 图标系统

- 图标库：tabler-outline
- 线宽：2
- 图标：`layout-dashboard, hierarchy-3, transform, world, pointer, mouse, keyboard, click, box, palette, device-tv, toggle-left, list, settings, checklist, camera`
- 同一页只使用这一套图标，颜色继承页面语义色

## 7. 页面结构

- P01 封面：Canvas 把控件组织成界面，EventSystem 把输入送到控件
- P02 UGUI 基本结构：Canvas / EventSystem / Controls / Behaviour 四层关系
- P03 Canvas 两种模式：Overlay 与 World Space 对比
- P04 RectTransform：Anchor、Pivot、Anchored Position 与 Canvas Scaler
- P05 EventSystem 输入管线：输入模块 → EventSystem → Raycaster → 控件
- P06 UI 看得见但点不到：九步排错清单
- P07 Button：状态、Transition、事件连接与监听生命周期
- P08 Image：Simple、Sliced、Tiled、Filled 四种类型
- P09 RawImage：Camera → RenderTexture → RawImage 与 Image 对比
- P10 Toggle：布尔状态、ToggleGroup 与无通知初始化
- P11 Dropdown：结构、索引映射与 TMP_Dropdown 工作流
- P12 综合实践：设置面板层级与控件职责
- P13 收尾：适配、交互、状态、事件、性能五项检查

## 8. 可视化选择

- P02 `layered_architecture`：四层 UI 职责
- P03 `vertical_pillars`：Overlay / World Space 双柱比较
- P04 `labeled_card`：锚点与轴心示意
- P05 `pipeline_with_stages`：输入事件管线
- P06 `vertical_list`：顺序排错
- P07 `module_composition`：按钮状态与事件模块
- P08 `icon_grid`：四种 Image Type
- P09 `pipeline_with_stages`：纹理输出管线
- P10 `vertical_pillars`：单 Toggle 与 ToggleGroup
- P11 `numbered_steps`：Dropdown 初始化与响应流程
- P12 `top_down_tree`：设置面板层级
- P13 `agenda_list`：五项交付检查

## 9. 页面角色与密度

- P01：cover / anchor
- P02：toc / breathing
- P03：content / dense
- P04：content / dense
- P05：content / breathing
- P06：content / dense
- P07：content / dense
- P08：content / dense
- P09：content / breathing
- P10：content / dense
- P11：content / dense
- P12：content / dense
- P13：ending / anchor

## 10. 验收标准

- 13 个 SVG 与 13 份讲者备注一一对应
- 不使用外部图片、外部 CSS、`foreignObject`、动画或蒙版
- 页面可读、无文字越界，配色与字号均来自锁定规范
- 最终 PPTX 可打开、页数为 13、讲者备注为 13

