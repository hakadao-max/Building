using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PlayerDetailInspectView : MonoBehaviour
{
    [Header("检测参数")]
    [LabelText("玩家相机")]
    [SerializeField] private Camera playerCamera;

    [LabelText("检测距离")]
    [SerializeField] private float inspectDistance = 12f;

    [LabelText("检测层")]
    [SerializeField] private LayerMask inspectLayerMask = ~0;

    [Header("十字标参数")]
    [LabelText("十字标长度")]
    [SerializeField] private float crosshairArmLength = 18f;

    [LabelText("十字标粗细")]
    [SerializeField] private float crosshairThickness = 2f;

    [LabelText("十字标颜色")]
    [SerializeField] private Color crosshairColor = new Color(1f, 1f, 1f, 0.92f);

    [Header("UI参数")]
    [LabelText("UI画布")]
    [SerializeField] private Canvas inspectCanvas;

    [LabelText("十字标根节点")]
    [SerializeField] private RectTransform crosshairRoot;

    private WorldDescriptionUI currentTarget;
    private bool isActive;
    private bool initialized;

    public bool IsActive => isActive;

    private void OnValidate()
    {
        inspectDistance = Mathf.Max(0.1f, inspectDistance);
        crosshairArmLength = Mathf.Max(4f, crosshairArmLength);
        crosshairThickness = Mathf.Max(1f, crosshairThickness);
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
        EnsureInspectCanvas();

        if (initialized)
        {
            return;
        }

        initialized = true;
        SetActive(false);
    }

    public void Toggle()
    {
        SetActive(!isActive);
    }

    public void SetActive(bool active)
    {
        isActive = active;

        if (inspectCanvas != null)
        {
            inspectCanvas.gameObject.SetActive(active);
        }

        if (!active)
        {
            ClearCurrentTarget();
        }
    }

    private void LateUpdate()
    {
        if (!isActive)
        {
            return;
        }

        EnsureCamera();
        if (playerCamera == null)
        {
            ClearCurrentTarget();
            return;
        }

        UpdateTargetFromCrosshair();
    }

    private void UpdateTargetFromCrosshair()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, inspectDistance, inspectLayerMask, QueryTriggerInteraction.Ignore))
        {
            ClearCurrentTarget();
            return;
        }

        WorldDescriptionUI descriptionUI = hit.collider.GetComponentInParent<WorldDescriptionUI>();
        if (descriptionUI == null || !descriptionUI.RequiresDetailInspect)
        {
            ClearCurrentTarget();
            return;
        }

        if (currentTarget == descriptionUI)
        {
            return;
        }

        ClearCurrentTarget();
        currentTarget = descriptionUI;
        currentTarget.SetDetailInspectHighlighted(true);
    }

    private void ClearCurrentTarget()
    {
        if (currentTarget == null)
        {
            return;
        }

        currentTarget.SetDetailInspectHighlighted(false);
        currentTarget = null;
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

    private void EnsureInspectCanvas()
    {
        if (inspectCanvas != null && crosshairRoot != null)
        {
            return;
        }

        if (inspectCanvas == null)
        {
            GameObject canvasObject = new GameObject("Detail Inspect Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            inspectCanvas = canvasObject.GetComponent<Canvas>();
            inspectCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            inspectCanvas.sortingOrder = 960;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (crosshairRoot == null)
        {
            GameObject crosshairObject = new GameObject("Crosshair", typeof(RectTransform));
            crosshairRoot = crosshairObject.GetComponent<RectTransform>();
            crosshairRoot.SetParent(inspectCanvas.transform, false);
            crosshairRoot.anchorMin = new Vector2(0.5f, 0.5f);
            crosshairRoot.anchorMax = new Vector2(0.5f, 0.5f);
            crosshairRoot.pivot = new Vector2(0.5f, 0.5f);
            crosshairRoot.anchoredPosition = Vector2.zero;
            crosshairRoot.sizeDelta = new Vector2(crosshairArmLength * 2f, crosshairArmLength * 2f);

            CreateCrosshairArm(crosshairRoot, "Horizontal", true);
            CreateCrosshairArm(crosshairRoot, "Vertical", false);
        }
    }

    private void CreateCrosshairArm(RectTransform parent, string objectName, bool horizontal)
    {
        GameObject armObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        RectTransform armRect = armObject.GetComponent<RectTransform>();
        armRect.SetParent(parent, false);
        armRect.anchorMin = new Vector2(0.5f, 0.5f);
        armRect.anchorMax = new Vector2(0.5f, 0.5f);
        armRect.pivot = new Vector2(0.5f, 0.5f);
        armRect.anchoredPosition = Vector2.zero;
        armRect.sizeDelta = horizontal
            ? new Vector2(crosshairArmLength * 2f, crosshairThickness)
            : new Vector2(crosshairThickness, crosshairArmLength * 2f);

        Image armImage = armObject.GetComponent<Image>();
        armImage.color = crosshairColor;
        armImage.raycastTarget = false;
    }
}
