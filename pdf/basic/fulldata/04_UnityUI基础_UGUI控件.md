# Unity UI 基础：Canvas、EventSystem 与常用 UGUI 控件

---

## UGUI 的基本结构

一个简单设置面板的层级可能是：

```text
Canvas
├─ Background (Image)
├─ StartButton (Button)
│  └─ Label
├─ MusicToggle (Toggle)
├─ QualityDropdown (Dropdown / TMP_Dropdown)
└─ CameraPreview (RawImage)
EventSystem
```

核心关系：

```text
Canvas 负责布局与渲染空间
EventSystem 负责分发输入事件
控件负责显示、状态和交互
脚本或 Inspector 事件负责真正的游戏行为
```

---

## Canvas：UI 的根容器

所有 UGUI 可视控件都必须位于 Canvas 下。创建第一个 UI 控件时，Unity 通常会自动创建 Canvas 和 EventSystem。

### RectTransform

UI 对象使用 `RectTransform`，它继承自 `Transform`，并增加矩形布局能力：

- Anchors：锚点，决定元素相对父矩形如何定位；
- Pivot：轴心，决定旋转、缩放和位置参考点；
- Anchored Position：相对锚点的位置；
- Width / Height 或四边偏移；
- Size Delta：相对锚点范围的尺寸变化。

UI 适配的关键通常不是“手动写一个固定坐标”，而是把锚点放在正确位置。

### Screen Space - Overlay

Overlay Canvas 直接覆盖在游戏画面最上层。

适合：

- 主菜单；
- 血条、分数、任务提示；
- 暂停界面；
- 设置面板。

特点：

- 不需要指定渲染摄像机；
- 一般不会被 3D 场景物体遮挡；
- UI 尺寸主要由屏幕和 Canvas Scaler 决定；
- 最适合普通屏幕 HUD。

基本设置：

1. 选中 Canvas。
2. Render Mode 选择 `Screen Space - Overlay`。
3. 添加或检查 `Canvas Scaler`。
4. 常用 UI Scale Mode 选择 `Scale With Screen Size`。
5. 设置 Reference Resolution，例如 `1920 × 1080`。
6. Match 值决定宽度和高度适配的权衡。

### World Space

World Space Canvas 是场景中的真实平面，可以像其他物体一样移动、旋转和缩放。

适合：

- 世界中的电脑屏幕；
- 角色头顶血条；
- 建筑标牌；
- 3D 控制面板；
- VR/AR 空间界面。

特点：

- 有真实世界位置和尺寸；
- 会受摄像机距离、角度和遮挡影响；
- 需要合理缩放，否则会大得遮住场景；
- Event Camera、Graphic Raycaster 和 EventSystem 配置会影响点击。

设置步骤：

1. Canvas 的 Render Mode 选择 `World Space`。
2. 调整 RectTransform 的宽高，例如 `800 × 450`。
3. 把整体 Scale 调小，例如从 `0.001` 附近开始试验，具体数值取决于场景单位和设计尺寸。
4. 把 Canvas 放到目标物体前方，确认正面朝向摄像机。
5. 在 Canvas 的 Event Camera 中指定用于交互的摄像机。
6. 保留 `Graphic Raycaster`。
7. 确认 EventSystem 的输入模块和项目输入系统匹配。

### Overlay 与 World Space 对比

| 对比项 | Screen Space - Overlay | World Space |
| --- | --- | --- |
| 所在空间 | 屏幕 | 3D 世界 |
| 是否需要渲染摄像机 | 通常不需要 | 通常需要 Event Camera |
| 是否受距离影响 | 否 | 是 |
| 是否可能被场景遮挡 | 通常不会 | 会 |
| 典型用途 | 菜单、HUD | 世界面板、头顶 UI |
| 主要适配重点 | 分辨率与锚点 | 世界尺寸、朝向和交互射线 |

### Canvas Scaler

Overlay UI 常用 `Scale With Screen Size`：

- Reference Resolution：设计基准分辨率；
- Screen Match Mode：如何处理不同宽高比；
- Match：0 偏向按宽度匹配，1 偏向按高度匹配，中间值折中。

课堂演示可切换 Game 视图宽高比，观察：

- 锚点在中心的控件；
- 锚点在右上角的控件；
- 横向拉伸的背景；
- 没有正确锚定时漂移的控件。

---

## EventSystem：UI 输入事件的调度中心

