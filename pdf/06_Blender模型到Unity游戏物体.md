# 从 Blender 模型到 Unity 可用的游戏物体

在 Blender 里看起来正常的模型，导入 Unity 后不一定能直接使用。

常见情况包括：

- 模型大得像一座山，或者小得几乎看不见；
- 模型横躺、倒着或朝向相反；
- 材质变白、贴图丢失或整个模型变粉；
- 门的轴心在中间，旋转时像螺旋桨；
- 人物骨骼扭曲，动画播放时脚底打滑；
- 模型能看见，却没有碰撞；
- 在 Inspector 中改好了，重新导入 FBX 后改动又消失；
- 模型虽然能运行，却有几十个材质和过于复杂的碰撞体，性能很差。

所以完整流程不是“把 `.blend` 文件拖进 Unity”，而是：

```text
确认用途
→ 在 Blender 整理模型
→ 处理轴心、比例、法线、UV 和材质
→ 导出 FBX 与贴图
→ 在 Unity 配置模型导入
→ 重建 URP 材质
→ 添加碰撞和游戏组件
→ 制作 Prefab
→ 在实际场景中测试
```

---

## 先决定模型要在游戏里做什么

导出前先明确用途，因为不同物体需要不同处理方式。

### 静态环境模型

例如：

- 墙壁；
- 建筑；
- 石头；
- 桌椅；
- 道路；
- 不会变形的道具。

重点是：

- 比例和轴心正确；
- UV 与材质正确；
- 碰撞体简单；
- 能参与灯光烘焙；
- 材质数量和面数合理。

### 可运动的机械物体

例如：

- 门；
- 抽屉；
- 车轮；
- 风扇；
- 升降平台；
- 枪械部件。

重点是：

- 每个活动部件要拆成单独对象；
- 每个部件的轴心必须放在真实转轴位置；
- 父子层级要清楚；
- 不能为了方便建模把所有部件合并成一个 Mesh。

### 骨骼角色

例如：

- 人形角色；
- 动物；
- 怪物；
- 需要骨骼变形的机械体。

重点是：

- Armature 和模型比例一致；
- 权重正确；
- 骨骼命名清楚；
- 动画已经烘焙；
- Unity Rig 类型选择正确；
- 角色根节点、模型和碰撞控制器分开。

### 只用于碰撞的模型

复杂场景可以单独制作低面数碰撞模型，例如：

```text
Building_Visual
Building_Collision
```

视觉模型负责好看，碰撞模型负责稳定和性能。两者不必拥有相同的面数。

---

## Blender 中先统一尺寸

### 把一米当作一米

Unity 中通常把一个单位看作一米。Blender 也建议使用 Metric，并让一个 Blender 单位对应一米。

可以在 Blender 的 Scene Properties > Units 中设置：

```text
Unit System：Metric
Unit Scale：1.0
Length：Meters
```

设置单位不会自动修复已经建错尺寸的模型。仍然要使用测量工具或 Item 面板检查真实大小。

参考尺寸：

| 物体 | 可用于检查的常见量级 |
| --- | --- |
| 普通房门 | 高约 2 米 |
| 成人角色 | 高约 1.6～1.9 米 |
| 普通桌面 | 高约 0.7～0.8 米 |
| 楼层 | 高约 3 米左右 |
| 游戏角色胶囊 | 应与角色视觉高度接近 |

这些数值不是设计限制，只是帮助发现“模型大了 100 倍”之类的问题。

### 不要靠 Unity 的 Scale 修正全部问题

如果模型导入后必须把 Transform Scale 设置成 `0.01` 才正常，说明源文件或导出单位很可能不统一。

偶尔调整实例缩放没有问题，但基础资源最好在 Blender 中保持正确尺寸，再以 Unity Scale Factor 1 导入。这样碰撞体、动画、物理质量、粒子和导航参数更容易保持一致。

---

## 处理位置、旋转和缩放

### 应用 Rotation 和 Scale

Blender 中模型看起来缩放为正常大小，不代表它的 Transform 已经干净。

选中要导出的对象，使用：

```text
Ctrl + A > Rotation & Scale
```

应用后通常应看到：

```text
Rotation：0, 0, 0
Scale：1, 1, 1
```

这一步对以下内容非常重要：

- 法线；
- 修改器；
- 骨骼绑定；
- 碰撞体尺寸；
- Unity 中的缩放；
- 动画旋转。

不要在已经做完动画以后随意应用 Armature 的变换。角色应在绑定和制作动画之前把模型、骨架和单位整理好。

### Location 不一定需要应用

位置是否应用取决于轴心设计。

如果把所有对象位置都应用为零，可能会破坏多个部件之间的相对位置。通常更重要的是：

