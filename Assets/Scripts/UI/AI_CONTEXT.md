# 设置面板（AI 上下文）

- 运行时入口：`SettingsPanelController`。
- Prefab：`Assets/Scenes/Prefab/UI/Settings Panel.prefab`。
- `ResolutionDropdown` 的三个选项调用 `ApplyResolution(int)`，对应 720P、1080P、2K。
- `FullscreenToggle` 调用 `ApplyFullscreen(bool)`，在 `Windowed` 与 `FullScreenWindow` 间切换。
- `CloseGameButton` 绑定 `CloseGame`；Editor 中设置 `EditorApplication.isPlaying = false`，Player 构建中调用 `Application.Quit()`。
- 打开面板时，控制器用当前屏幕尺寸和全屏模式无通知刷新下拉框与 Toggle。
- 设置面板 Prefab 只手工维护；不要恢复 Prefab 生成器或运行时回退 UI。