EventSystem 不负责把按钮画出来，而是负责：

- 当前选中哪个 UI；
- 鼠标指针进入、离开、按下、松开和点击；
- 键盘、手柄的导航与提交；
- 把输入模块生成的事件发送给对应控件。

### 一个场景通常只保留一个 EventSystem

多个 EventSystem 可能导致重复输入、选中状态混乱或警告。场景叠加加载时尤其要检查是否重复。

### 输入模块

本项目使用新输入系统，EventSystem 通常应使用：

```text
Input System UI Input Module
```

使用旧输入管理器的项目常见 `Standalone Input Module`。输入模块必须与 Player Settings 中启用的输入系统相匹配。

### Graphic Raycaster

Canvas 上的 `Graphic Raycaster` 负责检测指针命中了哪个 UI Graphic。EventSystem、输入模块和 Graphic Raycaster 缺一都可能导致 UI 无法点击。

### UI 看得见但点不到

按以下顺序排查：

1. 场景中是否有启用的 EventSystem？
2. 输入模块是否与项目输入系统匹配？
3. Canvas 是否有 Graphic Raycaster？
4. 控件的 `Interactable` 是否开启？
5. 控件或父对象的 `CanvasGroup` 是否阻止交互？
6. 前方是否有透明 Image，且它的 `Raycast Target` 仍然开启？
7. World Space Canvas 是否设置正确 Event Camera？
8. 摄像机是否能看到该 Canvas 所在 Layer？
9. UI 是否被另一个 Canvas 或控件挡住？

---

## Button：执行一次明确操作

Button 用于点击后执行命令，例如：

- 开始游戏；
- 打开面板；
- 确认选择；
- 切换场景；
- 调用角色技能。

### 重要属性

- Interactable：是否可交互；
- Transition：状态变化的视觉方式；
- Target Graphic：接收颜色或 Sprite 变化的图形；
- Navigation：键盘或手柄如何在控件之间移动；
- On Click()：成功点击后执行的事件。

### Transition

常见模式：

- Color Tint：不同状态使用不同颜色；
- Sprite Swap：不同状态替换 Sprite；
- Animation：用 Animator 控制状态；
- None：无自动视觉变化。

至少应让玩家能分辨：普通、悬停、按下、禁用。

### 在 Inspector 中连接事件

1. 选中 Button。
2. 在 On Click() 中点击 `+`。
3. 把包含目标脚本的对象拖入引用槽。
4. 从函数列表选择一个 `public` 方法。

```csharp
public void StartGame()
{
    Debug.Log("开始游戏");
}
```

优点是直观；缺点是连接关系藏在 Inspector 中，重构和复制时要仔细检查。

### 在脚本中监听

```csharp
using UnityEngine;
using UnityEngine.UI;

public class StartPanel : MonoBehaviour
{
    [SerializeField] private Button startButton;

    void OnEnable()
    {
        startButton.onClick.AddListener(StartGame);
    }

    void OnDisable()
    {
        startButton.onClick.RemoveListener(StartGame);
    }

    private void StartGame()
    {
        Debug.Log("开始游戏");
    }
}
```

在 `OnEnable` 添加的监听，应在 `OnDisable` 移除，避免重复订阅。

### Button 设计注意事项

- 文案描述结果，例如“保存设置”，不要只写模糊的“确定”；
- 点击区域要足够大；
- 禁用按钮时说明原因；
- 键盘和手柄用户需要清楚的选中状态；
- 破坏性操作应有确认或撤销机制。

---

## Image：显示 Sprite 的基础图形

Image 用于显示 Sprite，也常被 Button、Toggle 和面板背景使用。

### 重要属性

- Source Image：要显示的 Sprite；
- Color：与 Sprite 颜色相乘；
- Material：可选的 UI 材质；
- Raycast Target：是否阻挡 UI 射线；
- Maskable：是否受 UI Mask 影响；
- Image Type：Simple、Sliced、Tiled 或 Filled；
- Preserve Aspect：保持原图宽高比。

### 四种 Image Type

#### Simple

整张 Sprite 拉伸到 RectTransform。适合图标和不介意拉伸的图片。

#### Sliced

使用九宫格切片，四角保持形状，边和中心拉伸。适合按钮、面板和对话框背景。

Sprite 需要在 Sprite Editor 中设置 Border。

#### Tiled

平铺纹理填满矩形。适合重复图案。

