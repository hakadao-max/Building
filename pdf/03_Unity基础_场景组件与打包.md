# Unity 基础：场景组件、物理、渲染与打包



## 先建立 Unity 的对象模型

Unity 场景可以理解为一棵由 `GameObject` 组成的树：

```text
Scene
├─ Environment
│  ├─ Ground
│  ├─ Buildings
│  └─ Props
├─ Characters
│  └─ Player
├─ Cameras
│  └─ Main Camera
├─ Lighting
│  ├─ Directional Light
│  └─ Reflection Probe
└─ Systems
   └─ GameManager
```

每个 `GameObject` 都有 `Transform`，其他能力由组件提供。教学时建议不断强调：

```text
GameObject 是容器，Component 提供能力，Transform 决定空间和层级。
```

---

## Transform 节点与父子层级

### Transform 的三个核心值

- Position：位置；
- Rotation：旋转；
- Scale：缩放。

Inspector 中显示的通常是相对父物体的局部值。脚本中常见两组接口：

| 世界空间 | 局部空间 |
| --- | --- |
| `position` | `localPosition` |
| `rotation` | `localRotation` |
| 没有直接等价的可靠世界缩放设置 | `localScale` |

### 为什么要使用父子层级

父物体变化会影响子物体。例如：

```text
PlayerRoot
├─ Model
├─ InteractionPoint
└─ NameCanvas
```

移动 `PlayerRoot` 时，模型、交互点和头顶 UI 会一起移动；但它们仍能保留各自的局部偏移。

### 层级设计原则

1. 根对象负责整体位置和主要逻辑。
2. 可视模型作为子物体，方便单独修正朝向和缩放。
3. 摄像机、交互点、武器挂点使用命名清楚的空物体作为锚点。
4. 避免在层级深处随意使用非均匀缩放，否则旋转、碰撞体和子对象可能出现难以理解的变形。
5. 运行时频繁移动的对象不要挂在巨大的静态环境层级下。

### Pivot 与 Center

Scene 工具栏中的 Pivot/Center 只影响编辑工具手柄的位置：

- Pivot：使用对象真实轴心；
- Center：使用所选对象整体包围盒中心。

它不会真正修改模型文件的轴心。如果模型轴心不合适，常用一个空父物体作为新控制根节点。

### 课堂演示

1. 创建空物体 `ChairRoot`。
2. 把椅子模型作为子物体。
3. 调整模型局部位置，让 `ChairRoot` 的原点位于椅脚中心。
4. 只旋转 `ChairRoot`，观察模型跟随。

---

## Collider：给物体建立碰撞形状

Collider 定义物体在物理世界中的形状。它通常不可见，但会参与碰撞检测、射线检测和 Trigger 检测。

### 常见 Collider

| 类型 | 特点 | 适用对象 |
| --- | --- | --- |
| Box Collider | 快速、稳定、易调整 | 墙、门、箱子、桌面 |
| Sphere Collider | 方向无关、计算简单 | 球、范围检测、圆形道具 |
| Capsule Collider | 适合直立角色 | 人形角色、柱状物 |
| Mesh Collider | 贴合网格，成本较高 | 形状复杂的静态环境 |
| Terrain Collider | 配合 Terrain | 地形 |
| Character Controller | 特殊胶囊式角色控制器 | 玩家或脚本控制角色 |

### Collider 不是可视模型

视觉网格和碰撞形状是两套数据：

- 模型可以很复杂；
- 碰撞体应尽量简单；
- 多个 Box、Sphere、Capsule 可以组合成近似形状；
- 玩家看不到的细节通常不需要精确碰撞。

这种做法称为复合碰撞体。它通常比给动态物体直接使用复杂 Mesh Collider 更稳定、更高效。

### Mesh Collider 的注意事项

- 静态建筑、岩石、地面可使用非 Convex Mesh Collider；
- 要跟随非运动学 Rigidbody 运动的 Mesh Collider 通常需要勾选 Convex；
- Convex 会把形状近似成凸包，凹洞可能被封住；
- 复杂动态物体优先使用多个基础 Collider 组合。

### Physics Material

Physics Material 决定碰撞表面的摩擦和弹性，不是用来控制颜色的材质球。

常见参数：

- Dynamic Friction：运动时摩擦；
- Static Friction：静止时摩擦；
- Bounciness：弹性；
- Combine：双方材质接触时如何合并数值。

例如：

- 冰面：低摩擦；
- 橡胶球：较高弹性；
- 普通地面：中等摩擦、低弹性。

