using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerLocomotion))]
public sealed class SimplePlayerController : MonoBehaviour
{
    [LabelText("起始视角")]
    [SerializeField] private PlayerViewMode startViewMode = PlayerViewMode.FirstPerson;

    [LabelText("启动时锁定鼠标")]
    [SerializeField] private bool lockCursorOnStart = true;

    private CharacterController characterController;
    private PlayerLocomotion locomotion;
    private PlayerFirstPersonView firstPersonView;
    private PlayerThirdPersonView thirdPersonView;
    private PlayerFixedRouteRoamView fixedRouteRoamView;
    private PlayerFixedCameraView fixedCameraView;
    private PlayerMinimapTeleportView minimapTeleportView;
    private PlayerDetailInspectView detailInspectView;
    private PlayerPerspectivePickupView perspectivePickupView;
    private PlayerFlashlight flashlight;

    private PlayerViewMode currentViewMode;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private float yaw;
    private float pitch;
    private bool hasAppliedViewMode;

    public PlayerViewMode CurrentViewMode => currentViewMode;
    public Camera ActiveCamera => ResolveActiveCamera();
    public bool IsViewInputBlocked => fixedCameraView != null && fixedCameraView.IsSelectionPanelVisible;
    public bool AllowsManualAbilities => !IsViewInputBlocked
        && (currentViewMode == PlayerViewMode.FirstPerson
            || currentViewMode == PlayerViewMode.ThirdPerson
            || currentViewMode == PlayerViewMode.PerspectivePickup);
    public bool AllowsTriggerInteraction => currentViewMode == PlayerViewMode.FirstPerson
        || currentViewMode == PlayerViewMode.ThirdPerson;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        yaw = transform.eulerAngles.y;