#### Filled

按百分比显示，可做血条、技能冷却和进度环。

```csharp
[SerializeField] private Image healthFill;

public void SetHealth(float current, float maximum)
{
    healthFill.fillAmount = maximum > 0f ? current / maximum : 0f;
}
```

### Raycast Target

纯装饰 Image 通常可以关闭 `Raycast Target`。否则透明图片也可能挡住后面的按钮。

注意：关闭 Button 的目标 Image 的 Raycast Target 可能让 Button 无法接收指针。应区分“控件接收区”和“装饰图片”。

---

## RawImage：直接显示 Texture

Image 主要显示 Sprite，RawImage 直接显示 Texture。

适合：

- Render Texture 摄像机画面；
- 视频纹理；
- 运行时生成的 Texture2D；
- 不想导入为 Sprite 的图片。

### 重要属性

- Texture：要显示的纹理；
- Color：整体颜色；
- Material：可选 UI 材质；
- UV Rect：显示纹理的哪一部分、是否平铺；
- Raycast Target：是否接收或阻挡 UI 射线。

### 制作监控画面

1. 创建 Render Texture 资源。
2. 创建一台监控 Camera。
3. 把 Camera 的 Target Texture 设置为该 Render Texture。
4. 创建 RawImage。
5. 把同一个 Render Texture 指定给 RawImage 的 Texture。
6. 调整 RawImage 宽高比，避免画面变形。

### Image 与 RawImage 对比

| 对比项 | Image | RawImage |
| --- | --- | --- |
| 输入资源 | Sprite | Texture |
| 九宫格 | 支持 | 不支持 |
| Filled | 支持 | 不支持 |
| 常见用途 | 图标、按钮、面板、血条 | 摄像机画面、视频、动态纹理 |

RawImage 通常会产生额外纹理批次，不应为了普通图标而大量替代 Image。

---

## Toggle：布尔选项

Toggle 表示开/关、是/否等二元状态，例如：

- 音乐开启；
- 全屏模式；
- 显示字幕；
- 接受条款。

### 重要属性

- Is On：当前是否开启；
- Toggle Transition：勾选图形的过渡方式；
- Graphic：开关标记图形；
- Target Graphic：底图；
- Group：所属 Toggle Group；
- On Value Changed (Boolean)：值变化时触发。

### 脚本监听

```csharp
using UnityEngine;
using UnityEngine.UI;

public class AudioToggleView : MonoBehaviour
{
    [SerializeField] private Toggle musicToggle;

    void OnEnable()
    {
        musicToggle.onValueChanged.AddListener(SetMusicEnabled);
    }

    void OnDisable()
    {
        musicToggle.onValueChanged.RemoveListener(SetMusicEnabled);
    }

    private void SetMusicEnabled(bool isEnabled)
    {
        AudioListener.volume = isEnabled ? 1f : 0f;
    }
}
```

### Toggle Group

多个 Toggle 放入同一个 Toggle Group 后，可形成单选效果，例如难度选择或角色选择。

注意：如果选项天然是“从多个值中选一个”，Dropdown 或单选 Toggle Group 通常比多个互不关联的 Toggle 更合适。

### 初始化时避免误触发

设置初值可能触发事件。如果只想更新显示而不调用监听器，可以使用：

```csharp
musicToggle.SetIsOnWithoutNotify(savedValue);
```

---

## Dropdown：从列表中选择一项

Dropdown 适合从多个互斥选项中选一个，例如：

- 画质等级；
- 分辨率；
- 语言；
- 服务器区域。

项目中常见两种实现：

- `UnityEngine.UI.Dropdown`：传统 UGUI Dropdown；
- `TMPro.TMP_Dropdown`：使用 TextMeshPro 文本，通常更适合现代项目。

两者概念基本一致。本项目已安装 TextMeshPro，课堂项目可优先使用 `TMP_Dropdown`。

### 重要结构

一个 Dropdown 通常包含：

- Caption：当前选项显示；
- Arrow：下拉箭头；
- Template：展开列表模板；
- Viewport：可见区域；
- Content：选项容器；
- Item：单个选项模板。

不要随意破坏 Template 的引用关系。修改层级后要检查 Dropdown 组件中的 Template、Caption Text、Item Text 等引用。

### 在 Inspector 中添加选项

在 Options 列表中添加文本和可选图片，`Value` 表示当前选项索引，从 0 开始。