---

## 碰撞系统：Collider 与 Rigidbody 如何配合

### 三种常见物体

#### 静态碰撞体

- 有 Collider；
- 没有 Rigidbody；
- 运行时通常不移动。

例如地面、墙壁、固定建筑。

#### 动态刚体

- 有 Collider；
- 有非 Kinematic Rigidbody；
- 受重力、力、速度和碰撞影响。

例如球、箱子、掉落物。

#### 运动学刚体

- 有 Collider；
- 有 Rigidbody；
- `Is Kinematic` 开启；
- 由脚本或动画控制，不接受普通力推动，但可以参与碰撞检测。

例如自动门、移动平台、脚本控制机关。

### 发生碰撞回调的基本条件

两个 3D Collider 接触时，至少一方通常需要 Rigidbody，脚本才能稳定收到物理碰撞回调。

常用回调：

```csharp
void OnCollisionEnter(Collision collision)
{
    Debug.Log($"开始碰撞：{collision.gameObject.name}");
}

void OnCollisionStay(Collision collision)
{
    Debug.Log($"持续碰撞：{collision.gameObject.name}");
}

void OnCollisionExit(Collision collision)
{
    Debug.Log($"结束碰撞：{collision.gameObject.name}");
}
```

`Collision` 可以提供接触点、法线、相对速度等信息。

### 碰撞层

每个对象都有 Layer。通过 Project Settings > Physics 中的 Layer Collision Matrix，可决定哪些层之间需要碰撞。

推荐按职责建立层，例如：

```text
Player
Enemy
Environment
Interactable
Projectile
TriggerZone
```

如果两个对象完全不需要互相检测，就在矩阵中关闭它们，减少无意义计算并降低逻辑干扰。

### 碰撞检测模式

高速刚体可能在相邻物理步之间越过薄墙，这叫“穿透”或“隧道效应”。Rigidbody 的 Collision Detection 可按需要选择离散或连续模式。

原则：

- 大多数普通、低速物体使用默认离散模式；
- 高速、小尺寸、不能穿墙的物体再使用连续模式；
- 连续检测成本更高，不要给所有物体统一开启。

---

## Trigger 触发器：检测进入区域但不阻挡

Collider 勾选 `Is Trigger` 后，它会变成触发器区域：

- 能检测其他 Collider 进入；
- 不再像实体墙一样阻挡对象；
- 适合传送区、任务区、拾取范围、门禁区、音频区域。

### Trigger 回调

```csharp
using UnityEngine;

public class WelcomeZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家进入区域");
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家停留在区域中");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家离开区域");
        }
    }
}
```

### Trigger 不触发时的排查

1. 双方是否都有 3D Collider，而不是一方误用了 2D Collider？
2. 触发区域是否勾选 `Is Trigger`？
3. 相关对象中是否至少有 Rigidbody 或 Character Controller 等参与物理检测的组件？
4. Layer Collision Matrix 是否关闭了这两层的接触？
5. 脚本函数名和参数类型是否完全正确？
6. 对象或脚本是否处于激活状态？

> 2D 物理使用 `Collider2D`、`Rigidbody2D` 和 `OnTriggerEnter2D`，不能与 3D 物理回调混用。

---

## Skinned Mesh Renderer：显示带骨骼动画的角色

### 它与 Mesh Renderer 的区别

- `Mesh Renderer`：显示普通静态网格，通常配合 `Mesh Filter`；
- `Skinned Mesh Renderer`：显示受骨骼、蒙皮权重和 Blend Shape 影响的变形网格。

角色抬手时，骨骼旋转，顶点按照权重变形，因此皮肤和衣服能够跟随。

### 重要属性

| 属性 | 作用 |
| --- | --- |
| Mesh | 要渲染的蒙皮网格 |
| Materials | 各子网格使用的材质 |
| Root Bone | 骨骼层级的根 |
| Bones | 影响网格的骨骼列表 |
| Bounds | 判断网格是否在摄像机可见范围内 |
| Update When Offscreen | 离开视野后是否继续更新蒙皮 |
| Cast Shadows / Receive Shadows | 投射与接收阴影 |

### 常见问题

#### 动画播放时角色局部消失

可能是 Bounds 太小。先检查导入设置、动画范围和根骨骼，再谨慎调整 Bounds。不要一开始就无限放大 Bounds，否则离屏剔除会失去效果。

#### 材质数量不对

