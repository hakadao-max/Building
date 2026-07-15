# 设置面板（AI 上下文）

- 运行时入口：`SettingsPanelController`。
- Prefab：`Assets/Scenes/Prefab/UI/Settings Panel.prefab`。
- `ResolutionDropdown` 的三个选项调用 `ApplyResolution(int)`，对应 720P、1080P、2K。
- `FullscreenToggle` 调用 `ApplyFullscreen(bool)`，在 `Windowed` 与 `FullScreenWindow` 间切换。
- `CloseGameButton` 绑定 `CloseGame`；Editor 中设置 `EditorApplication.isPlaying = false`，Player 构建中调用 `Application.Quit()`。
- 打开面板时，控制器用当前屏幕尺寸和全屏模式无通知刷新下拉框与 Toggle。
- 设置面板 Prefab 只手工维护；不要恢复 Prefab 生成器或运行时回退 UI。
- `UIManager.WorldCanvasRoot` 对应 `UIRoot/WorldCanvas`；`AddWorldUI` 从直接子物体模板复制实例，`RemoveWorldUI` 只移除由管理器登记的实例。
- World UI 业务组件必须通过 `UIManager` 管理显示生命周期，不要自行扫描模板、实例化或销毁对象。
- `PlayerModeDisplay`、`DetailInspectPanel`、`PlayerInteractionPromptDisplay`、`FixedCameraPanel` 都是 `UIRoot/Canvas` 下的 `UIPanel`，不属于玩家对象。
- 玩家 UI 层级、按钮槽位、十字标和 EventSystem 必须由场景或 Prefab 预置；运行时代码不得创建缺失 UI。