- 物体的 Origin 是否正确；
- 导出时对象是否处于合理位置；
- 需要组合的部件是否共享清楚的父对象。

### Blender 与 Unity 的坐标轴不同

Blender 使用 Z 轴向上，Unity 使用 Y 轴向上。FBX 导出器会进行坐标转换。

常见 Unity FBX 导出设置是：

```text
Forward：-Z Forward
Up：Y Up
```

不要在 Blender 中把模型手动转 90 度、导出时再转一次、Unity 根节点又转一次。这样虽然某一版可能“看起来对”，动画和后续更新通常会越来越乱。

推荐做一个固定测试：

1. 在 Blender 中让角色或物体朝统一的前方建模。
2. 使用同一套 FBX 导出预设。
3. 导入 Unity 后检查模型的本地前方是否符合项目规范。
4. 如果需要视觉朝向修正，把模型放到一个 Unity 子节点中修正一次，不要到处补旋转。

---

## 把轴心放到真正有用的位置

轴心决定物体在 Unity 中如何旋转、缩放和对齐。

### 常见轴心位置

| 物体 | 推荐轴心 |
| --- | --- |
| 普通箱子 | 底部中心或几何中心，按放置方式决定 |
| 门 | 门轴铰链位置 |
| 车轮 | 轮轴中心 |
| 风扇叶片 | 转轴中心 |
| 抽屉 | 与柜体保持合理局部位置 |
| 角色 | 地面上的脚底中心 |
| 可拾取道具 | 便于抓取和旋转的位置 |

### 设置 Origin

常用方法：

1. 在 Edit Mode 选择希望作为轴心参考的顶点或边。
2. 使用 `Shift + S` 把 3D Cursor 放到选中位置。
3. 回到 Object Mode。
4. 使用 Object > Set Origin > Origin to 3D Cursor。

例如制作门时，把 Origin 放在铰链一侧。导入 Unity 后只需要旋转门对象，不需要再创建复杂补偿。

### 多部件物体使用根节点

一把枪可能包含：

```text
WeaponRoot
├─ Body
├─ Trigger
├─ Magazine
├─ Bolt
└─ MuzzlePoint
```

在 Blender 中可以创建空对象作为根，也可以在 Unity 中建立包装根节点。重要的是每个活动部件保持独立，且局部轴心正确。

---

## 清理 Mesh

### 删除重复点和无用几何

进入 Edit Mode，全选后可使用 Merge by Distance 清理意外重叠的顶点。

同时检查：

- 模型内部是否有看不见的面；
- 镜像后中线是否真正合并；
- 是否存在零面积面；
- 是否遗留不需要的测试物体；
- 是否有重复 Mesh 叠在一起；
- 修改器是否按预期生效。

不要为了“面数越少越好”破坏轮廓。游戏模型首先要在目标观看距离下保持正确形状，然后再优化看不见的细节。

### 检查法线方向

游戏通常只渲染三角形正面。法线反向时，Unity 中可能出现：

- 某些面消失；
- 光照发黑；
- 阴影异常；
- 室内墙面从一个方向完全透明。

在 Blender 中打开 Face Orientation：

- 蓝色通常表示朝外；
- 红色通常表示反面。

选中错误面后使用 Recalculate Outside，特殊结构再手动 Flip。

不要用 Unity 材质的双面渲染掩盖所有法线错误。双面材质有自己的性能和光照代价，只应在树叶、纸张、布片等确实需要双面的物体上使用。

### 平滑与硬边

圆柱需要平滑，箱子边缘通常需要保持硬朗。如果所有面全部平滑，箱子会像被充气；如果所有面都硬，圆柱会显得棱角过多。

在 Blender 中根据版本使用 Shade Smooth、按角度平滑、Sharp Edge 或相应法线工具。导出时保留正确的法线与切线。

### 是否提前三角化

Unity 最终会把多边形转成三角形。如果模型包含复杂 N-gon、法线贴图或精确动画变形，Blender 和 Unity 的三角化方式不同可能导致外观变化。

重要资产可在 Blender 中添加 Triangulate Modifier，并在烘焙法线贴图之前固定最终三角形结构。

简单静态模型不一定要手动三角化，但必须在 Unity 中检查最终结果。

---

## 准备 UV 和贴图

### UV 是什么

UV 决定二维贴图如何铺在三维模型表面。一个模型至少可能需要：

- 用于材质贴图的 UV；
- 用于光照贴图的非重叠 UV。

### 材质 UV

检查：

- 接缝是否放在不明显位置；
- 重要区域是否获得足够纹理面积；
- 是否存在意外重叠；
- 纹理密度是否与同场景其他模型接近；
- 镜像 UV 是否会让文字和方向图案反过来。

