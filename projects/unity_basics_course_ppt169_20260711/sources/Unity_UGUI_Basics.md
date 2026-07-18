# Unity_UGUI_Basics

# Unity UGUI 基础

目标：使用 Unity 的 UGUI 做一个游戏开始界面，包含背景、标题、按钮、Slider、Toggle、菜单 切换、参数绑定和基础主题样式。

## 一、项目设置

- 

Game 视图选择 16:9，常用参考分辨率为 1920 x 1080，便于和课程截图、素材尺寸统一。

- 

创建 Canvas 后，设置 Canvas Scaler：UI Scale Mode = Scale With Screen Size。

- 

Reference Resolution = 1920 x 1080，Screen Match Mode = Match Width Or Height，Match = 0.5。

- 

确认场景中存在 EventSystem；没有它，按钮、Slider、Toggle 等交互控件无法接收事件。

- 

推荐使用 TextMeshPro 文本：第一次使用时导入 TMP Essentials。

Canvas Background        Image MenuPanel         RectTransform Logo            Image Title           TextMeshProUGUI PlayButton      Button SettingsButton  Button QuitButton      Button SettingsPanel     RectTransform SpeedSlider     Slider FullscreenToggle Toggle

## 二、游戏开始界面

### 1. 添加 UI 图标和基础控件

- 

Canvas：所有 UGUI 控件的根容器。常用 Render Mode 为 Screen Space - Overlay。

- 

RectTransform：UI 物体的位置、尺寸、锚点、中心点。布局问题通常先检查它。

- 

Image：显示背景、图标、按钮底图。RawImage：直接显示 Texture。

- 

TextMeshProUGUI：显示标题、说明、分数、按钮文字。

- 

Layout Group：让子物体按规则排列，减少手动对齐。

- 

Vertical Layout Group：纵向排列。

- 

Horizontal Layout Group：横向排列。

- 

Grid Layout Group：网格排列。

- 

Content Size Fitter / Layout Element：控制自适应尺寸和最小尺寸。

Unity UGUI 基础 1

<!-- Page 2 -->

- 

常见交互控件：Button、Slider、Toggle、Dropdown、Input Field。

- 

做菜单时，建议先用空物体 Panel 分组，再在 Panel 下放按钮和文本。

**to do : 创建 Canvas、MenuPanel、Title、PlayButton、SettingsButton、QuitButton，并调整** **锚点。**

### 2. 为交互控件设置功能

- 

建议在层级最高的 UI 父物体上挂一个 UIManager 脚本。

- 

用[SerializeField]引用按钮、面板、Slider、Toggle，避免到处使用 Find。

- 

常见事件：Button.onClick、Slider.onValueChanged、Toggle.onValueChanged。

- 

显示/隐藏：gameObject.SetActive(true)或panel.SetActive(false)。

using UnityEngine;

using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour { [SerializeField] GameObject menuPanel;

[SerializeField] GameObject settingsPanel;

[SerializeField] Button playButton;

[SerializeField] Button settingsButton;

[SerializeField] Button quitButton;

void Awake() { playButton.onClick.AddListener(OnPlayPressed);

settingsButton.onClick.AddListener(OpenSettings);

quitButton.onClick.AddListener(QuitGame);

} void Start() { menuPanel.SetActive(true);

settingsPanel.SetActive(false);

} void OnPlayPressed() { menuPanel.SetActive(false);

} void OpenSettings() { settingsPanel.SetActive(true);

} void QuitGame() { Application.Quit();

} } **to do : 为 Play、Settings、Quit 三个 Button 设置相应功能。** Unity UGUI 基础 2

<!-- Page 3 -->

### 3. 退出游戏与全屏切换

- 

Application.Quit()在打包后的游戏中生效；在编辑器播放模式下不会真正关闭编辑器。

- 

切换全屏：Screen.fullScreen = true或Screen.SetResolution(width, height, fullscreen)。

- 

Toggle 的onValueChanged会传入 bool，适合控制全屏、静音、显示辅助线等开关。

[SerializeField] Toggle fullscreenToggle;

void Awake() { fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

} void SetFullscreen(bool isFullscreen) { Screen.fullScreen = isFullscreen;

} **to do : 为 FullscreenToggle 设定切换全屏功能。**

### 4. 用 Slider 调节游戏参数

- 

Slider 适合调节音量、速度、灵敏度、初始生命值等连续数值。

- 

在 Inspector 中设置 Min Value、Max Value、Whole Numbers。

- 

用脚本引用需要被调节的对象，例如 BallController。

using TMPro;

using UnityEngine;

using UnityEngine.UI;

public class SettingsUI : MonoBehaviour { [SerializeField] Slider speedSlider;

[SerializeField] TMP_Text speedText;

[SerializeField] BallController ball;

void Awake() { speedSlider.onValueChanged.AddListener(SetBallSpeed);

} void Start() { SetBallSpeed(speedSlider.value);

} void SetBallSpeed(float value) { ball.SetInitialSpeed(value);

speedText.text = value.ToString("0.0");

} } **to do : 在 UI 界面调节小球初始速度 ballInitialSpeed，并把数值显示到文本。** Unity UGUI 基础 3