### 使用 TMP_Dropdown

```csharp
using TMPro;
using UnityEngine;

public class QualityDropdownView : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown qualityDropdown;

    void Awake()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "低",
            "中",
            "高"
        });

        int currentIndex = Mathf.Clamp(
            QualitySettings.GetQualityLevel(),
            0,
            qualityDropdown.options.Count - 1
        );

        qualityDropdown.SetValueWithoutNotify(currentIndex);
        qualityDropdown.RefreshShownValue();
    }

    void OnEnable()
    {
        qualityDropdown.onValueChanged.AddListener(SetQuality);
    }

    void OnDisable()
    {
        qualityDropdown.onValueChanged.RemoveListener(SetQuality);
    }

    private void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
    }
}
```

> 实际项目应确保选项数量和 Quality Settings 中的等级数量一致。若显示顺序与内部等级不同，应建立明确映射，不能直接假设索引永远相同。

### Dropdown 设计注意事项

- 选项较少且需要同时比较时，Toggle Group 可能更直观；
- 选项文案应简短、互斥、无歧义；
- 展开方向不能超出屏幕；
- World Space Canvas 中要检查列表是否被其他几何体遮挡；
- 手柄操作时要验证展开、移动、确认和取消。

---

## 综合实践：设置面板

### 目标层级

```text
SettingsCanvas (Screen Space - Overlay)
└─ SettingsPanel (Image)
   ├─ Preview (RawImage)
   ├─ MusicToggle (Toggle)
   ├─ QualityDropdown (TMP_Dropdown)
   └─ ApplyButton (Button)
EventSystem
```

### 功能目标

- Image 作为面板背景；
- RawImage 显示角色预览摄像机；
- Toggle 控制音乐；
- Dropdown 选择画质；
- Button 应用并输出当前设置。

### 控制脚本

```csharp
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Button applyButton;

    private bool pendingMusicEnabled;
    private int pendingQualityIndex;

    void Awake()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string> { "低", "中", "高" });
    }

    void OnEnable()
    {
        musicToggle.onValueChanged.AddListener(OnMusicChanged);
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        applyButton.onClick.AddListener(ApplySettings);

        pendingMusicEnabled = AudioListener.volume > 0f;
        pendingQualityIndex = Mathf.Clamp(
            QualitySettings.GetQualityLevel(),
            0,
            qualityDropdown.options.Count - 1
        );

        musicToggle.SetIsOnWithoutNotify(pendingMusicEnabled);
        qualityDropdown.SetValueWithoutNotify(pendingQualityIndex);
        qualityDropdown.RefreshShownValue();
    }

    void OnDisable()
    {
        musicToggle.onValueChanged.RemoveListener(OnMusicChanged);
        qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
        applyButton.onClick.RemoveListener(ApplySettings);
    }

    private void OnMusicChanged(bool isEnabled)
    {
        pendingMusicEnabled = isEnabled;
    }

    private void OnQualityChanged(int index)
    {
        pendingQualityIndex = index;
    }

    private void ApplySettings()
    {
        AudioListener.volume = pendingMusicEnabled ? 1f : 0f;
        QualitySettings.SetQualityLevel(pendingQualityIndex, true);

        Debug.Log(
            $"已应用设置：音乐={pendingMusicEnabled}，画质索引={pendingQualityIndex}"
        );
    }
}
```

### 课堂讲解重点

1. View 只收集待应用状态，不必每次点击控件都立即修改全局设置。
2. 使用 `SetIsOnWithoutNotify` 和 `SetValueWithoutNotify` 初始化，避免加载面板时误触发逻辑。
3. `OnEnable` 订阅，`OnDisable` 取消订阅。
4. Inspector 引用比运行时按名称查找更稳定。
5. 真正项目还应保存设置，并处理“应用失败”“恢复默认”“取消修改”等状态。

---

## World Space 交互实践：墙上的控制面板

### 搭建步骤

1. 创建 World Space Canvas，放到墙面前方。
2. 添加背景 Image、开灯 Toggle 和开门 Button。
3. 设置 Canvas 尺寸与世界缩放。
4. 把 Canvas 的 Event Camera 指向玩家摄像机。
5. 确认场景中有 EventSystem 和 Input System UI Input Module。
6. 使用鼠标时，普通 UI 指针事件即可工作。
7. 第一人称锁定鼠标时，可在屏幕中心做射线交互，再主动选择或调用面板控件；这属于更完整的 3D 交互系统。

