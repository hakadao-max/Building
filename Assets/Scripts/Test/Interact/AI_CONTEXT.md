# MTInteractObj AI Context

## 职责

`MTInteractObj` 负责测试模式下漂浮物的 Kinematic 移动、持有者碰撞忽略、障碍物扫描和沿墙移动。

## 关键路径

- 运行时组件：`Assets/Scripts/Test/Interact/MTInteractObj.cs`
- 目标位置来源：`Assets/Scripts/Test/View/MTInteractViewMode.cs`
- 验证场景：`Assets/Scenes/Clean1.unity`

## 数据流

1. `MTInteractViewMode` 调用 `BeginInteract`。
2. 每个 Tick 使用 `MTCameraController` 本帧的 Pitch/Yaw 计算目标方向；不要读取 LateUpdate 前的旧 `transform.forward`。
3. 同一个 Tick 立即调用 `MoveWithCollision(targetPosition)`，由 `Rigidbody.SweepTest` 解析可移动距离并直接设置 Kinematic Rigidbody 的世界位置。
4. 本帧命中墙面后，先移动到墙前，再将剩余位移投影到墙面；不缓存法线，下一帧重新按完整目标方向扫描。
5. 最终位置同时写入 Kinematic Rigidbody 和 Transform，立即同步物理状态与渲染状态。
6. `EndInteract` 恢复刚体状态和持有者碰撞。

## 维护约束

- 漂浮期间只通过 `MTInteractObj` 设置 Kinematic Rigidbody 的 `position`，不要从其他脚本再次写入 Transform 或 Rigidbody。
- Rigidbody 插值只在交互期间强制为 `None`，结束时必须恢复原值。
- 不要跨帧缓存墙面法线；必须先扫描本帧完整位移，命中后再计算切向分量。
- `SetColliderIgnored(false)` 必须在清空持有者 Collider 引用之前执行。
- 若要支持初始重叠恢复，需要单独增加 `Physics.ComputePenetration` 逻辑。
