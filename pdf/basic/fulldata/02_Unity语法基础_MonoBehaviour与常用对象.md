# Unity 语法基础

---

## Unity 脚本到底是什么

Unity 场景中的基本对象是 `GameObject`。一个空的 `GameObject` 只有 `Transform`，其他能力都来自组件：

- `MeshRenderer` 负责显示网格；
- `Collider` 负责碰撞形状；
- `AudioSource` 负责播放声音；
- 自己编写的 `MonoBehaviour` 脚本也会成为组件。

```csharp
using UnityEngine;

public class Rotator : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0f, 60f * Time.deltaTime, 0f);
    }
}
```

把 `Rotator.cs` 挂到物体上之后，它才会参与场景运行。

### 文件名与类名

对于可挂载的 `MonoBehaviour` 脚本，文件名应与类名完全一致：

- 文件：`Rotator.cs`
- 类：`public class Rotator : MonoBehaviour`

名称不一致时，Unity 通常无法把脚本正常挂到物体上。

### 三个常用入口

在 `MonoBehaviour` 中可以直接使用：

```csharp
transform        // 当前物体的 Transform
gameObject       // 当前脚本所挂载的 GameObject
GetComponent<T>() // 获取当前物体上的某种组件
```

注意大小写：

- `GameObject` 是类型名；
- `gameObject` 是当前脚本所在的具体对象；
- `Transform` 是类型名；
- `transform` 是当前对象的具体 `Transform` 组件。

---

## MonoBehaviour 生命周期

生命周期函数由 Unity 在特定时机调用。它们不是普通意义上由我们随意调用的方法，而是引擎约定好的事件函数。

## 常用执行顺序

一个典型脚本从出现到销毁，大致经历：

```text
Awake
  ↓
OnEnable
  ↓
Start
  ↓
Update / FixedUpdate / LateUpdate（运行中反复执行）
  ↓
OnDisable
  ↓
OnDestroy
```

如果对象之后重新启用，会再次执行 `OnEnable`，但同一个脚本实例的 `Start` 通常只执行一次。

### `Awake()`：建立自身引用

`Awake` 在脚本实例加载时调用，适合：

- 获取当前对象上的必要组件；
- 初始化完全属于自身的数据；
- 建立不依赖其他对象启动顺序的引用。

```csharp
private CharacterController controller;

void Awake()
{
    controller = GetComponent<CharacterController>();
}
```

不要默认不同对象的 `Awake` 谁先执行。若 A 的初始化依赖 B 已经完成，应该通过明确的启动流程解决，而不是碰运气依赖顺序。

### `OnEnable()`：每次启用时开始工作

适合：

- 订阅事件；
- 启动每次启用都需要的行为；
- 重置临时状态。

```csharp
void OnEnable()
{
    Debug.Log("组件已启用");
}
```

如果在 `OnEnable` 订阅事件，应在 `OnDisable` 取消订阅。

### `Start()`：开始游戏逻辑

`Start` 在脚本第一次启用、第一次帧更新之前调用，适合：

- 使用其他对象在 `Awake` 中建立好的数据；
- 设置初始游戏状态；
- 输出启动信息。

```csharp
void Start()
{
    Debug.Log("角色准备完成");
}
```

### `Update()`：每帧逻辑

`Update` 每个渲染帧调用一次，适合：

- 读取玩家输入；
- 更新普通游戏逻辑；
- 执行不依赖固定物理时间步的移动。

```csharp
void Update()
{
    transform.Rotate(0f, 90f * Time.deltaTime, 0f);
}
```

帧率会变化，因此连续变化通常要乘 `Time.deltaTime`。

```csharp
移动距离 = 每秒速度 × 本帧经过的秒数
```

不要给“只触发一次”的操作乘 `Time.deltaTime`。例如按下按键时创建一个物体，不属于连续变化。

### `FixedUpdate()`：固定时间步的物理逻辑

`FixedUpdate` 按固定物理时间步调用，不保证每个渲染帧恰好执行一次。它适合对 `Rigidbody` 施加力或执行物理移动。

