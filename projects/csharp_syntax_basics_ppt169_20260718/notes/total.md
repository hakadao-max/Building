# 01_cover

今天我们用游戏角色来理解 C# 的几个核心语法。先记住一个最重要的比喻：类像角色说明书，对象是按照说明书真正创建出来的角色。后面所有内容都会围绕 Character、Player 和 enemy 逐步展开。

---

# 02_roadmap

先看这条学习路线，避免把每个关键字当成孤立知识点。类组织数据与行为，对象保存各自的数据，访问修饰符与继承定义边界和复用方式，方法与参数负责执行行为。最后的 static 会把成员提升到类型层级，但它有明确的使用边界。

---

# 03_class_blueprint

如果把 Character 看成一张说明书，字段回答“角色拥有什么”，方法回答“角色能做什么”。Name 和 Health 会出现在每一个角色对象里，而 TakeDamage() 和 PrintStatus() 描述可执行的行为。常见误区是把类本身当成一个已经存在的角色；实际上还要用 new 创建对象。

---

# 04_independent_objects

player 和 enemy 都由 Character 创建，但它们保存的是两套独立数据。调用 enemy.TakeDamage(10) 时，只有 enemy 的 Health 从 30 变成 20，player 仍然保持 100。这个例子帮助我们区分“同一种类型”和“同一个对象”。

---

# 05_access_modifiers

访问修饰符不是为了让代码看起来完整，而是在定义类的边界。public 对外开放，private 只允许类内部访问，protected 还会把访问权留给子类。入门阶段可以优先把可变数据设为 private，再通过方法控制它如何变化。

---

# 06_inheritance

继承表达的是“Player 是一种 Character”，所以冒号后面写父类 Character。Player 自动获得父类中允许访问的字段和方法，同时还能增加 UseItem() 这样的独有行为。构造器中的 base(health) 表示把初始化生命值的工作继续交给父类处理。

---

# 07_method_anatomy

一条方法声明包含五个部分：访问修饰符、返回类型、方法名、参数列表和方法体。参数是调用者交给方法的输入，return 是方法交还给调用者的输出。把这些部分读顺之后，再长的方法声明也可以按同样顺序拆解。

---

# 08_return_and_early_exit

void 表示方法只执行行为，不交回计算结果；bool IsAlive() 则会返回一个真假值。Heal(int amount) 先检查参数，如果 amount 小于等于零，就用 return 提前结束。这样可以让无效输入停在入口处，避免继续修改对象状态。

---

# 09_method_overload

方法重载允许同一个类里出现多个同名方法，但它们的参数列表必须不同。调用 Attack() 时匹配无参数版本，调用 Attack(25) 时匹配接收 int damage 的版本。编译器关注的是参数数量、类型和顺序，而不是只看方法名。

---

# 10_static

static 成员属于类型本身，而不是某一个对象。Mathf.Clamp() 这样的无状态工具方法很适合 static，真正只应存在一份的类型级信息也可以考虑使用。不要为了少写 new 或调用方便，就把大量可变状态和业务逻辑静态化；最后始终用“它属于实例还是属于类型”来判断。