网格可能包含多个 SubMesh。Materials 数量与顺序应和导入网格匹配。

#### 模型粉红色

通常表示 Shader 不兼容或丢失。本项目使用 URP，应使用 URP 支持的 Shader，例如 `Universal Render Pipeline/Lit`。

#### 角色离开视野后动画状态异常

先确认 Animator 的剔除设置和 Skinned Mesh Renderer 的更新策略。`Update When Offscreen` 会增加成本，只在确有需要时启用。

### 推荐角色层级

```text
PlayerRoot
├─ CharacterController
├─ PlayerMovement
├─ ModelRoot
│  ├─ Armature
│  └─ Body (Skinned Mesh Renderer)
└─ CameraTarget
```

让角色控制、可视模型和摄像机锚点分开，后续更容易调整。

---

## 材质球：决定表面如何被渲染

### Material、Shader 与 Texture

- Shader：表面如何计算颜色、光照、透明等效果的程序；
- Material：某个 Shader 的一组具体参数；
- Texture：输入给材质的图像或数据。

可以把 Shader 理解为配方，Material 是按配方调好的材料，Texture 是材料使用的图案或数据。

### URP Lit 常用参数

- Base Map：基础颜色和贴图；
- Metallic：金属程度；
- Smoothness：表面光滑程度；
- Normal Map：细小凹凸方向；
- Occlusion Map：局部遮蔽；
- Emission：自发光；
- Surface Type：Opaque 或 Transparent；
- Alpha Clipping：按透明度裁切，适合树叶、铁丝网等。

### 创建与应用材质

1. 在 Project 窗口右键创建 Material。
2. Shader 选择 `Universal Render Pipeline/Lit`。
3. 设置 Base Map、Metallic 和 Smoothness。
4. 把材质拖到场景物体，或放入 Renderer 的 Materials 列表。

### 材质实例注意事项

运行时：

```csharp
Renderer targetRenderer = GetComponent<Renderer>();
targetRenderer.material.color = Color.red;
```

访问 `.material` 可能为该 Renderer 创建独立材质实例。若要修改所有共享同一材质的对象，要明确是否应该改 `.sharedMaterial`；编辑器工具中尤其要谨慎，避免意外修改资源。大量对象只改变少数参数时，可进一步学习 `MaterialPropertyBlock`。

---

## 天空盒：场景背景与环境光来源

### 天空盒的作用

天空盒不仅是背景，也可以影响：

- 环境光；
- 反射探针和高光反射；
- 场景整体色调与时间氛围。

### 设置步骤

1. 创建 Material。
2. 选择适合的天空盒 Shader，例如 Cubemap 或 Panoramic。
3. 配置天空纹理。
4. 打开 Window > Rendering > Lighting。
5. 在 Environment 设置中把该材质指定为 Skybox Material。
6. 根据场景需要调整 Environment Lighting 的来源和强度。

### 摄像机是否显示天空盒

摄像机背景类型设为 Skybox 时显示天空盒；设为纯色或其他背景方式时，环境光仍可能受 Lighting 设置影响，但画面背景不会显示该天空。

### 常见问题

- 天空过亮：降低材质 Exposure 或环境光强度；
- 反射没有更新：重新烘焙或刷新 Reflection Probe；
- 天空旋转不对：调整材质 Rotation；
- 室内被天空照得过亮：调整环境光、遮蔽、光照贴图与反射探针，而不是只把天空盒删掉。

---

## 灯光

### 常见灯光类型

| 类型 | 特点 | 常见用途 |
| --- | --- | --- |
| Directional Light | 模拟无限远平行光 | 太阳、月光 |
| Point Light | 从一点向四周发光 | 灯泡、火焰 |
| Spot Light | 锥形光束 | 手电筒、舞台灯 |
| Area Light | 面光源，通常用于烘焙 | 窗户、柔和室内光 |

### 常用参数

- Color：颜色；
- Intensity：强度；
- Range：点光源和聚光灯的范围；
- Spot Angle：聚光灯锥角；
- Shadows：阴影类型与强度；
- Culling Mask 或 Rendering Layers：灯光影响哪些对象；
- Mode：Realtime、Mixed 或 Baked。

### 三种灯光模式

#### Realtime

运行时实时计算，适合移动灯光、昼夜变化和动态效果。灵活，但成本较高。

#### Baked

光照结果预先写入光照贴图，运行时成本低，适合静态环境。它不能像实时光那样完整响应运行时移动物体。

#### Mixed

