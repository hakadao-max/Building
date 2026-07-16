# 世界说明 UI

## 目标

`WorldDescriptionUI` 只保存场景物体的标题、说明、显示规则和提示参数。界面结构与视觉样式由场景中的世界空间模板负责；模板实例的添加与移除统一交给 `UIManager`。

## 模板结构

在场景中准备以下层级：

```text
UIRoot
└── WorldCanvas
    └── TipUI
        ├── Title
        └── Description
```

- `WorldCanvas` 是 `UIRoot` 的直接子物体，负责 World Space Canvas 配置。
- `TipUI` 必须是 `RectTransform`，负责面板背景、尺寸、缩放和整体视觉样式。
- `Title` 与 `Description` 必须带 `TMP_Text`，字体、字号、颜色、对齐和换行规则都在模板中配置。
- `TipUI` 必须是 `WorldCanvas` 的直接子物体，并保持禁用，仅作为复制模板。

## 使用方式

给需要说明的场景物体添加 `WorldDescriptionUI`，配置标题、说明、显示偏移、显示规则和 R 键颜色提示参数，并调整同物体 `SphereCollider` 的 Trigger 范围。玩家进入范围后，组件调用 `UIManager.AddWorldUI("TipUI")` 复制模板，把参数应用到新实例并移动到该物体的显示位置；隐藏时调用 `UIManager.RemoveWorldUI(...)` 移除实例。

中心准星射线检测继续用于选择详情目标。启用“仅详情查看时显示”后，玩家必须位于 Trigger 范围内、开启详情查看模式并用准星命中目标；关闭该选项时，进入范围即可显示。R 键提示也由范围内的 `WorldDescriptionUI` 直接响应，不注册活动实例，也不使用 `FindObjectsByType` 扫描场景。

不同说明物体拥有各自的 World UI 实例，可以同时显示，实例生命周期都由 `UIManager` 管理。

## 验证

- 进入 Play Mode 前确认模板层级和节点名称完全匹配。
- 第一人称按 `6` 开启详情查看，进入 Trigger 范围并用准星对准带 `WorldDescriptionUI` 的物体，确认模板显示对应内容。
- 切换目标时确认标题、说明和世界位置同步更新。
- 准星移开、离开 Trigger 范围或关闭详情查看后确认对应的 `TipUI Instance` 被移除。

## 限制

模板查找依赖 `UIRoot/WorldCanvas/TipUI` 固定名称与直接父子层级。需要更换名称或层级时，应同步修改 `UIManager` 根节点约定和 `WorldDescriptionUI` 的模板名。

显示距离由同物体的 `SphereCollider` 半径决定；旧的“最大显示距离”和“距离检测目标”配置已不再使用。
