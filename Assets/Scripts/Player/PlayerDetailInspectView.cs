using UnityEngine;

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

    private WorldDescriptionUI currentTarget;
    private bool isActive;
    private bool initialized;

    public bool IsActive => isActive;

    private void OnValidate()
    {
        inspectDistance = Mathf.Max(0.1f, inspectDistance);
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

    public bool TryHandleToggleInput(KeyCode toggleKey, bool inputAllowed)
    {
        if (!inputAllowed || toggleKey == KeyCode.None || !RuntimeInput.GetKeyDown(toggleKey))
        {
            return false;
        }

        Toggle();
        return true;
    }

    public void SetActive(bool active)
    {
        isActive = active;

        if (UIManager.GetPanel<DetailInspectPanel>(UIPanelNames.DetailInspect) != null)
        {
            UIManager.SetPanelVisible(UIPanelNames.DetailInspect, active);
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

}
