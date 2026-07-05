# AI Context: 设置面板

一句话接入：执行 `工具/创建设置面板 Prefab`，将 `Assets/Prefab/UI/Settings Panel.prefab` 拖入场景。

## 关键类

- `SettingsPanelController`：只切换已序列化的 `panelRoot`，并绑定 `returnSpawnButton` / `closeButton`。不得恢复 `Resources.Load`、运行时 Canvas 或运行时 EventSystem 生成。
- `SettingsPanelPrefabCreator`：编辑器阶段生成固定面板 Prefab，其中包含 Canvas、EventSystem、两个按钮和已绑定的控制器。

## 数据流

1. `Escape` 调用 `TogglePanel()`。
2. 打开面板时调用 `SimplePlayerController.SetExternalInputLocked(true)` 并释放鼠标。
3. 回出生点按钮调用 `TeleportTo()` 或 `ReturnToSpawn()` 后关闭面板。
4. 退出按钮只关闭面板并恢复玩家输入。