如果多个物体共用一张纹理，可以使用 Texture Atlas 减少材质数量，但要注意分辨率和后续修改成本。

### Lightmap UV

参与 Unity 烘焙光照的静态模型需要合适的 Lightmap UV：

- 岛之间不能重叠；
- 岛之间要留出足够间距；
- 不要出现极度拉伸；
- 大面积重要表面应有合理占比。

可以在 Blender 中自己制作第二套 UV，也可以在 Unity Model Importer 中开启 Generate Lightmap UV。自动生成适合普通资产，复杂建筑最好人工检查。

### Blender 程序材质不会完整进入 Unity

Blender Shader Editor 中的复杂节点、噪声、ColorRamp、混合着色器和程序纹理，通常不能直接变成 Unity URP 材质。

需要把最终效果烘焙或导出为常见 PBR 贴图：

| Blender/PBR 内容 | Unity URP Lit |
| --- | --- |
| Base Color | Base Map |
| Metallic | Metallic Map |
| Roughness | 转换为 Smoothness |
| Normal | Normal Map |
| Ambient Occlusion | Occlusion Map |
| Emission | Emission Map |

Unity 中：

```text
Smoothness = 1 - Roughness
```

URP Lit 的 Metallic 工作流通常把 Smoothness 放在 Metallic 贴图的 Alpha 通道。直接把一张黑白 Roughness 图拖进去，结果往往会反过来或没有进入正确通道。

### 推荐贴图格式

- PNG：常用，无损，支持透明；
- TGA：常用于游戏贴图和 Alpha；
- EXR：HDR、烘焙或特殊数据；
- JPG：适合无透明、允许有损的颜色图，不适合法线和遮罩数据。

不要把所有贴图都嵌入 FBX。团队项目更适合把贴图作为独立文件管理，材质关系也更清楚。

---

## 命名和文件整理

清楚的名称能减少导入后大量猜测。

### Blender 对象命名示例

```text
SM_Chair_A
SM_DoorFrame_A
SM_DoorLeaf_A
SK_Character_A
Armature_Character_A
COL_Stair_A
SOCKET_Weapon_R
```

前缀不是必须使用这一套，但团队应保持一致。

### 材质命名示例

```text
M_Wood_Oak
M_Metal_Painted
M_Character_Skin
```

### 贴图命名示例

```text
T_Chair_BaseColor.png
T_Chair_Normal.png
T_Chair_MetallicSmoothness.png
T_Chair_Occlusion.png
```

### Unity 文件夹建议

```text
Assets
└─ Art
   ├─ Models
   │  ├─ Environment
   │  ├─ Props
   │  └─ Characters
   ├─ Materials
   ├─ Textures
   ├─ Animations
   └─ Prefabs
```

`.blend` 源文件可以放在项目外的 `SourceArt` 目录，或放在不会被 Unity 当作运行资源导入的版本管理目录中。提交到 Unity `Assets` 的是经过确认的 FBX、贴图和相关资源。

Unity 能通过本机安装的 Blender 间接处理 `.blend`，但这会依赖每台电脑的 Blender 版本和安装状态。团队协作和稳定构建更推荐显式导出 FBX。

---

## 导出静态模型 FBX

### 导出前检查

- [ ] 尺寸正确。
- [ ] Rotation 与 Scale 已应用。
- [ ] Origin 位于正确位置。
- [ ] 法线朝向正确。
- [ ] UV 已完成。
- [ ] 不需要的对象已隐藏或不会被导出。
- [ ] 对象名称和材质名称清楚。
- [ ] 需要的修改器已经确认。

### 推荐导出方式

选中需要导出的物体，使用 File > Export > FBX。

常用设置可以从下面开始：

```text
Limit to：Selected Objects
Object Types：Mesh，需要时包含 Empty
Scale：1.0
Apply Scaling：保持项目统一预设
Forward：-Z Forward
Up：Y Up
Apply Modifiers：开启
Add Leaf Bones：关闭，静态模型无影响
Bake Animation：静态模型关闭
```

不同 Blender 版本的选项名称可能略有差异。关键不是死记每一个选项，而是建立一份项目统一的导出预设，并用一个标准测试模型验证比例、朝向和法线。

### 不要把整个 Blender 场景一次性全导出

如果一张 Blender 场景包含桌子、椅子、灯、门、测试相机和参考模型，直接全部导出会让 Unity 资源难以维护。

更常见的做法是：

- 每类可复用道具单独导出；
- 建筑按合理模块拆分；
- 需要整体保持位置的组合资产导出为一个 FBX；
- 摄像机和灯光只有在确实需要时才导出。

---

## 导入 Unity

把 FBX 放入项目中的 Models 文件夹，把贴图放入 Textures 文件夹。选中 FBX 后，Inspector 会显示 Model Importer。

