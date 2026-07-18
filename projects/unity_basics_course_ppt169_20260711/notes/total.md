# 01_cover
今天我们用一个 Pong 原型串起 Unity 的两条基础能力：脚本让对象产生行为，UGUI 让玩家看到并控制这些行为。先看最终目标，再逐步拆解。

# 02_route
课程分两段。前半段建立对象、组件、生命周期、输入、碰撞和事件；后半段把这些状态连接到菜单、按钮、Slider 与 Toggle。最终检查点是一个可运行、可调参数、可返回菜单的 Pong。

# 03_scripting_section
先进入脚本部分。记住本单元的一句话：GameObject 本身只是容器，行为来自挂载的组件。

# 04_components
以 Ball 为例。Transform 决定位置，SpriteRenderer 负责显示，Rigidbody2D 参与物理，Collider2D 检测碰撞，BallController 编排规则。Prefab 则保存这组配置以便复用。

# 05_lifecycle
Awake 适合缓存组件，Start 适合初始化状态。Update 每帧读取输入，FixedUpdate 按固定时间步处理物理。请学生思考：把键盘输入放在 FixedUpdate 会有什么体验？

# 06_script_skeleton
先讲意图，再讲语法。using 导入 Unity API；类名与文件名一致；继承 MonoBehaviour 才能挂到对象上；生命周期方法由 Unity 调用。

# 07_input
变量保存状态，Input API 读取玩家意图。演示在 Inspector 中调整 speed，再用 W/S 控制 Paddle。强调 GetKey、GetKeyDown 和 GetAxisRaw 的差异。

# 08_paddle
移动公式只有三部分：方向、速度、时间。Time.deltaTime 抵消帧率差异，Mathf.Clamp 防止 Paddle 离开场地。逐行运行并观察 pos.y 的变化。

# 09_functions_collections
函数把重复计算封装成一个名字。数组适合固定数量，List 适合动态增减，Dictionary 适合用名字查值。集合通常配合 for 或 foreach。

# 10_physics_events
物理层先产生碰撞或触发，事件层再通知计分系统和 UI。这样 Ball 不需要知道分数文本放在哪里，脚本之间更容易替换和测试。

# 11_debug
调试第一原则：先处理 Console 的第一条错误，因为后续错误可能只是连锁反应。再用 Pause、Step、断点和 Gizmos 验证假设。最后把四个 Pong 挑战留作练习。

# 12_ugui_section
进入 UGUI 部分。我们的目标不是把控件摆出来，而是让界面准确反映并改变游戏状态。

# 13_canvas_hierarchy
Canvas 是根，Panel 负责语义分组，具体控件负责交互。Canvas Scaler 解决分辨率适配，EventSystem 负责事件分发。先把层级整理好，再写交互。

# 14_buttons
UIManager 集中持有面板和按钮引用。Awake 中绑定监听，方法中切换 SetActive。提醒：Application.Quit 在编辑器播放模式下不会真正关闭编辑器。

# 15_binding
Slider 的 value 是 float，可以直接传给 BallController；Toggle 的值是 bool，可以直接设置全屏。界面值改变后，要让玩家立即看到结果或数值反馈。

# 16_layout_theme
手工对齐在一个分辨率上看起来正确，换屏幕后往往失效。Layout Group、Anchor 和 Canvas Scaler 共同负责适配；Button States、TMP 与 Prefab 负责一致性。

# 17_godot_unity
如果学生来自 Godot，不要从零记忆。先做概念映射：Node 对应对象加组件，Signal 对应事件，Container 对应 Layout Group。映射建立后，再学习 Unity 的具体 API。

# 18_checklist
最后用六项清单验收原型。每完成一项就实际运行验证，不只看代码。课程结束后的下一步，是在 Unity 中亲手把这些模块连接起来。