```csharp
private Rigidbody body;

void Awake()
{
    body = GetComponent<Rigidbody>();
}

void FixedUpdate()
{
    body.AddForce(Vector3.forward * 10f);
}
```

原则：

- 在 `Update` 读取瞬时输入；
- 对 `Rigidbody` 的物理操作放在 `FixedUpdate`；
- `CharacterController` 不是动力学刚体，通常在 `Update` 中调用 `Move`。

### `LateUpdate()`：其他更新结束后再处理

`LateUpdate` 每帧在普通 `Update` 之后调用，常用于跟随摄像机。

```csharp
[SerializeField] private Transform target;
[SerializeField] private Vector3 offset;

void LateUpdate()
{
    if (target == null)
    {
        return;
    }

    transform.position = target.position + offset;
}
```

角色先移动，摄像机后跟随，画面更容易保持稳定。

### `OnDisable()` 与 `OnDestroy()`

`OnDisable` 在组件或对象被禁用时调用。`OnDestroy` 在脚本实例被销毁时调用。

```csharp
void OnDisable()
{
    Debug.Log("组件停止工作");
}

void OnDestroy()
{
    Debug.Log("组件被销毁");
}
```

常见用途：

- `OnDisable`：取消事件订阅、停止临时行为；
- `OnDestroy`：释放与该实例同寿命的资源。

### 生命周期选择表

| 任务 | 推荐位置 |
| --- | --- |
| 获取自身组件 | `Awake` |
| 订阅输入或事件 | `OnEnable` |
| 取消事件订阅 | `OnDisable` |
| 依赖其他对象初始化后的启动逻辑 | `Start` |
| 读取按键、更新普通逻辑 | `Update` |
| 对 Rigidbody 施加力 | `FixedUpdate` |
| 摄像机跟随 | `LateUpdate` |
| 实例销毁前清理 | `OnDestroy` |

---

## Transform：位置、旋转、缩放与层级

每个 `GameObject` 都有一个 `Transform`。它决定物体在场景中的空间状态，也保存父子层级。

### 世界坐标与局部坐标

```csharp
transform.position       // 世界坐标位置
transform.localPosition  // 相对父物体的局部位置

transform.rotation       // 世界旋转，Quaternion
transform.localRotation  // 相对父物体的局部旋转

transform.localScale     // 相对父物体的局部缩放
```

如果对象没有父物体，`position` 和 `localPosition` 通常相同。存在旋转或缩放的父物体时，它们可能明显不同。

### 修改位置

直接设置：

```csharp
transform.position = new Vector3(0f, 1f, 5f);
```

按每秒速度移动：

```csharp
[SerializeField] private float speed = 3f;

void Update()
{
    transform.position += Vector3.forward * speed * Time.deltaTime;
}
```

使用 `Translate`：

```csharp
transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
```

- `Space.Self`：沿物体自己的方向移动；
- `Space.World`：沿世界坐标轴移动。

### 方向向量

```csharp
transform.forward // 物体正前方
transform.right   // 物体右方
transform.up      // 物体上方
```

例如：

```csharp
Vector3 moveDirection = transform.forward * verticalInput
                      + transform.right * horizontalInput;
```

### 旋转

```csharp
transform.Rotate(0f, 90f * Time.deltaTime, 0f);
```

让物体朝向目标：

```csharp
transform.LookAt(target);
```

如果只希望水平转向，应先去掉高度差：

```csharp
Vector3 direction = target.position - transform.position;
direction.y = 0f;

if (direction.sqrMagnitude > 0.001f)
{
    transform.rotation = Quaternion.LookRotation(direction);
}
```

不建议每帧通过 `eulerAngles` 累加复杂旋转。一般优先使用 `Quaternion`、`Rotate`、`LookRotation` 等接口。

### 父子关系

```csharp
Transform parent = transform.parent;
int childCount = transform.childCount;
Transform firstChild = transform.GetChild(0);
```