### Model 设置

常用检查项：

#### Scale Factor 与 Convert Units

目标是模型实例在 Unity 中保持 Scale `1, 1, 1` 时，尺寸就是正确的。

如果尺寸错误，先回 Blender 检查单位、对象 Scale 和导出设置，不要立刻在每个 Prefab 上手工补缩放。

#### Normals

- Import：使用 Blender 导出的法线；
- Calculate：让 Unity 重新计算。

已经认真处理硬边和自定义法线的模型应优先 Import。若导入法线明显错误，再检查 FBX 是否真正导出了法线。

#### Tangents

使用 Normal Map 时需要正确切线。通常可让 Unity 计算 MikkTSpace 切线，或按项目管线保持统一。

#### Read/Write Enabled

只有运行时需要通过脚本读取或修改 Mesh 数据时才开启。普通静态模型关闭可以减少内存占用。

#### Mesh Compression

可以减少模型资源占用，但过高压缩可能让顶点位置和动画变形出现误差。重要轮廓、精密机械和角色先保持较低压缩并观察结果。

#### Generate Colliders

它会按 Mesh 自动生成碰撞体，适合快速原型，不适合代替正式碰撞设计。正式资产通常在 Prefab 中手动添加 Box、Capsule、Sphere 或专用低模 Collider。

#### Generate Lightmap UVs

静态烘焙模型没有第二套 UV 时可以开启。导入后仍要在场景烘焙中检查接缝和重叠。

### 点击 Apply

修改 Model Importer 后必须点击 Apply。Unity 会重新导入 FBX。

如果结果不对，优先修改 Blender 源文件和导入设置，不要在 FBX 的内部只读层级上进行无法保存的修补。

---

## 在 Unity 中重建 URP 材质

本项目使用 URP。导入 FBX 后即使自动生成了材质，也要检查 Shader 是否为 URP 支持的类型。

### 创建材质

1. 在 Materials 文件夹创建 Material。
2. Shader 选择 `Universal Render Pipeline/Lit`。
3. 把 Base Color 贴图放到 Base Map。
4. 把 Metallic/Smoothness 组合图放到 Metallic Map。
5. 把 Normal 贴图放到 Normal Map。
6. 把 AO 放到 Occlusion Map。
7. 需要时启用 Emission 并设置 Emission Map。
8. 把材质拖到模型 Renderer，或在 FBX Materials 页签中进行 Remap。

### 正确设置贴图类型

#### Base Color

保持 sRGB，因为它表示颜色。

#### Normal Map

在 Texture Importer 中把 Texture Type 设置为 Normal Map。Unity 弹出修复提示时，应确认贴图确实是法线图再转换。

#### Metallic、Roughness、AO 和遮罩

这些是数据贴图，通常不应按普通颜色进行 sRGB 采样。检查贴图的 sRGB 设置和通道打包方式。

### 模型变粉

粉色通常表示 Shader 丢失或不受当前渲染管线支持。

处理方法：

- 确认项目正在使用正确的 URP Asset；
- 把材质 Shader 改为 URP/Lit；
- 检查材质是否引用了项目中不存在的自定义 Shader；
- 不要只重新导入贴图，粉色问题通常在 Shader。

### 材质槽不要过多

一个 Mesh 上的每个材质槽通常意味着至少一个额外的渲染批次。一个简单杯子如果用了十几个材质，性能和维护都不划算。

可以在 Blender 中合并相同材质、使用 Atlas，或重新安排模型拆分。

---

## 把导入模型包装成游戏 Prefab

FBX 导入后会表现为 Model Prefab。它主要保存模型、层级和 Renderer，不适合直接承担所有游戏逻辑。

更推荐创建一个普通 Prefab 作为外层：

```text
Chair
├─ Model
│  └─ SM_Chair_A（FBX 实例）
├─ BoxCollider
└─ Interactable（需要时）
```

或者一扇门：

```text
Door
├─ DoorFrame
├─ DoorLeaf
│  └─ SM_DoorLeaf_A
├─ BoxCollider
├─ InteractionPoint
└─ DoorController
```

这样做的好处：

- FBX 更新时不会轻易破坏游戏组件；
- 可以在外层统一处理缩放、标签和 Layer；
- 碰撞体不依赖复杂可视 Mesh；
- 同一个模型可以制作多个玩法 Prefab；
- 美术资源与游戏逻辑职责分开。

### 更新模型时不要改文件名和层级标识

替换 FBX 后，Unity 会尽量保留材质映射和 Prefab 引用。但如果随意修改对象名、骨骼名、材质槽顺序或整个层级，已有引用可能断开。

更新前要确认：