        EnsureComponents();
        ApplyViewMode(startViewMode);
    }

    private void Start()
    {
        if (GameController.PlayerControlEnabled)
        {
            GameController.SetCursorLocked(lockCursorOnStart);
        }
    }

    private void OnDisable()
    {
        perspectivePickupView?.Exit();
    }

    public void ToggleViewMode()
    {
        ApplyViewMode(currentViewMode == PlayerViewMode.FirstPerson
            ? PlayerViewMode.ThirdPerson
            : PlayerViewMode.FirstPerson);
    }

    public void ApplyViewMode(PlayerViewMode viewMode)
    {
        if (hasAppliedViewMode && currentViewMode == viewMode)
        {
            return;
        }

        EnsureComponents();
        if (viewMode != PlayerViewMode.FirstPerson && detailInspectView.IsActive)
        {
            detailInspectView.SetActive(false);
        }

        bool restoreCursor = currentViewMode == PlayerViewMode.MinimapTeleport
            || fixedCameraView.IsSelectionPanelVisible;
        if (viewMode != PlayerViewMode.FixedCamera)
        {
            fixedCameraView.HideSelectionPanel();
        }

        if (hasAppliedViewMode)
        {
            SyncAnglesBeforeExit(viewMode);
            ExitCurrentViewMode();
        }

        currentViewMode = viewMode;
        hasAppliedViewMode = true;
        locomotion.ResetVerticalVelocity();
        EnterCurrentViewMode();

        flashlight.OnViewModeChanged(currentViewMode, ActiveCamera);
        RefreshActiveView();
        RefreshModeDisplay();

        if (currentViewMode == PlayerViewMode.MinimapTeleport)
        {
            GameController.SetCursorLocked(false);
        }
        else if (restoreCursor && GameController.PlayerControlEnabled)
        {
            GameController.SetCursorLocked(lockCursorOnStart);
        }
    }

    internal void ApplyControlPermission(bool enabled)
    {
        if (!enabled)
        {
            ResetMovementState();
        }

        GameController.SetCursorLocked(enabled && lockCursorOnStart);
    }

    public void ReturnToSpawn()
    {
        TeleportTo(spawnPosition, spawnRotation);
    }

    public void SetSpawnPoint(Vector3 position, Quaternion rotation)
    {
        spawnPosition = position;
        spawnRotation = rotation;
    }

    public void TeleportTo(Vector3 position, Quaternion rotation)
    {
        if (currentViewMode == PlayerViewMode.FixedRouteRoam
            || currentViewMode == PlayerViewMode.FixedCamera
            || currentViewMode == PlayerViewMode.MinimapTeleport)
        {
            ApplyViewMode(PlayerViewMode.FirstPerson);
        }

        bool wasEnabled = characterController.enabled;
        characterController.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        characterController.enabled = wasEnabled;

        locomotion.ResetVerticalVelocity();
        yaw = transform.eulerAngles.y;
        pitch = 0f;
        RefreshActiveView();
    }

    public void TickFirstPersonView(PlayerFirstPersonView view, PlayerLocomotion playerLocomotion)
    {
        view.Tick(ref yaw, ref pitch, lockCursorOnStart, playerLocomotion);
    }

    public void TickThirdPersonView(PlayerThirdPersonView view, PlayerLocomotion playerLocomotion)
    {
        view.Tick(ref yaw, ref pitch, lockCursorOnStart, playerLocomotion);
    }

    public void TickFixedRouteRoamView(PlayerFixedRouteRoamView view)
    {
        if (view == null || view.Tick(ref yaw, ref pitch, lockCursorOnStart))
        {
            yaw = transform.eulerAngles.y;
            pitch = 0f;
            ApplyViewMode(PlayerViewMode.FirstPerson);
        }
    }

    public void TickFixedCameraView(PlayerFixedCameraView view)
    {
        if (view == null || !view.Tick(lockCursorOnStart))
        {
            ApplyViewMode(PlayerViewMode.FirstPerson);
        }
    }

    public void TickMinimapTeleportView(PlayerMinimapTeleportView view)
    {
        if (view == null || !view.IsActive)
        {
            ApplyViewMode(PlayerViewMode.FirstPerson);
            return;
        }

        if (!view.Tick(out Vector3 teleportPosition))
        {
            return;
        }

        bool reopenMinimap = !view.ExitAfterTeleport;
        TeleportTo(teleportPosition, Quaternion.Euler(0f, yaw, 0f));
        if (reopenMinimap)
        {
            ApplyViewMode(PlayerViewMode.MinimapTeleport);
        }
    }

    public void ToggleFixedCameraSelection()
    {
        bool panelVisible = fixedCameraView.ToggleSelectionPanel(ApplyFixedCameraPoint);
        GameController.SetCursorLocked(!panelVisible && lockCursorOnStart);
        if (panelVisible)
        {
            RefreshModeDisplay(PlayerViewMode.FixedCamera, false);
        }
        else
        {
            RefreshModeDisplay();
        }
    }

    private void ApplyFixedCameraPoint(int pointIndex)
    {
        if (!fixedCameraView.HasUsablePoint(pointIndex))
        {
            return;
        }

        detailInspectView.SetActive(false);
        if (hasAppliedViewMode && currentViewMode != PlayerViewMode.FixedCamera)
        {
            SyncAnglesBeforeExit(PlayerViewMode.FixedCamera);
            ExitCurrentViewMode();
        }

        if (!fixedCameraView.Enter(pointIndex))
        {
            return;
        }

        currentViewMode = PlayerViewMode.FixedCamera;
        hasAppliedViewMode = true;
        locomotion.ResetVerticalVelocity();
        ResetMovementState();
        flashlight.OnViewModeChanged(currentViewMode, ActiveCamera);
        RefreshModeDisplay();
        GameController.SetCursorLocked(!fixedCameraView.IsSelectionPanelVisible && lockCursorOnStart);
    }

    private void EnterCurrentViewMode()
    {
        switch (currentViewMode)
        {
            case PlayerViewMode.FirstPerson:
                firstPersonView.Enter(ref yaw, ref pitch);
                break;
            case PlayerViewMode.PerspectivePickup:
                firstPersonView.Enter(ref yaw, ref pitch);
                perspectivePickupView.Enter(firstPersonView.PlayerCamera, GetComponentsInChildren<Collider>(true));
                break;
            case PlayerViewMode.ThirdPerson:
                thirdPersonView.Enter(yaw, pitch);
                break;
            case PlayerViewMode.FixedRouteRoam:
                fixedRouteRoamView.Enter(yaw, pitch);
                break;
            case PlayerViewMode.FixedCamera:
                fixedCameraView.Enter(0);
                break;
            case PlayerViewMode.MinimapTeleport:
                minimapTeleportView.Enter();
                break;
        }
    }

    private void ExitCurrentViewMode()
    {
        switch (currentViewMode)
        {
            case PlayerViewMode.FirstPerson:
                firstPersonView.Exit();
                break;
            case PlayerViewMode.PerspectivePickup:
                perspectivePickupView.Exit();
                firstPersonView.Exit();
                break;
            case PlayerViewMode.ThirdPerson:
                thirdPersonView.Exit();
                break;
            case PlayerViewMode.FixedRouteRoam:
                fixedRouteRoamView.Exit();
                break;
            case PlayerViewMode.FixedCamera:
                fixedCameraView.Exit();
                break;
            case PlayerViewMode.MinimapTeleport:
                minimapTeleportView.Exit();
                break;
        }
    }

    private void SyncAnglesBeforeExit(PlayerViewMode nextViewMode)
    {
        if (currentViewMode != PlayerViewMode.FixedRouteRoam || nextViewMode == PlayerViewMode.FixedRouteRoam)
        {
            return;
        }

        yaw = transform.eulerAngles.y;
        if (!fixedRouteRoamView.UsesFreeLook)
        {
            pitch = 0f;
        }
    }

    private void RefreshActiveView()
    {
        switch (currentViewMode)
        {
            case PlayerViewMode.FirstPerson:
            case PlayerViewMode.PerspectivePickup:
                firstPersonView.RefreshCamera(yaw, pitch);
                break;
            case PlayerViewMode.ThirdPerson:
                thirdPersonView.RefreshCamera(yaw, pitch);
                break;
            case PlayerViewMode.FixedCamera:
                fixedCameraView.RefreshCamera();
                break;
        }
    }

    private void ResetMovementState()
    {
        thirdPersonView?.ResetMovementState();
    }

    private void RefreshModeDisplay()
    {
        RefreshModeDisplay(currentViewMode, detailInspectView != null && detailInspectView.IsActive);
    }

    public void RefreshModeDisplayFromView()
    {
        RefreshModeDisplay();
    }

    private static void RefreshModeDisplay(PlayerViewMode viewMode, bool detailInspectActive)
    {
        PlayerModeDisplay panel = UIManager.GetPanel<PlayerModeDisplay>(UIPanelNames.PlayerMode);
        if (panel == null)
        {
            return;
        }

        panel.Refresh(viewMode, detailInspectActive);
        UIManager.ShowPanel(UIPanelNames.PlayerMode);
    }

    private Camera ResolveActiveCamera()
    {
        switch (currentViewMode)
        {
            case PlayerViewMode.ThirdPerson:
                return thirdPersonView?.PlayerCamera;
            case PlayerViewMode.FixedRouteRoam:
                return fixedRouteRoamView?.PlayerCamera;
            case PlayerViewMode.FixedCamera:
                return fixedCameraView?.PlayerCamera;
            case PlayerViewMode.MinimapTeleport:
                return minimapTeleportView?.PlayerCamera;
            default:
                return firstPersonView?.PlayerCamera;
        }
    }

    private void EnsureComponents()
    {
        locomotion = GetOrAddComponent<PlayerLocomotion>();
        firstPersonView = GetOrAddComponent<PlayerFirstPersonView>();
        firstPersonView.Bind(this, locomotion);
        firstPersonView.Initialize();
        Camera sharedCamera = firstPersonView.PlayerCamera;

        thirdPersonView = GetOrAddComponent<PlayerThirdPersonView>();
        thirdPersonView.Bind(this, locomotion);
        thirdPersonView.SetPlayerCamera(sharedCamera);
        thirdPersonView.Initialize();

        fixedRouteRoamView = GetOrAddComponent<PlayerFixedRouteRoamView>();
        fixedRouteRoamView.Bind(this);
        fixedRouteRoamView.SetPlayerCamera(sharedCamera);
        fixedRouteRoamView.Initialize();

        fixedCameraView = GetOrAddComponent<PlayerFixedCameraView>();
        fixedCameraView.Bind(this);
        fixedCameraView.SetPlayerCamera(sharedCamera);
        fixedCameraView.Initialize();

        minimapTeleportView = GetOrAddComponent<PlayerMinimapTeleportView>();
        minimapTeleportView.Bind(this);
        minimapTeleportView.SetPlayerCamera(sharedCamera);
        minimapTeleportView.Initialize();

        detailInspectView = GetOrAddComponent<PlayerDetailInspectView>();
        detailInspectView.Bind(this);
        detailInspectView.SetPlayerCamera(sharedCamera);
        detailInspectView.Initialize();

        perspectivePickupView = GetOrAddComponent<PlayerPerspectivePickupView>();
        perspectivePickupView.Bind(this);
        flashlight = GetOrAddComponent<PlayerFlashlight>();
        flashlight.Initialize(this);
        PlayerInteractionHintInput interactionHintInput = GetOrAddComponent<PlayerInteractionHintInput>();
        interactionHintInput.Bind(this);
    }

    private T GetOrAddComponent<T>() where T : Component
    {
        T component = GetComponent<T>();
        return component != null ? component : gameObject.AddComponent<T>();
    }
}
