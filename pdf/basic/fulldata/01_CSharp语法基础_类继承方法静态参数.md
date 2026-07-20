# C# 语法基础：类、继承、方法、静态与参数


## 类：把数据和行为组织在一起

### 类是什么

类可以理解为一种“对象说明书”。它描述：

- 这种对象拥有什么数据；
- 这种对象能够执行什么行为。

例如，游戏中的角色拥有名字和生命值，也可以受伤和显示状态。我们可以把这些内容写进一个 `Character` 类。

```csharp
public class Character
{
    // 字段：保存对象的数据
    public string Name;
    public int Health;

    // 方法：描述对象能够执行的行为
    public void TakeDamage(int damage)
    {
        Health = Health - damage;
    }

    public void PrintStatus()
    {
        Console.WriteLine($"{Name} 当前生命值：{Health}");
    }
}
```

### 创建对象

类是说明书，对象是按照说明书创建出来的具体实例。

```csharp
Character player = new Character();
player.Name = "小明";
player.Health = 100;

Character enemy = new Character();
enemy.Name = "史莱姆";
enemy.Health = 30;

enemy.TakeDamage(10);
enemy.PrintStatus();
```

`player` 和 `enemy` 都属于 `Character`，但它们保存的是两套相互独立的数据。



### 访问修饰符

初学阶段先掌握三个：

| 关键字 | 谁可以访问 | 常见用途 |
| --- | --- | --- |
| `public` | 任何能获得对象的代码 | 对外提供的方法 |
| `private` | 只有当前类内部 | 保护内部数据 |
| `protected` | 当前类和它的子类 | 为继承留下扩展点 |

更推荐把数据设为 `private`，再通过方法控制数据如何改变。


---

## 继承：从通用类型得到具体类型

### 继承表达“是一种”关系

玩家是一种角色，敌人也是一种角色。它们都需要名字、生命值和受伤行为，因此可以把共同内容放在父类中。

```csharp
public class Character
{
    protected int health;

    public Character(int health)
    {
        this.health = health;
    }

    public void TakeDamage(int damage)
    {
        health = Math.Max(health - damage, 0);
    }
}

public class Player : Character
{
    public Player(int health) : base(health)
    {
    }

    public void UseItem()
    {
        Console.WriteLine("玩家使用了道具");
    }
}
```

语法中的冒号表示继承：

```csharp
public class Player : Character
```

此时：

- `Player` 是子类或派生类；
- `Character` 是父类或基类；
- `Player` 自动拥有父类中允许它访问的成员；
- `Player` 还可以增加自己独有的方法。



---

## 方法：给行为起名字

### 方法的基本结构

```csharp
访问修饰符 返回类型 方法名(参数列表)
{
    方法体
}
```

例如：

```csharp
public int Add(int a, int b)
{
    int result = a + b;
    return result;
}
```

调用：

```csharp
int total = Add(3, 5);
Console.WriteLine(total); // 8
```

###  `void` 与返回值

不需要把结果交给调用者时，返回类型写 `void`。
需要返回结果时，写出结果类型并使用 `return`。

```csharp
public bool IsAlive()
{
    return health > 0;
}
```

`return` 也可以提前结束一个 `void` 方法：

```csharp
public void Heal(int amount)
{
    if (amount <= 0)
    {
        return;
    }

    health = health + amount;
}
```

### 方法重载

同一个类中可以有同名方法，只要参数列表不同。

```csharp
public void Attack()
{
    Console.WriteLine("普通攻击");
}

public void Attack(int damage)
{
    Console.WriteLine($"造成 {damage} 点伤害");
}

```

这叫方法重载。编译器会根据调用时提供的参数选择对应版本。

---

## `static`：属于类型，而不是某个对象



### 何时使用静态

适合：

- 无状态的计算工具；
- 全局共享且确实只应存在一份的数据；
- 创建数量统计等类型级信息。

谨慎使用：

- 会在场景切换后残留的可变状态；
- 任何对象都能随意改写的全局数据；
- 为了“调用方便”而把大量逻辑都设为静态。

在 Unity 中，`Mathf` 的常用方法就是静态方法，例如 `Mathf.Clamp()`。

---