- 文件路径是否保持不变；
- Mesh 和骨骼名称是否稳定；
- 材质槽是否只是新增，还是整体换序；
- 动画 Clip 名称是否变化；
- Prefab 中是否引用了某个具体子节点。

---

## 添加碰撞体

模型能显示，不代表它能碰撞。

### 优先使用基础 Collider

| 物体 | 推荐 Collider |
| --- | --- |
| 箱子、墙、桌面 | Box Collider |
| 球形道具 | Sphere Collider |
| 柱子、树干、角色近似体 | Capsule Collider |
| 复杂静态建筑 | 简化 Mesh Collider 或多个 Box |
| 动态复杂道具 | 多个基础 Collider 组成复合碰撞体 |

### Mesh Collider 的使用

非 Convex Mesh Collider 更适合不会移动的静态环境。带普通动态 Rigidbody 的物体通常不能使用任意凹形 Mesh Collider，需要开启 Convex，或改用复合基础 Collider。

Convex 会把凹洞封成凸包，所以椅子、拱门和碗等物体可能失去原来的空隙。

### 单独导出碰撞模型

可以在 Blender 中制作低模碰撞体并使用清楚名称：

```text
SM_Stair_Visual
COL_Stair_Simple
```

导入 Unity 后隐藏碰撞模型的 Mesh Renderer，只使用它的 Mesh 作为 Mesh Collider；或用它的位置搭建多个 Box Collider。

### 碰撞体要按玩法检查

不要只在 Scene 视图看线框。真正运行角色测试：

- 能否正常走过门洞；
- 楼梯会不会卡脚；
- 桌面能否放置物品；
- 椅子下方是否需要穿过；
- 射线能否准确选中；
- 动态物体是否会不停抖动。

---

## 制作门、抽屉和机械部件

### Blender 中拆分活动部分

门框和门扇必须是不同对象。门扇 Origin 位于铰链，门框保持自己的轴心。

不要把动画部件全合并为一个不可分离 Mesh，再希望 Unity 自动知道哪里应该旋转。

### Unity 中保持稳定层级

```text
DoorRoot
├─ FrameModel
└─ DoorPivot
   ├─ DoorModel
   └─ DoorCollider
```

即使 Blender 中门扇轴心正确，在 Unity 中增加 `DoorPivot` 仍然很有用，因为它能把模型更新和游戏旋转逻辑隔开。

### 动画方式

简单机械物体可以：

- 直接用脚本插值旋转；
- 使用 Unity Animator；
- 从 Blender 导入关键帧动画。

只有一个旋转轴的门通常不必专门在 Blender 制作动画。复杂机械、连续联动或需要精确曲线的装置再考虑导入动画。

---

## 骨骼角色导出前的准备

### 模型和 Armature 使用同一比例

绑定前应确认：

- Mesh Scale 为 `1, 1, 1`；
- Armature Scale 为 `1, 1, 1`；
- 角色高度正确；
- 角色脚底接近世界原点；
- 角色朝向符合项目约定。

绑定完成后再大幅修改整体比例，容易造成动画、Root Motion 和骨骼缩放问题。

### 只让需要的骨骼参与变形

控制 Rig 可能有大量 IK、控制器和辅助骨骼。游戏中通常只需要真正影响 Mesh 的 Deform Bones。

导出时可以选择 Only Deform Bones，避免把整套控制 Rig 带进 Unity。

### 关闭 Add Leaf Bones

Blender FBX 导出器可能为骨骼末端增加额外 Leaf Bone。Unity 角色一般不需要这些自动末端骨骼，通常关闭 Add Leaf Bones。

### 权重检查

在 Blender 中播放极端姿势，重点看：

- 肩部；
- 手肘；
- 手腕；
- 胯部；
- 膝盖；
- 脚踝；
- 衣服与身体交界。

清理没有作用的顶点组，限制单个顶点的骨骼影响数量。移动端或大量角色场景更应控制骨骼与权重复杂度。

### 形态键

Blender Shape Keys 可以对应 Unity Skinned Mesh Renderer 的 Blend Shapes，常用于表情和口型。

导出前检查：

- 名称清楚；
- Basis 正确；
- 修改器是否与 Shape Key 兼容；
- FBX 导入后 Blend Shapes 是否出现；
- 法线变化是否正常。

---

## 导出骨骼和动画

选择角色 Mesh 和 Armature，一起导出 FBX。

### Armature 设置

常用设置：

```text
Armature 与 Mesh 一起导出
Only Deform Bones：按项目需要开启
Add Leaf Bones：关闭
Forward：-Z Forward
Up：Y Up
```

### Bake Animation

如果动画使用 IK、约束、驱动器或复杂控制 Rig，Unity 不会理解 Blender 的全部控制逻辑。导出时需要把最终骨骼运动烘焙成关键帧。