设置父物体：

```csharp
transform.SetParent(newParent, true);
```

第二个参数为 `true` 时尽量保持世界位置、旋转和缩放；为 `false` 时保持局部变换。

### 坐标转换

```csharp
Vector3 worldPoint = transform.TransformPoint(localPoint);
Vector3 localPoint = transform.InverseTransformPoint(worldPoint);

Vector3 worldDirection = transform.TransformDirection(localDirection);
Vector3 localDirection = transform.InverseTransformDirection(worldDirection);
```

点包含位置信息，方向不包含位置。因此坐标点和方向向量不要混用转换函数。

---

## GameObject：场景对象的容器

### 当前对象与其他对象

```csharp
Debug.Log(gameObject.name);
Debug.Log(gameObject.tag);
Debug.Log(gameObject.layer);
```

启用和禁用：

```csharp
gameObject.SetActive(false);
```

- `activeSelf`：对象自身是否被设置为激活；
- `activeInHierarchy`：考虑父对象之后，它在场景层级中是否真正激活。

父物体被禁用时，子物体即使 `activeSelf == true`，`activeInHierarchy` 也会是 `false`。

### 获取组件

```csharp
CharacterController controller = GetComponent<CharacterController>();
```

更安全的尝试方式：

```csharp
if (TryGetComponent(out AudioSource audioSource))
{
    audioSource.Play();
}
```

其他常见查找：

```csharp
GetComponentInChildren<Animator>();
GetComponentInParent<Rigidbody>();
```

建议在 `Awake` 中获取并缓存需要反复使用的组件，而不是每帧重复查找。

### 添加组件

```csharp
AudioSource source = gameObject.AddComponent<AudioSource>();
```

运行时添加组件是允许的，但固定配置通常更适合提前在 Inspector 或 Prefab 中完成。

### 创建和销毁对象

从 Prefab 创建：

```csharp
[SerializeField] private GameObject projectilePrefab;
[SerializeField] private Transform firePoint;

void Fire()
{
    Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
}
```

销毁对象：

```csharp
Destroy(gameObject);
Destroy(gameObject, 2f); // 2 秒后销毁
```

### 标签判断

```csharp
if (other.gameObject.CompareTag("Player"))
{
    Debug.Log("检测到玩家");
}
```

判断标签时优先使用 `CompareTag`。

### 查找对象的注意事项

可以使用：

```csharp
GameObject target = GameObject.Find("Target");
GameObject player = GameObject.FindWithTag("Player");
```

但名称查找容易因重命名失效，也不适合每帧执行。更稳妥的方式是：

- 在 Inspector 中通过 `[SerializeField]` 引用；
- 在创建对象时传递引用；
- 只在初始化阶段查找一次并缓存。

---

## CharacterController：非刚体式角色移动

### 它适合什么

`CharacterController` 适合需要直接、可控移动的角色，例如：

- 第一人称或第三人称玩家；
- 希望角色能沿斜坡移动、迈过小台阶；
- 不希望角色被普通刚体随意撞飞。

它不是普通 `Rigidbody`。`Move()` 会处理碰撞约束，但不会自动施加重力。

### 重要属性

| 属性 | 作用 |
| --- | --- |
| `height`、`radius`、`center` | 胶囊形状 |
| `slopeLimit` | 可攀爬的最大坡度 |
| `stepOffset` | 可迈过的台阶高度 |
| `skinWidth` | 碰撞外壳余量，过小容易抖动 |
| `minMoveDistance` | 小于该值的移动可被忽略，通常保持较小 |
| `isGrounded` | 上一次移动后是否接触地面 |
| `velocity` | 控制器最近一次移动形成的速度 |

### `Move()`

```csharp
CollisionFlags flags = controller.Move(motion * Time.deltaTime);
```

特点：

- 参数是本次要移动的位移，不是“每秒速度”；
- 通常用速度乘 `Time.deltaTime` 得到位移；
- 会受场景 Collider 阻挡；
- 不会自动产生重力；
- 返回 `CollisionFlags`，可判断上下或侧面碰撞。

