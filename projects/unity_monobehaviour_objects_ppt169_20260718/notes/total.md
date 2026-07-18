# P01_封面

这一单元关注 Unity 脚本真正进入场景后的工作方式。我们会把 MonoBehaviour、生命周期、Transform、GameObject 与 CharacterController 串成一条可运行的角色控制链。

---

# P02_学习路线

学习路线从脚本身份开始，经过生命周期和常用对象，最后落到角色移动与摄像机跟随。每个知识点都围绕两个问题：当前代码操作哪个对象，它应该在什么时机执行。

---

# P03_MonoBehaviour是组件

GameObject 本身是组件容器，Transform 是每个对象都具备的基础能力。自定义 MonoBehaviour 只有挂载到对象上才会运行，因此文件名、类名和对象上的组件关系必须保持清晰。

---

# P04_生命周期主序列

生命周期函数由 Unity 在固定时机调用，从 Awake、OnEnable、Start 进入运行循环，再到 OnDisable 与 OnDestroy。对象重新启用时 OnEnable 会再次执行，而同一实例的 Start 通常只执行一次。

---

# P05_生命周期函数选择

选择 生命周期函数时，先判断任务依赖谁、发生几次、持续多久。自身组件引用放在 Awake，事件订阅与取消成对放在 OnEnable 和 OnDisable，跨对象启动则使用明确流程而不是猜测 Awake 顺序。

---

# P06_三个更新循环

Update 处理输入和普通逻辑，FixedUpdate 面向 Rigidbody 的物理时间步，LateUpdate 适合摄像机等收尾行为。连续变化需要使用 Time.deltaTime，一次性触发的创建或切换则不需要。

---

# P07_Transform空间与层级

Transform 同时保存世界与局部空间数据，也保存父子层级。移动和旋转时要分清 Space.Self 与 Space.World，坐标点和方向向量也必须使用各自对应的转换函数。

---

# P08_GameObject与组件引用

gameObject 用于访问对象身份和激活状态，GetComponent 系列用于获取对象能力。稳定引用应优先通过 Inspector、创建时传入或初始化缓存建立，避免在每帧中反复按名称查找。

---

# P09_对象创建交互与销毁

对象的完整寿命包含引用、创建、交互和销毁四段。Instantiate 与 AddComponent 都可以动态增加内容，但固定配置更适合提前放进 Prefab；销毁前后还要注意引用失效和清理逻辑。

---

# P10_CharacterController心智模型

CharacterController 提供受碰撞约束的脚本控制移动，但 Move 不会自动施加重力。理解胶囊尺寸、坡度、台阶、贴地状态以及 Move 与 SimpleMove 的参数差异，是稳定移动的前提。

---

# P11_完整移动算法

一个完整移动帧要依次读取输入、限制斜向速度、平滑转向、处理贴地和跳跃、积分重力，再合成速度调用 Move。Move 返回的 CollisionFlags 还能帮助修正撞到顶部后的竖直速度。

---

# P12_Controller与Rigidbody选择

CharacterController 更强调玩家控制的确定性，Rigidbody 更强调动力学响应。先决定玩法需要可控性还是物理交互，不要让两套系统在同一个角色根节点上争夺移动权。

---

# P13_角色与摄像机场景闭环

最小场景闭环是角色在 Update 中完成移动，摄像机在 LateUpdate 中读取角色的新位置并跟随。通过 Inspector 连接 target，再逐项验证移动、跳跃、碰撞和镜头稳定性，就完成了从脚本到场景运行的全过程。