检查：

- Bake Animation 是否开启；
- 开始帧和结束帧是否正确；
- Sampling Rate 是否足够；
- Simplify 是否删掉过多关键帧；
- 是否导出了需要的 Action 或 NLA Track；
- 没有使用的测试动画是否被一同导出。

### 一个 FBX 还是多个 FBX

常见两种流程：

#### 模型与动画放在同一个 FBX

适合动画较少、资源简单的角色。

#### 模型 FBX 与动画 FBX 分开

```text
Character_Model.fbx
Character_Idle.fbx
Character_Walk.fbx
Character_Run.fbx
```

适合动画较多、多人协作和频繁更新。动画 FBX 必须使用相同骨骼名称和层级，Unity 中可以引用模型 FBX 的 Avatar。

---

## Unity 中配置角色 Rig

选中角色 FBX，打开 Rig 页签。

### Humanoid

适合标准人形角色。优点是不同人形模型之间可以重定向动画。

设置：

```text
Animation Type：Humanoid
Avatar Definition：Create From This Model
```

点击 Apply 后再进入 Configure，检查 Unity 是否正确识别：

- Hips；
- Spine；
- Head；
- Upper Arm；
- Lower Arm；
- Hand；
- Upper Leg；
- Lower Leg；
- Foot。

如果骨骼映射错误，先修正 Avatar Mapping。角色 T Pose 不正确时，使用 Pose 工具或回 Blender 调整绑定姿态。

### Generic

适合：

- 动物；
- 怪物；
- 非标准人形；
- 机械骨骼；
- 不需要人形动画重定向的角色。

Generic 需要指定 Root Node，让 Unity 知道动画层级的根。

### Legacy

主要用于旧动画系统。新项目通常选择 Humanoid 或 Generic。

---

## Unity 中切分和检查动画

在 Animation 页签中可以查看导入的动画 Clip。

### 检查 Clip 范围

确认每个动画的：

- 名称；
- Start 帧；
- End 帧；
- 是否循环；
- Root Transform 设置；
- 预览是否完整。

### 循环动画

Idle、Walk、Run 通常开启 Loop Time。必要时开启 Loop Pose，让首尾姿势过渡更自然。

如果循环仍然明显跳动，应回 Blender 检查首尾关键帧、根骨骼位移和曲线切线。

### Root Motion

两种常见角色移动方式：

- In-place：动画在原地播放，角色位移由 Character Controller 或脚本控制；
- Root Motion：动画根骨骼本身带位移，由 Animator 推动角色。

两种方法都能使用，但不要让脚本和 Root Motion 同时重复移动同一个角色。

初学项目若使用 Character Controller，通常先导出原地走跑动画，再由脚本控制移动更容易理解。

### 动画事件

脚步声、挥刀命中和弹匣插入可以使用 Animation Event，但关键游戏状态不应只依赖某个模型动画事件。模型更新或动画替换后，要重新验证事件时间。

---

## 把角色做成可用 Prefab

推荐结构：

```text
PlayerRoot
├─ CharacterController
├─ PlayerMovement
├─ Animator
├─ ModelRoot
│  └─ SK_Character_A
├─ CameraTarget
├─ GroundCheck
└─ InteractionPoint
```

### 为什么要有 PlayerRoot

模型 FBX 只负责显示和骨骼动画。PlayerRoot 负责：

- 世界位置；
- Character Controller；
- 移动脚本；
- 输入；
- 标签与 Layer；
- 摄像机跟随点；
- 交互检测。

模型朝向不对时，只调整 `ModelRoot` 的局部旋转，不要让角色控制根节点带着奇怪的补偿旋转。

### Character Controller 配置

- Height 覆盖角色整体高度；
- Radius 接近身体宽度；
- Center 让胶囊底部落在脚下；
- Step Offset 符合能跨越的台阶；
- Slope Limit 符合关卡坡度。

播放动画时观察四肢可以超出胶囊，但身体主干和脚底应保持合理关系。

---

## 添加 LOD

复杂模型离摄像机很远时，不需要继续渲染全部细节。

可以在 Blender 制作：

```text
SM_Tree_LOD0
SM_Tree_LOD1
SM_Tree_LOD2
```

Unity 中在 Prefab 根节点添加 LOD Group，把不同模型分配到对应等级。

### LOD 制作重点

- 优先保持外轮廓；
- 删除看不见的小部件；
- 减少远处材质槽；
- 法线和 UV 仍要正确；
- 切换距离要在实际 Game 视图测试；
- 避免 LOD 切换时体积突然变化；
- 角色 LOD 还要考虑骨骼数量和 Skinned Mesh 成本。

