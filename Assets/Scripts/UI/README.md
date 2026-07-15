# 设置面板

`SettingsPanelController` 管理设置面板显隐、返回出生点、退出游戏、分辨率和全屏状态。`ResolutionDropdown` 提供 1280×720、1920×1080、2560×1440（2K）三档；`FullscreenToggle` 控制窗口模式与无边框全屏模式。两项修改都会通过 `Screen.SetResolution` 立即应用。`CloseGameButton` 调用 `CloseGame`：在 Unity Editor 中停止 Play Mode，在构建后的程序中调用 `Application.Quit` 关闭程序。

正式 UI 直接维护在 `Assets/Scenes/Prefab/UI/Settings Panel.prefab`。设置面板不会再由编辑器脚本重新生成，也不会在运行时创建缺失控件；修改布局时直接编辑该 Prefab，并保持控制器上的分辨率下拉框与全屏开关引用有效。

## World UI

`UIManager` 同时管理 `UIRoot/WorldCanvas`。World UI 模板作为 `WorldCanvas` 的禁用直接子物体保存；调用 `AddWorldUI(templateName)` 会复制并登记一个显示实例，调用 `RemoveWorldUI(instance)` 会移除该实例。业务组件不得绕过 `UIManager` 自行实例化或销毁 World UI。

## 玩家 UI 面板

玩家 HUD 必须预先放在 `UIRoot/Canvas` 下，并绑定对应 `UIPanel`：

- `PlayerModePanel`：挂 `PlayerModeDisplay`，绑定模式 TMP 文本。
- `DetailInspectPanel`：挂 `DetailInspectPanel`，在层级中预先制作十字标。
- `InteractionPromptPanel`：挂 `PlayerInteractionPromptDisplay`，绑定交互提示 TMP 文本。
- `FixedCameraPanel`：挂 `FixedCameraPanel`，预先配置足够数量的固定视角按钮以及空状态文字。

场景还必须预置正确输入模块的 `EventSystem`。玩家脚本只通过 `UIManager` 显示、隐藏和更新面板，不会创建 Canvas、文本、图片、按钮、十字标或 EventSystem。固定视角与预制体交换功能只使用面板内已经存在的按钮槽位，槽位不足时输出警告，不会动态补按钮。
