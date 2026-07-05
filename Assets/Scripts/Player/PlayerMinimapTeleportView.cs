using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PlayerMinimapTeleportView : MonoBehaviour
{
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

    [Header("UI参数")]
    [LabelText("UI画布")]
    [SerializeField] private Canvas minimapCanvas;

    [LabelText("地图显示区域")]
    [SerializeField] private RawImage minimapImage;

    [LabelText("背景颜色")]
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.72f);

    [LabelText("地图边距")]
    [SerializeField] private Vector2 mapPadding = new Vector2(80f, 80f);

    private RectTransform minimapRect;
    private bool isActive;

    public Camera PlayerCamera => playerCamera;
    public bool IsActive => isActive;
    public bool ExitAfterTeleport => exitAfterTeleport;

    private void OnValidate()
    {
        mapWorldSize.x = Mathf.Max(0.1f, mapWorldSize.x);
        mapWorldSize.y = Mathf.Max(0.1f, mapWorldSize.y);
        groundProbeHeight = Mathf.Max(0.1f, groundProbeHeight);
        groundProbeDistance = Mathf.Max(0.1f, groundProbeDistance);
        mapPadding.x = Mathf.Max(0f, mapPadding.x);
        mapPadding.y = Mathf.Max(0f, mapPadding.y);
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
        EnsureMinimapCanvas();
        ApplyTexture();
        Hide();
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
        EnsureMinimapCanvas();
        ApplyTexture();

        if (minimapCanvas != null)
        {
            minimapCanvas.gameObject.SetActive(true);
        }

        isActive = true;
        EnsureEventSystem();
    }

    public void Exit()
    {
        Hide();
    }

    public bool TryHandleTeleportClick(out Vector3 teleportPosition)
    {
        teleportPosition = Vector3.zero;

        if (!isActive || minimapRect == null || minimapTexture == null)
        {
            return false;
        }

        if (!RuntimeInput.GetMouseButtonDown(0))
        {
            return false;
        }

        Vector2 pointerPosition = RuntimeInput.GetPointerPosition();
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, pointerPosition, null, out Vector2 localPoint))
        {
            return false;
        }

        Rect rect = minimapRect.rect;
        float normalizedX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        float normalizedY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);
        if (normalizedX < 0f || normalizedX > 1f || normalizedY < 0f || normalizedY > 1f)
        {
            return false;
        }

        Vector3 target = MapNormalizedPointToWorld(normalizedX, normalizedY);
        teleportPosition = ResolveGroundedPosition(target);
        return true;
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
        if (minimapCanvas != null)
        {
            minimapCanvas.gameObject.SetActive(false);
        }

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

    private void EnsureMinimapCanvas()
    {
        if (minimapCanvas == null)
        {
            GameObject canvasObject = new GameObject("Minimap Teleport Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            minimapCanvas = canvasObject.GetComponent<Canvas>();
            minimapCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            minimapCanvas.sortingOrder = 980;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (minimapCanvas.GetComponent<Image>() == null)
        {
            Image background = minimapCanvas.gameObject.AddComponent<Image>();
            background.color = backgroundColor;
        }
        else
        {
            minimapCanvas.GetComponent<Image>().color = backgroundColor;
        }

        if (minimapImage == null)
        {
            GameObject imageObject = new GameObject("Minimap Image", typeof(RectTransform), typeof(RawImage));
            minimapImage = imageObject.GetComponent<RawImage>();
            minimapImage.transform.SetParent(minimapCanvas.transform, false);
        }

        minimapRect = minimapImage.rectTransform;
        minimapRect.anchorMin = Vector2.zero;
        minimapRect.anchorMax = Vector2.one;
        minimapRect.offsetMin = mapPadding;
        minimapRect.offsetMax = -mapPadding;
    }

    private void ApplyTexture()
    {
        if (minimapImage != null)
        {
            minimapImage.texture = minimapTexture;
            minimapImage.color = Color.white;
        }
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = EventSystem.current != null ? EventSystem.current : FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            eventSystem = eventSystemObject.GetComponent<EventSystem>();
            DontDestroyOnLoad(eventSystemObject);
        }

#if ENABLE_INPUT_SYSTEM
        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        foreach (StandaloneInputModule legacyModule in eventSystem.GetComponents<StandaloneInputModule>())
        {
            legacyModule.enabled = false;
        }
#else
        if (eventSystem.GetComponent<StandaloneInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
#endif
    }
}