不要只比较面数。一个低面模型如果仍有很多材质和骨骼，渲染成本可能依然很高。

---

## 性能检查

### 面数

没有一条适合所有平台的固定面数标准。应根据：

- 目标平台；
- 同屏数量；
- 摄像机距离；
- 是否变形；
- 材质复杂度；
- 是否有 LOD；
- 项目实际性能测试。

来决定预算。

### 材质槽

一个模型材质槽越多，通常需要越多渲染批次。能共享的材质尽量共享，小道具不要无意义地拆成大量材质。

### 贴图尺寸

不要因为源文件是 4K 就让所有平台都使用 4K。Unity Texture Importer 可以为不同平台设置最大尺寸和压缩格式。

### Read/Write

普通 Mesh 和贴图不需要运行时 CPU 读取时，应关闭不必要的 Read/Write，减少内存副本。

### 骨骼和蒙皮

大量 Skinned Mesh Renderer、骨骼和 Blend Shape 会增加 CPU 与 GPU 成本。远处角色可使用 LOD、减少骨骼或降低更新频率。

### 碰撞体

复杂 Mesh Collider 不仅影响物理性能，也更容易卡住角色。优先使用简单、符合玩法的碰撞形状。

---

## 常见问题与处理方法

### 模型大小不对

检查顺序：

1. Blender Scene Units；
2. 模型真实尺寸；
3. Object Scale 是否为 1；
4. FBX 导出 Scale；
5. Unity Scale Factor 与 Convert Units；
6. Prefab 根节点是否又缩放了一次。

不要只在最后一步随便乘 `0.01`，否则同类资源会继续混乱。

### 模型旋转 90 度或朝向相反

检查：

- Blender 中对象 Rotation 是否应用；
- 导出是否使用统一 Forward 与 Up；
- Unity 是否又额外进行了轴转换；
- ModelRoot 是否重复补偿旋转。

### 材质变白

通常是材质没有映射、贴图没有连接，或 Blender 程序材质无法转换。创建 URP/Lit 材质并手动连接贴图。

### 模型变粉

检查 Shader 与 URP 是否兼容。

### 模型某些面消失

检查法线是否反向，以及是否错误依赖双面显示。

### 法线贴图看起来凹凸反了

检查：

- Unity 中 Texture Type 是否为 Normal Map；
- 贴图使用 DirectX 还是 OpenGL 方向；
- 是否需要翻转绿色通道；
- 模型切线与烘焙时是否一致。

### 材质接缝明显

检查 UV 接缝、法线、切线、贴图边缘 Padding，以及模型是否在烘焙后又改变了三角形结构。

### 门旋转中心错误

回 Blender 修正 Origin，或在 Unity 创建正确位置的 `DoorPivot` 父节点。

### 动画角色扭曲

检查：

- 模型与 Armature 的 Scale；
- 骨骼层级和名称；
- 权重；
- Humanoid Avatar 映射；
- 导出时是否包含不需要的控制骨骼；
- 动画是否与模型使用同一套骨架。

### 角色脚滑

检查：

- 动画是 In-place 还是 Root Motion；
- 脚本速度是否与动画步幅匹配；
- Animator Speed 是否被修改；
- Root Transform 设置；
- Blend Tree 中走路和跑步阈值。

### 重新导入后组件丢失

不要把游戏脚本和碰撞配置只放在 FBX 的内部 Model Prefab 上。创建普通外层 Prefab，把 FBX 作为子物体。

### Unity 中修改模型后，Blender 更新把改动覆盖了

源模型的 Mesh、骨骼和层级修改应回 Blender 完成。Unity 负责材质映射、Collider、Prefab 和游戏组件。明确两边分别负责什么，就不会互相覆盖。

---

## 静态道具完整示例

目标：把 Blender 中的木箱做成 Unity 可推动道具。

### Blender

1. 确认木箱尺寸约为合理游戏尺寸。
2. 应用 Rotation 与 Scale。
3. 把 Origin 放到底部中心。
4. 检查法线。
5. 完成 UV。
6. 导出 Base Color、Normal、Metallic/Smoothness 和 AO。
7. 只选择木箱 Mesh，导出 FBX。

### Unity

1. 导入 FBX 和贴图。
2. 检查模型尺寸、法线和切线。
3. 创建 URP/Lit 木箱材质。
4. 创建空对象 `Crate`。
5. 把 FBX 模型放到 `Crate` 下。
6. 添加 Box Collider。
7. 添加 Rigidbody。
8. 调整质量和阻力。
9. 把根对象制作成 Prefab。
10. 在斜坡、墙角和角色碰撞中测试。

最终结构：

```text
Crate
├─ Rigidbody
├─ BoxCollider
└─ Model
   └─ SM_Crate_A
```

---

## 门模型完整示例

