using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerMinimapTeleportView : MonoBehaviour
{
    [LabelText("小地图传送按键")]
    [SerializeField] private KeyCode activationKey = KeyCode.Alpha5;

    [Header("地图参数")]
    [LabelText("玩家相机")]
    [SerializeField] private Camera playerCamera;

    [LabelText("小地图图片")]
    [SerializeField] private Texture2D minimapTexture;

    [LabelText("地图世界中心")]
    [SerializeField] private Vector3 mapWorldCenter;

    [LabelText("地图世界尺寸")]
    [SerializeField] private Vector2 mapWorldSize = new Vector2(50f, 50f);

    [LabelText("点击后退出小地图")]
    [SerializeField] private bool exitAfterTeleport = true;

    [Header("传送参数")]
    [LabelText("贴地检测层")]
    [SerializeField] private LayerMask groundLayerMask = ~0;

    [LabelText("贴地检测上方高度")]
    [SerializeField] private float groundProbeHeight = 60f;

    [LabelText("贴地检测距离")]
    [SerializeField] private float groundProbeDistance = 140f;

    [LabelText("贴地高度偏移")]
    [SerializeField] private float groundOffset = 0.05f;

    private bool isActive;
    private SimplePlayerController controller;

    public Camera PlayerCamera => playerCamera;
    public bool IsActivationRequested => activationKey != KeyCode.None && RuntimeInput.GetKeyDown(activationKey);
    public bool IsActive => isActive && UIManager.IsPanelVisible(UIPanelNames.Minimap);
    public bool ExitAfterTeleport => exitAfterTeleport;

    private void OnValidate()
    {
        mapWorldSize.x = Mathf.Max(0.1f, mapWorldSize.x);
        mapWorldSize.y = Mathf.Max(0.1f, mapWorldSize.y);
        groundProbeHeight = Mathf.Max(0.1f, groundProbeHeight);
        groundProbeDistance = Mathf.Max(0.1f, groundProbeDistance);
    }

    private void Update()
    {
        if (controller == null || !GameController.PlayerControlEnabled)
        {
            return;
        }

        if (IsActivationRequested)
        {
            controller.ApplyViewMode(controller.CurrentViewMode == PlayerViewMode.MinimapTeleport
                ? PlayerViewMode.FirstPerson
                : PlayerViewMode.MinimapTeleport);
        }

        if (controller.CurrentViewMode == PlayerViewMode.MinimapTeleport)
        {
            controller.TickMinimapTeleportView(this);
        }
    }

    public void Bind(SimplePlayerController owner)
    {
        controller = owner;
    }

    public void SetPlayerCamera(Camera camera)
    {
        if (playerCamera == null)
        {
            playerCamera = camera;
        }
    }

    public void Initialize()
    {
        EnsureCamera();
        ApplyTexture();
        UIManager.HidePanel(UIPanelNames.Minimap);
    }

    public void SetMap(Texture2D texture, Vector3 worldCenter, Vector2 worldSize)
    {
        minimapTexture = texture;
        mapWorldCenter = worldCenter;
        mapWorldSize = new Vector2(Mathf.Max(0.1f, worldSize.x), Mathf.Max(0.1f, worldSize.y));
        ApplyTexture();
    }

    public void Enter()
    {
        ApplyTexture();
        isActive = UIManager.ShowPanel(UIPanelNames.Minimap);
    }

    public void Exit()
    {
        Hide();
    }

    public bool TryHandleTeleportClick(out Vector3 teleportPosition)
    {
        teleportPosition = Vector3.zero;

        MinimapPanel panel = UIManager.GetPanel<MinimapPanel>(UIPanelNames.Minimap);
        if (!IsActive || panel == null || minimapTexture == null)
        {
            return false;
        }

        if (!RuntimeInput.GetMouseButtonDown(0))
        {
            return false;
        }

        if (!panel.TryGetPointerNormalizedPosition(RuntimeInput.GetPointerPosition(), out Vector2 normalizedPosition))
        {
            return false;
        }

        Vector3 target = MapNormalizedPointToWorld(normalizedPosition.x, normalizedPosition.y);
        teleportPosition = ResolveGroundedPosition(target);
        return true;
    }

    public bool Tick(out Vector3 teleportPosition)
    {
        return TryHandleTeleportClick(out teleportPosition);
    }

    private Vector3 MapNormalizedPointToWorld(float normalizedX, float normalizedY)
    {
        float worldX = mapWorldCenter.x + (normalizedX - 0.5f) * mapWorldSize.x;
        float worldZ = mapWorldCenter.z + (normalizedY - 0.5f) * mapWorldSize.y;
        return new Vector3(worldX, mapWorldCenter.y, worldZ);
    }

    private Vector3 ResolveGroundedPosition(Vector3 target)
    {
        Vector3 rayOrigin = new Vector3(target.x, mapWorldCenter.y + groundProbeHeight, target.z);
        float maxDistance = groundProbeHeight + groundProbeDistance;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, maxDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point + Vector3.up * groundOffset;
        }

        return target;
    }

    private void Hide()
    {
        UIManager.HidePanel(UIPanelNames.Minimap);
        isActive = false;
    }

    private void EnsureCamera()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>(true);
        }

        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main;
        }
    }

    private void ApplyTexture()
    {
        MinimapPanel panel = UIManager.GetPanel<MinimapPanel>(UIPanelNames.Minimap);
        if (panel != null)
        {
            panel.SetTexture(minimapTexture);
        }
    }

}
