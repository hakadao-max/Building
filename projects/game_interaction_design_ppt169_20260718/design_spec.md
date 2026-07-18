# Design Specification — 游戏交互设计：从概念到实践

## 1. 项目概述

- 来源：`05_游戏交互设计_从概念到实践.md`
- 输出：15 页、PPT 16:9、可编辑 DrawingML PPTX
- 受众：游戏设计、Unity 与交互原型初学者
- 模式：instructional
- 视觉风格：sketch-notes
- 图片策略：不依赖外部图片，案例全部用原生 SVG 概念图表达

## 2. 核心叙事

先建立“想法—操作—世界变化—新想法”的交互闭环，再总结吸引人的交互原则；随后用多个经典游戏拆解机制和实现，最后把规律转化为可执行的原型方法。

## 3. 画布与版式

- 画布：1280 × 720；安全边距左右 60px
- 标题区：标题基线 76px；正文从 150px 开始
- 统一页脚：课程短名 + 页码
- 案例页采用一页一个机制，视觉中心突出“玩家改变了什么”

## 4. 色彩系统

- 背景：`#FFFDF6`；次背景：`#F4F0E8`
- 主蓝：`#2457A6`；强调橙：`#F2A93B`；辅助绿：`#55A68B`
- 正文：`#1F2937`；辅文：`#596273`；边框：`#D8D2C5`
- 警示：`#D85C45`；成功：`#2F8F6A`
- 白：`#FFFFFF`；蓝浅：`#EAF0FA`；橙浅：`#FFF0D1`；绿浅：`#E8F5F0`；橙深：`#8C5A00`

## 5. 字体系统

- 标题：KaiTi，56pt；封面 92pt；副标题 42pt
- 正文：Microsoft YaHei，32pt
- 引导标题：36pt；属性、代码与注释：24pt
- 页脚与页码：18pt；代码：Consolas

## 6. 图标系统

- 图标库：tabler-outline；线宽：2
- 图标：`bulb, pointer, arrows-exchange, eye, camera, hierarchy-3, walk, dimensions, package, flame, bolt, snowflake, text-recognition, file-text, scale, hammer, checklist, ghost, box, circle, device-gamepad, route`

## 7. 页面结构

- P01 封面：让玩家真正想动手
- P02 定义：交互是一个驱动下一步想法的闭环
- P03 吸引力：可感知性与即时反馈
- P04 深度：多用途、稳定规则、改变局面与乱玩空间
- P05 教学曲线：发现、简单问题、组合、压力、反向利用
- P06 《超阈限空间》：透视缩放与合法放置
- P07 《看门狗》：控制权沿摄像机网络移动
- P08 《超级马力欧》：跳跃手感的隐藏宽容
- P09 《传送门》：位置、方向与速度的空间变换
- P10 《半衰期 2》：重力枪把环境变成武器库
- P11 《旷野之息》：属性、状态与统一事件形成系统组合
- P12 《Baba Is You》：玩家直接改写规则
- P13 普通动作变玩法：《Papers, Please》桌面与《死亡搁浅》走路
- P14 三个原创方向：材质转移、力量记忆、视线外行动
- P15 原型清单：先验证一条规则，再扩展组合与关卡

## 8. 可视化选择

- P02 `chevron_process`；P03 `vertical_pillars`；P04 `icon_grid`
- P05 `numbered_steps`；P06 `pipeline_with_stages`；P07 `top_down_tree`
- P08 `labeled_card`；P09 `pipeline_with_stages`；P10 `module_composition`
- P11 `layered_architecture`；P12 `top_down_tree`；P13 `vertical_pillars`
- P14 `icon_grid`；P15 `agenda_list`

## 9. 页面角色与密度

- P01 cover / anchor；P02–P05 content；P06–P12 content / dense
- P13–P14 content；P15 ending / anchor
- P02、P05、P15 为结构性锚点；案例页保留一个主机制和一个实现框架

## 10. 验收标准

- 15 个 SVG 与 15 份讲者备注一一对应
- 不使用外部图片、外部 CSS、`foreignObject`、动画、蒙版与渐变
- 所有颜色、字体、字号来自 `spec_lock.md`
- 最终 PPTX 可打开、页数 15、讲者备注 15