目标：制作一扇可以交互开关的门。

### Blender

1. 门框与门扇分开。
2. 门扇 Origin 放在铰链。
3. 应用 Rotation 与 Scale。
4. 检查门洞尺寸是否允许角色通过。
5. 分别导出，或作为同一 FBX 的两个对象导出。

### Unity

1. 创建 `DoorRoot`。
2. 添加门框模型和 Box Collider。
3. 创建 `DoorPivot`，放在铰链位置。
4. 把门扇模型放到 `DoorPivot` 下。
5. 给门扇添加简单 Box Collider。
6. 通过脚本或 Animator 旋转 `DoorPivot`。
7. 添加交互点和开门脚本。
8. 制作 Prefab。
9. 测试门打开时是否碰到玩家、墙和其他物体。

最终结构：

```text
DoorRoot
├─ FrameModel
├─ FrameColliders
├─ DoorPivot
│  ├─ DoorModel
│  └─ DoorCollider
├─ InteractionPoint
└─ DoorController
```

---

## 角色完整示例

目标：把带 Idle、Walk、Run 动画的人形角色导入 Unity。

### Blender

1. 确认角色高度和朝向。
2. 在绑定前应用 Mesh 和 Armature 的 Rotation 与 Scale。
3. 检查脚底位置和根骨骼。
4. 完成权重。
5. 测试肩、肘、膝等极端动作。
6. 确认 Idle、Walk、Run 的动作范围。
7. 烘焙约束和 IK 结果。
8. 选择 Mesh 与 Armature 导出 FBX。
9. 关闭 Add Leaf Bones。

### Unity

1. Rig 选择 Humanoid。
2. 创建 Avatar 并检查骨骼映射。
3. 在 Animation 页签切分或检查三个 Clip。
4. 为 Idle、Walk、Run 设置正确的 Loop。
5. 创建 Animator Controller 和 Blend Tree。
6. 创建 `PlayerRoot`。
7. 添加 Character Controller 与移动脚本。
8. 把角色模型作为 `ModelRoot` 子物体。
9. 确认脚本移动和动画速度匹配。
10. 制作 Prefab，并在坡道、台阶和不同帧率下测试。

---

## 最终检查清单

### Blender

- [ ] 模型用途明确：静态、机械、角色或碰撞。
- [ ] 单位和实际尺寸正确。
- [ ] Rotation 与 Scale 已正确应用。
- [ ] Origin 位于合理位置。
- [ ] 模型朝向符合项目规范。
- [ ] 没有重复点、内部废面和错误法线。
- [ ] 平滑与硬边正确。
- [ ] UV 和纹理密度合理。
- [ ] 程序材质已经烘焙为游戏贴图。
- [ ] 材质槽数量合理。
- [ ] 导出对象选择正确。
- [ ] FBX 使用统一导出预设。

### 骨骼角色

- [ ] Mesh 与 Armature 比例一致。
- [ ] 权重在极端姿势下正常。
- [ ] 只导出需要的骨骼。
- [ ] Add Leaf Bones 已关闭。
- [ ] IK 和约束已烘焙。
- [ ] 动画帧范围正确。
- [ ] Root Motion 方案明确。

### Unity

- [ ] Scale 为 1 时尺寸正确。
- [ ] 朝向和轴心正确。
- [ ] 法线、切线和 UV 正常。
- [ ] 材质使用 URP 兼容 Shader。
- [ ] Normal Map 和数据贴图导入类型正确。
- [ ] Collider 简单并符合玩法。
- [ ] 动态物体的 Rigidbody 配置合理。
- [ ] FBX 被包装在普通 Prefab 中。
- [ ] 静态模型需要时有 Lightmap UV。
- [ ] 角色 Avatar 和动画循环正常。
- [ ] Read/Write、贴图尺寸和材质槽经过检查。
- [ ] 已在实际 Game 视图和构建版本中测试。

---

## 参考资料

- [Blender Manual：FBX Import/Export](https://docs.blender.org/manual/en/latest/files/import_export/fbx.html)
- [Unity Manual：Importing models](https://docs.unity3d.com/6000.0/Documentation/Manual/ImportingModelFiles.html)
- [Unity Manual：Model Import Settings](https://docs.unity3d.com/6000.0/Documentation/Manual/FBXImporter-Model.html)
- [Unity Manual：Material Import Settings](https://docs.unity3d.com/6000.0/Documentation/Manual/FBXImporter-Materials.html)
- [Unity Manual：Rig Import Settings](https://docs.unity3d.com/6000.0/Documentation/Manual/FBXImporter-Rig.html)
- [Unity Manual：Animation clips](https://docs.unity3d.com/6000.0/Documentation/Manual/AnimationClips.html)