尝试结合烘焙与实时效果。具体行为取决于 Mixed Lighting 模式和渲染管线设置。

### 灯光设计原则

1. 先确定主光方向，再补充局部光。
2. 不要靠大量灯光平均照亮一切，要保留明暗层次。
3. 用光线引导玩家关注入口、目标和危险。
4. 实时阴影成本高，按重要性分配。
5. 用 Frame Debugger、Rendering Debugger 和 Profiler 检查实际开销。

---

## Bake：光照烘焙

### 为什么烘焙

静态环境的间接光、柔和阴影和颜色反弹可以预先计算，保存到 Lightmap 中。这样运行时不必每帧重新计算完整光照。

### 烘焙前准备

1. 确认参与烘焙的环境对象适合标记为静态，或在 Mesh Renderer 中启用相应的 GI 贡献设置。
2. 检查模型是否有合适的光照贴图 UV；必要时在模型导入设置中生成 Lightmap UV。
3. 为灯光选择 Baked 或 Mixed。
4. 打开 Window > Rendering > Lighting。
5. 配置 Lightmapping Settings、Lightmapper、Lightmap Resolution、最大贴图尺寸等。
6. 设置环境光和天空盒。

### 开始烘焙

在 Lighting 窗口中生成光照。小场景先使用较低分辨率快速预览，构图和灯光稳定后再提高质量。

建议迭代顺序：

```text
低质量快速烘焙
→ 检查方向、曝光和漏光
→ 调整 UV、灯光和几何体
→ 中等质量验证
→ 最终质量烘焙
```

### 常见问题

#### 光斑、接缝或脏块

检查 Lightmap UV 是否重叠、边缘间距是否足够、分辨率是否过低。

#### 墙角漏光

检查墙体是否过薄、几何是否有缝隙、法线是否正确，以及烘焙采样与偏移设置。

#### 动态角色显得与环境脱离

使用 Light Probe Group，让动态对象获得空间中的烘焙光照信息；使用 Reflection Probe 改善反射。

#### 烘焙很慢

先降低分辨率和采样，缩小测试范围，确认模型 UV 与静态设置，最终阶段再提高质量。

---

## 摄像机

### 摄像机决定玩家看到什么

Camera 把三维场景投影成最终画面。常用参数：

- Projection：Perspective 或 Orthographic；
- Field of View：透视视野角；
- Clipping Planes：最近和最远渲染距离；
- Culling Mask：渲染哪些 Layer；
- Background：天空盒或背景色；
- Target Texture：输出到 Render Texture；
- Depth 和 URP Camera Stack：控制多个摄像机组合。

### 透视与正交

- Perspective：近大远小，适合大多数 3D 游戏；
- Orthographic：物体大小不随距离改变，适合 2D、策略视图、建筑图式画面。

### MainCamera 标签

需要被 `Camera.main` 找到的摄像机应使用 `MainCamera` 标签。频繁访问时仍建议缓存引用，因为查找不是免费的。

### Render Texture

把 Camera 的 Target Texture 指向 Render Texture，可以制作：

- 监控摄像机画面；
- 小地图；
- 镜子或传送门的近似画面；
- UI 中的角色预览。

随后可在 UGUI `RawImage` 中显示该 Render Texture。

### 摄像机配置检查

1. 近裁剪面不要过大，否则贴近摄像机的物体会消失。
2. 远裁剪面不要无意义地设得极远，会影响深度精度和渲染成本。
3. 检查 Culling Mask，避免意外漏渲染或重复渲染。
4. 角色跟随摄像机通常放在 `LateUpdate`，并处理墙体遮挡。

---

## Prefab：可复用的对象模板

### 为什么使用 Prefab

Prefab 能保存一组完整配置：

- 层级；
- 组件；
- 参数；
- 材质和资源引用；
- 子对象。

敌人、子弹、门、道具、UI 面板都适合做成 Prefab。

### 创建 Prefab

1. 在场景中配置好对象。
2. 把对象从 Hierarchy 拖到 Project 文件夹。
3. Project 中生成 Prefab Asset。
4. 场景中的对象成为该 Prefab 的实例。

### 实例覆盖

修改 Prefab 实例后，Inspector 会记录 Overrides：

- Apply：把改动写回 Prefab，影响其他实例；
- Revert：放弃实例改动，恢复 Prefab 状态。

应用前要确认修改是否真的应该影响所有实例。

### Prefab Variant

Variant 继承一个基础 Prefab，并保存差异。例如：

