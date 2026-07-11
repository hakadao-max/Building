# 设置面板

`SettingsPanelController` 管理设置面板显隐、返回出生点、退出游戏、分辨率和全屏状态。`ResolutionDropdown` 提供 1280×720、1920×1080、2560×1440（2K）三档；`FullscreenToggle` 控制窗口模式与无边框全屏模式。两项修改都会通过 `Screen.SetResolution` 立即应用。`CloseGameButton` 调用 `CloseGame`：在 Unity Editor 中停止 Play Mode，在构建后的程序中调用 `Application.Quit` 关闭程序。

正式 UI 直接维护在 `Assets/Scenes/Prefab/UI/Settings Panel.prefab`。设置面板不会再由编辑器脚本重新生成，也不会在运行时创建缺失控件；修改布局时直接编辑该 Prefab，并保持控制器上的分辨率下拉框与全屏开关引用有效。