### `SimpleMove()`

```csharp
controller.SimpleMove(horizontalVelocity);
```

特点：

- 参数是速度；
- 自动应用重力；
- 不需要再乘 `Time.deltaTime`；
- 不适合直接实现自定义竖直速度和跳跃。

初学阶段若需要跳跃，使用 `Move()` 更容易理解完整过程。

### 完整示例：移动、转向、重力和跳跃

本项目已安装新输入系统，因此示例直接读取 `Keyboard.current`。

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimpleCharacterMover : MonoBehaviour
{
    [Header("移动")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 12f;

    [Header("竖直运动")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.5f;

    private CharacterController controller;
    private float verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed) horizontal -= 1f;
        if (keyboard.dKey.isPressed) horizontal += 1f;
        if (keyboard.sKey.isPressed) vertical -= 1f;
        if (keyboard.wKey.isPressed) vertical += 1f;

        Vector3 input = new Vector3(horizontal, 0f, vertical);
        input = Vector3.ClampMagnitude(input, 1f);

        Vector3 horizontalVelocity = input * moveSpeed;

        if (input.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(input);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            // 保持一个很小的向下速度，使角色稳定贴地
            verticalVelocity = -2f;
        }

        if (controller.isGrounded && keyboard.spaceKey.wasPressedThisFrame)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = horizontalVelocity;
        velocity.y = verticalVelocity;

        CollisionFlags flags = controller.Move(velocity * Time.deltaTime);

        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
        {
            verticalVelocity = 0f;
        }
    }
}
```

搭建步骤：

1. 创建一个 Capsule 或空物体作为角色根对象。
2. 添加 `CharacterController`。
3. 添加 `SimpleCharacterMover`。
4. 创建 Plane 作为地面，并确保它有 Collider。
5. 播放场景，用 W、A、S、D 移动，空格跳跃。

> 如果角色模型朝向与世界 Z 正方向不一致，可把模型作为角色根对象的子物体，再单独调整模型局部旋转。

### `OnControllerColliderHit()`

当角色控制器在调用 `Move` 时撞到普通 Collider，可接收碰撞信息。

```csharp
void OnControllerColliderHit(ControllerColliderHit hit)
{
    Debug.Log($"碰到：{hit.gameObject.name}");
}
```

如果要推动刚体，可根据 `hit.rigidbody` 和移动方向施加受控效果，但要限制力度，避免角色把所有物体瞬间弹飞。

### CharacterController 与 Rigidbody 的区别

| 对比项 | CharacterController | Rigidbody |
| --- | --- | --- |
| 主要控制方式 | 脚本直接调用 `Move` | 力、速度或物理移动 |
| 自动重力 | `Move` 不自动提供 | 可使用物理重力 |
| 被其他刚体推动 | 默认不会像普通刚体那样响应 | 会参与动力学反应 |
| 台阶、坡度 | 提供专门参数 | 需通过碰撞体、力和额外逻辑处理 |
| 常见用途 | 玩家角色 | 箱子、球、载具、物理角色 |

不要在同一个角色根对象上同时使用 `CharacterController` 和一个参与普通动力学的非运动学 `Rigidbody` 来争夺移动控制权。

---

## 会移动的角色和跟随摄像机

### 场景结构

```text
DemoScene
├─ Ground
├─ Obstacles
├─ Player
│  ├─ Model
│  ├─ CharacterController
│  └─ SimpleCharacterMover
└─ Main Camera
   └─ SimpleCameraFollow
```

### 简单摄像机跟随脚本

```csharp
using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -7f);
    [SerializeField] private float followSpeed = 8f;

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        transform.LookAt(target);
    }
}
```

讲解重点：

- 角色在 `Update` 移动；
- 摄像机在 `LateUpdate` 跟随；
- `target` 通过 Inspector 连接，避免运行时按名称反复查找；
- `Time.deltaTime` 让跟随速度较少受帧率影响。

---