```text
Enemy_Base
├─ Enemy_Red
├─ Enemy_Blue
└─ Enemy_Boss
```

共同组件在基础 Prefab 中维护，颜色、数值或模型差异放在 Variant 中。

### 运行时实例化

```csharp
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform spawnPoint;

    public void Spawn()
    {
        if (prefab == null || spawnPoint == null)
        {
            return;
        }

        Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }
}
```

频繁生成和销毁大量对象时，应进一步学习对象池；本课先掌握 Prefab 和 `Instantiate`。

---

## 角色控制器

### 创建角色根对象

推荐结构：

```text
PlayerRoot
├─ CharacterController
├─ MovementScript
├─ ModelRoot
│  └─ CharacterModel
└─ CameraTarget
```

### 配置 Character Controller

1. 调整 Height 和 Radius，使胶囊包住角色。
2. 调整 Center，让胶囊底部接近脚底。
3. `Step Offset` 不应高于角色能合理跨越的台阶。
4. `Slope Limit` 决定可走坡度。
5. `Skin Width` 不要设得过小，以免频繁卡住或抖动。

### 移动原则

- 在 `Update` 读取输入并调用 `CharacterController.Move`；
- 传入“本帧位移”，即速度乘 `Time.deltaTime`；
- 自己维护重力与跳跃的竖直速度；
- 使用 `isGrounded` 或更精确的地面检测判断落地；
- 通过 Layer 控制哪些对象能阻挡角色。

完整移动代码见[《Unity 语法基础：MonoBehaviour、Transform、GameObject 与角色控制器》](./02_Unity语法基础_MonoBehaviour与常用对象.md)。

---

## 综合课堂实践：搭建可探索的小房间

### 目标

完成一个可打包的 3D 小场景：

- 有地面、墙和门；
- 有材质、天空盒、主灯和烘焙光；
- 玩家可移动；
- 进入门口 Trigger 时门打开；
- 有监控摄像机输出到 UI；
- 门和玩家被制作为 Prefab。

### 推荐步骤

1. 用 Cube 搭建地面和墙体，使用简单 Box Collider。
2. 创建 URP Lit 材质，区分地面、墙和门。
3. 设置天空盒与 Directional Light。
4. 为静态环境准备光照烘焙并生成 Lightmap。
5. 创建 `PlayerRoot`，添加 Character Controller 和移动脚本。
6. 创建门，添加 Collider、Animator 或简单旋转脚本。
7. 在门前创建 Box Collider，勾选 `Is Trigger`。
8. 玩家进入时切换门动画参数。
9. 创建第二台 Camera，输出到 Render Texture。
10. 把 Render Texture 放到 RawImage 中。
11. 把玩家和门拖到 Project，生成 Prefab。
12. 保存场景，进入 Build Profiles 构建 Windows 版本。

### 门区示例

```csharp
using UnityEngine;

public class AutomaticDoorZone : MonoBehaviour
{
    [SerializeField] private Animator doorAnimator;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            doorAnimator.SetBool("Open", true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            doorAnimator.SetBool("Open", false);
        }
    }
}
```

---

## 打包成可执行文件

以下以 Windows 桌面程序为例。

### 构建前检查

1. 保存所有场景和资源。
2. 清空 Console，修复所有编译错误。
3. 在 Game 视图测试目标分辨率和宽高比。
4. 确认要构建的场景已加入构建场景列表。
5. 检查场景中的摄像机、EventSystem、光照数据和必要管理对象。
6. 进入 Project Settings > Player，设置 Company Name、Product Name、版本号、图标和窗口选项。
7. 确认输入系统、渲染管线和质量等级在目标平台正常。

### 使用 Build Profiles

1. 打开 File > Build Profiles。
2. 创建或选择 Windows 平台的 Build Profile。
3. 如果 Windows 构建支持未安装，通过 Unity Hub 为当前编辑器安装对应模块。
4. 把需要的场景加入 Scene List，并确认启动场景位于第一项。
5. 根据需要切换到该 Profile。
6. 设置 CPU Architecture、Development Build 等选项。
7. 普通发布版本不要勾选 Development Build；调试时可临时开启。
8. 点击 Build，选择一个空的输出文件夹。

### Windows 构建结果

典型结果包含：

```text
MyGame.exe
MyGame_Data/
UnityPlayer.dll
其他运行库文件
```

发布给其他人时，应打包整个输出文件夹，不能只发送 `.exe`。