<!-- Page 4 -->

### 5. 开始游戏与返回菜单

- 

游戏启动后显示菜单，隐藏游戏 HUD。按 Play 后隐藏菜单，显示游戏内容。

- 

返回菜单可以只切换 Panel，也可以加载另一个 Scene；初学阶段先用 Panel 切换。

- 

如果要暂停游戏，可使用Time.timeScale = 0f，恢复时设回1f。

**to do : 游戏启动后显示 Menu 按钮，按下后回到 UI 菜单界面。**

## 三、变得更好看吧

### 1. Logo、背景和九宫格按钮

- 

Logo：用 Image 显示 Sprite，保持 Preserve Aspect，避免拉伸变形。

- 

背景：Image 铺满 Canvas，放在层级最上方或使用 Sorting Order 控制显示顺序。

- 

九宫格按钮：Sprite Editor 设置 Border 后，Image Type 选择 Sliced，类似 Godot 的 NinePatchRect。

### 2. Container 与布局

- 

Vertical Layout Group 控制按钮纵向排列，Spacing 控制间距，Padding 控制边距。

- 

Layout Element 的 Min Width / Min Height 可以统一按钮大小。

- 

RectTransform 的 Anchor 决定窗口缩放时 UI 如何跟随屏幕。

- 

Canvas Scaler 负责整体缩放，Layout Group 负责局部排列，两者要配合使用。

### 3. 主题与交互状态

- 

Button Transition 可以选择 Color Tint、Sprite Swap、Animation。

- 

常见状态：Normal、Highlighted、Pressed、Selected、Disabled。

- 

TextMeshPro 的 Font Asset、字号、字距、描边和阴影决定文字质感。

- 

把调好的按钮做成 Prefab，之后所有菜单按钮都从它复制。

**to do : 调节所有 Button 在 Highlighted、Pressed、Disabled 状态下的颜色或图片。**

## 四、UGUI 框架核心概念

### 1. Canvas、Graphic Raycaster、EventSystem

- 

Canvas 负责把 UI 画到屏幕上。Canvas 过多或频繁重建会影响性能。

- 

Graphic Raycaster 负责检测鼠标或触摸点到了哪个 UI 图形。

- 

EventSystem 负责把点击、拖拽、选择等事件分发给控件。

- 

不需要响应点击的 Image 和 Text，可以关闭 Raycast Target，减少误挡按钮。

### 2. 常用脚本接口

Unity UGUI 基础 4

<!-- Page 5 -->

- 

Button.onClick.AddListener(MethodName)：绑定按钮点击。

- 

Slider.onValueChanged.AddListener(MethodName)：绑定数值变化。

- 

Toggle.onValueChanged.AddListener(MethodName)：绑定开关变化。

- 

TMP_Text.text = "Score: 0"：更新文字。

- 

CanvasGroup.alpha、interactable、blocksRaycasts：控制淡入淡出和交互。

[SerializeField] CanvasGroup menuGroup;

void ShowMenu(bool visible) { menuGroup.alpha = visible ? 1f : 0f;

menuGroup.interactable = visible;

menuGroup.blocksRaycasts = visible;

}

## 五、课堂练习顺序

- 

step 1：搭建 Canvas、MenuPanel、SettingsPanel，并设置 Canvas Scaler。

- 

step 2：完成 Play、Settings、Back、Quit 的按钮交互。

- 

step 3：用 Slider 修改 BallController 的初始速度。

- 

step 4：用 Toggle 切换全屏或静音。

- 

step 5：使用 Layout Group 统一按钮大小和间距。

- 

step 6：设置按钮不同状态的颜色、图片和字体，制作可复用 UI Prefab。

Unity UGUI 基础 5

<!-- Page 6 -->

## 附：Godot UI 到 Unity UGUI 对照

|Godot UI|Unity UGUI|说明|
|---|---|---|
|Control|RectTransform + Graphic|Unity UI 物体通常都有 RectTransform，显<br>示部分来自 Image、Text、RawImage 等<br>组件。|
|CenterContainer /<br>VBoxContainer|Layout Group|Unity 使用 Vertical、Horizontal、Grid<br>Layout Group 管理子物体排列。|
|Button.pressed signal|Button.onClick|按钮点击事件，可在 Inspector 或脚本中<br>绑定。|
|Slider.value changed<br>_|Slider.onValueChanged|Slider 数值变化事件，回调参数是 float。|
|CheckBox.toggled|Toggle.onValueChanged|Toggle 开关变化事件，回调参数是 bool。|
|Theme Overrides|Button Transition / TMP / Prefab|Unity 常用组件状态、字体资源和 Prefab<br>统一 UI 风格。|
Unity UGUI 基础 6
