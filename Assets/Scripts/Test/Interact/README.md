# 测试交互物体

## 目标

`MTInteractObj` 用于第一人称测试模式中的物体漂浮交互。交互期间物体由 Kinematic Rigidbody 驱动，能够在障碍物前停止，并在持续朝墙移动时保留沿墙方向的位移。

## 使用方式

1. 在可交互物体上添加 `Rigidbody`、至少一个非 Trigger Collider 和 `MTInteractObj`。
2. 配置漂浮距离与碰撞间隙；碰撞间隙建议从 `0.02` 开始调整。
3. `MTInteractViewMode` 通过 `BeginInteract`、`MoveWithCollision` 和 `EndInteract` 驱动物体。

## 运行行为

- `MTInteractViewMode.Tick` 得到本帧目标位置后立即扫描并更新刚体位置，不再跨 `Update`、`FixedUpdate` 或 `LateUpdate` 缓存目标坐标。
- 目标方向直接使用本帧输入得到的 Pitch/Yaw，避免读取 LateUpdate 前的上一帧相机朝向。
- 临时忽略漂浮物与持有者 Collider 的碰撞。
- 每帧先按完整目标方向执行 `SweepTest`；仅在本帧命中时移除剩余位移中进入墙面的分量，实现沿墙移动，不缓存旧墙面法线。
- 最终位置同时写入 Kinematic Rigidbody 和 Transform，使物理查询位置与当前渲染帧的显示位置立即一致。
- 结束交互时恢复交互前的 Kinematic、插值和碰撞忽略状态。

## 验证

在 `Assets/Scenes/Clean1.unity` 进入 Play Mode：

- 在空地移动和旋转视角，物体应连续跟随。
- 斜向靠近墙壁，物体应停止向墙内移动并沿墙滑动。
- 放下物体后，物体应重新与玩家发生正常碰撞。

## 限制

- 漂浮物开始交互时不应已经嵌入障碍物；`SweepTest` 不负责修复初始重叠。
- 当前单次扫描处理一个阻挡平面；进入由多个墙面组成的窄角时，可能需要增加第二次切向扫描。