### 可读性与距离

- 文字大小要按玩家实际观看距离测试；
- 不要只在 Scene 视图近距离看起来清楚；
- 面板应有足够对比度；
- 交互状态要有声音、颜色或动画反馈；
- 玩家走远或转身时，应明确面板是否仍保持焦点。

---

## 常见问题速查

### UI 跟随分辨率变形

检查 Canvas Scaler、Reference Resolution、Match 和锚点。不要只为一个分辨率手动摆放。

### 按钮没有反应

检查 EventSystem、输入模块、Graphic Raycaster、Interactable、Raycast Target 和遮挡顺序。

### 点击一次执行多次

很可能重复调用了 `AddListener`，但没有相应 `RemoveListener`；也可能场景中存在重复 EventSystem 或重复脚本实例。

### 透明图片挡住按钮

关闭纯装饰 Image 的 `Raycast Target`，或调整层级顺序。

### RawImage 是黑色

检查 Camera 是否启用、Target Texture 是否一致、Culling Mask 是否包含目标 Layer，以及 Render Texture 是否与当前平台兼容。

### World Space Canvas 无法点击

检查 Event Camera、Graphic Raycaster、EventSystem 输入模块、Canvas 朝向和摄像机 Layer。

### Dropdown 展开后选项不可见

检查 Template 是否启用方式正确、层级引用是否断开、RectMask/Viewport 是否裁掉内容，以及列表是否超出 Canvas。

### Toggle 打开面板时自动执行逻辑

初始化时改用 `SetIsOnWithoutNotify`。

---

## 课堂练习

### 练习 1：Overlay 主菜单

创建全屏背景 Image 和三个 Button：开始、设置、退出。要求：

- 不同宽高比下按钮保持居中；
- 悬停、按下、禁用状态清晰；
- 点击时输出不同日志。

### 练习 2：血条 Image

使用 Filled Image 制作血条，并编写：

```csharp
public void SetHealth(float current, float maximum)
```

测试 100%、50%、0% 和最大值为 0 的情况。

### 练习 3：监控 RawImage

创建第二台摄像机和 Render Texture，在 UI 中显示场景另一个角度。

### 练习 4：设置面板

使用 Toggle 控制音乐，Dropdown 控制画质，Button 负责应用。要求面板重复打开时不会重复触发监听器。

### 练习 5：World Space 门禁面板

在场景墙面放置 World Space Canvas，通过 Button 开关门，通过 Toggle 开关灯。测试近距离、远距离和斜角观看。

---

## 验收清单

- [ ] Canvas 模式符合用途：HUD 使用 Overlay，场景面板使用 World Space。
- [ ] Overlay Canvas 配置了合理的 Canvas Scaler。
- [ ] UI 锚点能适配至少三种宽高比。
- [ ] 场景中只有一个有效 EventSystem。
- [ ] 输入模块与新输入系统匹配。
- [ ] 纯装饰 Image 不阻挡射线。
- [ ] Button 有普通、悬停、按下和禁用反馈。
- [ ] RawImage 的纹理和宽高比正确。
- [ ] Toggle 初值设置不会误触发事件。
- [ ] Dropdown 展开、选择和关闭均正常。
- [ ] World Space Canvas 设置了正确的 Event Camera。
- [ ] 脚本监听器有成对的添加和移除。

---

## 本课小结

- Canvas 决定 UI 所在的渲染与布局空间。
- Overlay 适合菜单和 HUD，World Space 适合场景内面板。
- EventSystem、输入模块和 Graphic Raycaster 共同让 UI 接收输入。
- Button 表示一次命令，Toggle 表示布尔状态，Dropdown 表示多选一。
- Image 适合 Sprite、九宫格和填充效果，RawImage 适合 Texture 与 Render Texture。
- 好的 UI 不只“能点”，还要适配分辨率、提供状态反馈、支持键盘与手柄，并能清楚解释操作结果。

## 参考资料

- [Unity UGUI Manual](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/index.html)
- [Canvas](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/UICanvas.html)
- [Event System](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/EventSystem.html)
- [Button](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-Button.html)
- [Image](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-Image.html)
- [Raw Image](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-RawImage.html)
- [Toggle](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-Toggle.html)
- [Dropdown](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-Dropdown.html)
