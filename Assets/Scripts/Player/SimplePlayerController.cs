using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class SimplePlayerController : MonoBehaviour
{
    [Header("视角模式")]
    [LabelText("起始视角")]
    [SerializeField] private PlayerViewMode startViewMode = PlayerViewMode.FirstPerson;

    [Header("按键配置")]
    [LabelText("第一人称按键")]
    [SerializeField] private KeyCode firstPersonKey = KeyCode.Alpha1;

    [LabelText("第三人称按键")]
    [SerializeField] private KeyCode thirdPersonKey = KeyCode.Alpha2;

    [LabelText("固定路线漫游按键")]
    [SerializeField] private KeyCode fixedRouteRoamKey = KeyCode.Alpha3;

    [LabelText("固定视角选择按键")]
    [SerializeField] private KeyCode fixedCameraSelectionKey = KeyCode.Alpha4;

    [LabelText("小地图传送按键")]
    [SerializeField] private KeyCode minimapTeleportKey = KeyCode.Alpha5;

    [LabelText("详情查看按键")]
    [SerializeField] private KeyCode detailInspectKey = KeyCode.Alpha6;

    [LabelText("透视拾取按键")]
    [SerializeField] private KeyCode perspectivePickupKey = KeyCode.Alpha7;

    [LabelText("奔跑按键")]
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

    [LabelText("交互按键")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [LabelText("显示交互提示按键")]
    [SerializeField] private KeyCode revealInteractablesKey = KeyCode.R;

    [LabelText("手电筒按键")]
    [SerializeField] private KeyCode flashlightKey = KeyCode.F;

    [Header("移动配置")]
    [LabelText("行走速度")]
    [SerializeField] private float walkSpeed = 3.5f;

    [LabelText("奔跑速度")]
    [SerializeField] private float runSpeed = 6.5f;

    [LabelText("重力")]
    [SerializeField] private float gravity = -24f;

    [LabelText("启动时锁定鼠标")]
    [SerializeField] private bool lockCursorOnStart = true;

    [Header("手电筒配置")]
    [LabelText("手电筒灯光")]
    [SerializeField] private Light flashlightLight;

    [LabelText("未指定时创建灯光")]
    [SerializeField] private bool createFlashlightWhenMissing = true;

    [LabelText("手电筒默认开启")]
    [SerializeField] private bool flashlightStartsOn;

    [LabelText("手电筒局部位置")]
    [SerializeField] private Vector3 flashlightLocalPosition = new Vector3(0.25f, -0.2f, 0.35f);

    [LabelText("第三人称手电筒前方偏移")]
    [SerializeField] private Vector3 thirdPersonFlashlightLocalPosition = new Vector3(0.25f, 1.35f, 0.85f);

    [LabelText("第三人称手电筒前方角度")]
    [SerializeField] private Vector3 thirdPersonFlashlightLocalEulerAngles;

    [LabelText("手电筒强度")]
    [SerializeField] private float flashlightIntensity = 2f;

    [LabelText("手电筒范围")]
    [SerializeField] private float flashlightRange = 18f;

    [LabelText("手电筒角度")]
    [SerializeField] private float flashlightSpotAngle = 55f;

    [Header("自动组件引用（只读）")]
    [ReadOnly]
    [LabelText("第一人称组件")]
    [SerializeField] private PlayerFirstPersonView firstPersonView;

    [ReadOnly]
    [LabelText("第三人称组件")]
    [SerializeField] private PlayerThirdPersonView thirdPersonView;

    [ReadOnly]
    [LabelText("固定路线漫游组件")]
    [SerializeField] private PlayerFixedRouteRoamView fixedRouteRoamView;

    [ReadOnly]
    [LabelText("固定视角组件")]
    [SerializeField] private PlayerFixedCameraView fixedCameraView;

    [ReadOnly]
    [LabelText("小地图传送组件")]
    [SerializeField] private PlayerMinimapTeleportView minimapTeleportView;

    [ReadOnly]
    [LabelText("详情查看组件")]
    [SerializeField] private PlayerDetailInspectView detailInspectView;

    private const float GroundedStickForce = -2f;

    private CharacterController controller;
    private PerspectivePickupObject heldPerspectiveObject;

    private PlayerViewMode currentViewMode;
    private Vector3 verticalVelocity;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private float yaw;
    private float pitch;
    private bool hasAppliedViewMode;

    public PlayerViewMode CurrentViewMode => currentViewMode;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        yaw = transform.eulerAngles.y;

        EnsureViewComponents();
        EnsureFlashlight();
        ApplyViewMode(startViewMode);
    }

    private void Start()
    {
        if (GameController.PlayerControlEnabled)
        {
            SetCursorLocked(lockCursorOnStart);
        }
    }

    private void Update()
    {
        if (!GameController.PlayerControlEnabled)
        {
            ResetViewMovementState();
            HideInteractionPrompt();
            return;
        }

        HandleViewModeInput();
        HandleDetailInspectInput();
        RefreshInteractionPrompt();

        if (fixedCameraView != null && fixedCameraView.IsSelectionPanelVisible)
        {
            ResetViewMovementState();

            if (currentViewMode == PlayerViewMode.FixedCamera)
            {
                UpdateFixedCameraView();
            }

            return;
        }

        if (currentViewMode == PlayerViewMode.MinimapTeleport)
        {
            UpdateMinimapTeleportView();
            return;
        }

        if (currentViewMode == PlayerViewMode.FixedRouteRoam)
        {
            UpdateFixedRouteRoam();
            return;
        }

        if (currentViewMode == PlayerViewMode.FixedCamera)
        {
            UpdateFixedCameraView();
            return;
        }

        HandleInteractionInput();
        HandleRevealInteractablesInput();
        HandleFlashlightInput();
        HandleLookInput();
        HandleMoveInput();
    }

    private void LateUpdate()
    {
        RefreshActiveView();

        if (heldPerspectiveObject != null)
        {
            heldPerspectiveObject.TickHeldObject();
        }
    }

    private void OnDisable()
    {
        ReleasePerspectiveObject();
        HideInteractionPrompt();
    }

    private void OnValidate()
    {
        walkSpeed = Mathf.Max(0f, walkSpeed);
        runSpeed = Mathf.Max(walkSpeed, runSpeed);
        gravity = Mathf.Approximately(gravity, 0f) ? -24f : -Mathf.Abs(gravity);
        flashlightIntensity = Mathf.Max(0f, flashlightIntensity);
        flashlightRange = Mathf.Max(0.1f, flashlightRange);
        flashlightSpotAngle = Mathf.Clamp(flashlightSpotAngle, 1f, 179f);
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

        EnsureViewComponents();
        if (viewMode != PlayerViewMode.FirstPerson && detailInspectView.IsActive)
        {
            detailInspectView.SetActive(false);
        }

        bool wasFixedCameraPanelVisible = fixedCameraView != null && fixedCameraView.IsSelectionPanelVisible;
        bool shouldRestoreCursorAfterModeSwitch = currentViewMode == PlayerViewMode.MinimapTeleport || wasFixedCameraPanelVisible;
        if (viewMode != PlayerViewMode.FixedCamera && fixedCameraView != null)
        {
            fixedCameraView.HideSelectionPanel();
        }

        if (hasAppliedViewMode)
        {
            SyncViewAnglesBeforeLeavingCurrentMode(viewMode);
            ExitCurrentViewMode();
        }

        currentViewMode = viewMode;
        hasAppliedViewMode = true;
        verticalVelocity = Vector3.zero;

        switch (currentViewMode)
        {
            case PlayerViewMode.FirstPerson:
            case PlayerViewMode.PerspectivePickup:
                firstPersonView.Enter(ref yaw, ref pitch);
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

        AttachFlashlightToActiveView();
        RefreshActiveView();
        RefreshModeDisplay();

        if (currentViewMode == PlayerViewMode.MinimapTeleport)
        {
            SetCursorLocked(false);
        }
        else if (shouldRestoreCursorAfterModeSwitch && GameController.PlayerControlEnabled)
        {
            SetCursorLocked(lockCursorOnStart);
        }
    }

    internal void ApplyControlPermission(bool enabled)
    {
        if (!enabled)
        {
            ResetViewMovementState();
            HideInteractionPrompt();
        }

        SetCursorLocked(enabled && lockCursorOnStart);
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

        bool wasEnabled = controller.enabled;
        controller.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        controller.enabled = wasEnabled;

        verticalVelocity = Vector3.zero;
        yaw = transform.eulerAngles.y;
        pitch = 0f;
        RefreshActiveView();
    }

    private void HandleViewModeInput()
    {
        if (firstPersonKey != KeyCode.None && RuntimeInput.GetKeyDown(firstPersonKey))
        {
            ApplyViewMode(PlayerViewMode.FirstPerson);
        }
        else if (thirdPersonKey != KeyCode.None && RuntimeInput.GetKeyDown(thirdPersonKey))
        {
            ApplyViewMode(PlayerViewMode.ThirdPerson);
        }
        else if (fixedRouteRoamKey != KeyCode.None && RuntimeInput.GetKeyDown(fixedRouteRoamKey))
        {
            ApplyViewMode(PlayerViewMode.FixedRouteRoam);
        }
        else if (fixedCameraSelectionKey != KeyCode.None && RuntimeInput.GetKeyDown(fixedCameraSelectionKey))
        {
            ToggleFixedCameraSelection();
        }
        else if (minimapTeleportKey != KeyCode.None && RuntimeInput.GetKeyDown(minimapTeleportKey))
        {
            if (currentViewMode == PlayerViewMode.MinimapTeleport)
            {
                ApplyViewMode(PlayerViewMode.FirstPerson);
            }
            else
            {
                ApplyViewMode(PlayerViewMode.MinimapTeleport);
            }
        }
        else if (perspectivePickupKey != KeyCode.None && RuntimeInput.GetKeyDown(perspectivePickupKey))
        {
            ApplyViewMode(PlayerViewMode.PerspectivePickup);
        }
    }

    private void HandleDetailInspectInput()
    {
        if (detailInspectKey == KeyCode.None || !RuntimeInput.GetKeyDown(detailInspectKey))
        {
            return;
        }

        if (currentViewMode != PlayerViewMode.FirstPerson
            || (fixedCameraView != null && fixedCameraView.IsSelectionPanelVisible))
        {
            return;
        }

        EnsureViewComponents();
        detailInspectView.Toggle();
        RefreshModeDisplay();
    }

    private void UpdateFixedRouteRoam()
    {
        if (fixedRouteRoamView == null)
        {
            ApplyViewMode(PlayerViewMode.FirstPerson);
            return;
        }

        HandleFixedRouteRoamLookInput();

        bool moved = fixedRouteRoamView.TickRoam(yaw, pitch, out bool finished);
        if (!moved)
        {
            if (fixedRouteRoamView.ReturnToFirstPersonWhenFinished)
            {
                yaw = transform.eulerAngles.y;
                pitch = 0f;
                ApplyViewMode(PlayerViewMode.FirstPerson);
            }

            return;
        }

        if (finished && fixedRouteRoamView.ReturnToFirstPersonWhenFinished)
        {
            yaw = transform.eulerAngles.y;
            pitch = 0f;
            ApplyViewMode(PlayerViewMode.FirstPerson);
        }
    }

    private void UpdateMinimapTeleportView()
    {
        if (minimapTeleportView == null || !minimapTeleportView.IsActive)
        {
            ApplyViewMode(PlayerViewMode.FirstPerson);
            return;
        }

        if (!minimapTeleportView.TryHandleTeleportClick(out Vector3 teleportPosition))
        {
            return;
        }

        bool exitAfterTeleport = minimapTeleportView.ExitAfterTeleport;
        TeleportTo(teleportPosition, Quaternion.Euler(0f, yaw, 0f));

        if (!exitAfterTeleport)
        {
            ApplyViewMode(PlayerViewMode.MinimapTeleport);
        }
    }

    private void HandleFixedRouteRoamLookInput()
    {
        if (!fixedRouteRoamView.UsesFreeLook)
        {
            return;
        }

        if (UnityEngine.Cursor.lockState != CursorLockMode.Locked)
        {
            if (lockCursorOnStart && RuntimeInput.GetMouseButtonDown(0))
            {
                SetCursorLocked(true);
            }

            return;
        }

        fixedRouteRoamView.HandleLookInput(ref yaw, ref pitch);
    }

    private void ToggleFixedCameraSelection()
    {
        EnsureViewComponents();

        bool panelVisible = fixedCameraView.ToggleSelectionPanel(ApplyFixedCameraView);
        SetCursorLocked(!panelVisible && lockCursorOnStart);

        if (panelVisible)
        {
            RefreshModeDisplay(PlayerViewMode.FixedCamera, false);
        }
        else
        {
            RefreshModeDisplay();
        }
    }

    private void ApplyFixedCameraView(int pointIndex)
    {
        EnsureViewComponents();

        if (!fixedCameraView.HasUsablePoint(pointIndex))
        {
            return;
        }

        if (detailInspectView != null && detailInspectView.IsActive)
        {
            detailInspectView.SetActive(false);
        }

        if (hasAppliedViewMode && currentViewMode != PlayerViewMode.FixedCamera)
        {
            SyncViewAnglesBeforeLeavingCurrentMode(PlayerViewMode.FixedCamera);
            ExitCurrentViewMode();
        }

        if (!fixedCameraView.Enter(pointIndex))
        {
            return;
        }

        currentViewMode = PlayerViewMode.FixedCamera;
        hasAppliedViewMode = true;
        verticalVelocity = Vector3.zero;
        ResetViewMovementState();
        AttachFlashlightToActiveView();
        RefreshModeDisplay();
        SetCursorLocked(!fixedCameraView.IsSelectionPanelVisible && lockCursorOnStart);
    }

    private void UpdateFixedCameraView()
    {
        if (fixedCameraView == null || !fixedCameraView.IsActive)
        {
            ApplyViewMode(PlayerViewMode.FirstPerson);
            return;
        }

        if (fixedCameraView.IsSelectionPanelVisible)
        {
            if (RuntimeInput.GetMouseButton(1))
            {
                fixedCameraView.HandleLookInput();
                fixedCameraView.RefreshCamera();
            }

            return;
        }

        if (UnityEngine.Cursor.lockState != CursorLockMode.Locked)
        {
            if (lockCursorOnStart && RuntimeInput.GetMouseButtonDown(0))
            {
                SetCursorLocked(true);
            }

            return;
        }

        fixedCameraView.HandleLookInput();
        fixedCameraView.RefreshCamera();
    }

    private void HandleLookInput()
    {
        if (UnityEngine.Cursor.lockState != CursorLockMode.Locked)
        {
            if (lockCursorOnStart && RuntimeInput.GetMouseButtonDown(0))
            {
                SetCursorLocked(true);
            }

            return;
        }

        switch (currentViewMode)
        {
            case PlayerViewMode.FirstPerson:
            case PlayerViewMode.PerspectivePickup:
                firstPersonView.HandleLookInput(ref yaw, ref pitch);
                break;
            case PlayerViewMode.ThirdPerson:
                thirdPersonView.HandleLookInput(ref yaw, ref pitch);
                break;
        }
    }

    private void HandleMoveInput()
    {
        Vector2 moveAxes = RuntimeInput.GetMoveAxesRaw();
        Vector3 input = new Vector3(moveAxes.x, 0f, moveAxes.y);
        input = Vector3.ClampMagnitude(input, 1f);

        Vector3 moveDirection = GetMoveDirection(input);
        bool isRunning = runKey != KeyCode.None && RuntimeInput.GetKey(runKey);
        float speed = isRunning ? runSpeed : walkSpeed;

        if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = GroundedStickForce;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        Vector3 horizontalVelocity = moveDirection * speed;
        controller.Move((horizontalVelocity + verticalVelocity) * Time.deltaTime);

        Vector3 actualHorizontalVelocity = controller.velocity;
        actualHorizontalVelocity.y = 0f;

        if (currentViewMode == PlayerViewMode.ThirdPerson)
        {
            thirdPersonView.SetMovementState(moveDirection, actualHorizontalVelocity.magnitude, isRunning, controller.isGrounded);
        }
        else
        {
            ResetViewMovementState();
        }
    }

    private void HandleInteractionInput()
    {
        if (interactKey == KeyCode.None || !RuntimeInput.GetKeyDown(interactKey))
        {
            return;
        }

        if (heldPerspectiveObject != null)
        {
            ReleasePerspectiveObject();
            return;
        }

        if (currentViewMode == PlayerViewMode.PerspectivePickup)
        {
            PerspectivePickupObject pickupObject = FindPerspectivePickupObject();
            Camera activeCamera = ResolveActiveCamera();
            Collider[] playerColliders = GetComponentsInChildren<Collider>(true);
            if (pickupObject != null && pickupObject.TryPickup(activeCamera, playerColliders))
            {
                heldPerspectiveObject = pickupObject;
                return;
            }
        }

        InteractableArea nearestArea = FindNearestInteractableArea();
        if (nearestArea != null)
        {
            nearestArea.Interact(gameObject);
        }
    }

    private PerspectivePickupObject FindPerspectivePickupObject()
    {
        Camera activeCamera = ResolveActiveCamera();
        if (activeCamera == null)
        {
            return null;
        }

        Ray ray = new Ray(activeCamera.transform.position, activeCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            Mathf.Infinity,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
        PerspectivePickupObject nearestObject = null;
        float nearestDistance = float.PositiveInfinity;

        foreach (RaycastHit hit in hits)
        {
            PerspectivePickupObject candidate = hit.collider.GetComponentInParent<PerspectivePickupObject>();
            if (candidate != null
                && !candidate.IsHeld
                && candidate.IsWithinPickupDistance(hit.distance)
                && hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                nearestObject = candidate;
            }
        }

        return nearestObject;
    }

    private void ReleasePerspectiveObject()
    {
        if (heldPerspectiveObject == null)
        {
            return;
        }

        heldPerspectiveObject.Release();
        heldPerspectiveObject = null;
    }

    private void HandleFlashlightInput()
    {
        if (flashlightKey == KeyCode.None || !RuntimeInput.GetKeyDown(flashlightKey))
        {
            return;
        }

        ToggleFlashlight();
    }

    private void HandleRevealInteractablesInput()
    {
        if (revealInteractablesKey == KeyCode.None || !RuntimeInput.GetKeyDown(revealInteractablesKey))
        {
            return;
        }

        ShowNearbyInteractableHints();
    }

    public void ToggleFlashlight()
    {
        if (flashlightLight == null)
        {
            EnsureFlashlight();
        }

        if (flashlightLight != null)
        {
            AttachFlashlightToActiveView();
            flashlightLight.enabled = !flashlightLight.enabled;
        }
    }

    private Vector3 GetMoveDirection(Vector3 input)
    {
        switch (currentViewMode)
        {
            case PlayerViewMode.ThirdPerson:
                return thirdPersonView.GetMoveDirection(input, yaw);
            case PlayerViewMode.FirstPerson:
            case PlayerViewMode.PerspectivePickup:
                return firstPersonView.GetMoveDirection(input);
            default:
                return Vector3.zero;
        }
    }

    private InteractableArea FindNearestInteractableArea()
    {
        InteractableArea nearestArea = null;
        float nearestSqrDistance = float.PositiveInfinity;

        foreach (InteractableArea area in InteractableArea.ActiveInstances)
        {
            if (area == null
                || !area.TryGetInteractionDistance(transform.position, out float sqrDistance))
            {
                continue;
            }

            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearestArea = area;
            }
        }

        return nearestArea;
    }

    private void ShowNearbyInteractableHints()
    {
        foreach (InteractableArea area in InteractableArea.ActiveInstances)
        {
            if (area != null)
            {
                area.TryShowHint(transform.position);
            }
        }

        ShowNearbyWorldDescriptionHints();
    }

    private void ShowNearbyWorldDescriptionHints()
    {
        foreach (WorldDescriptionUI descriptionUI in FindObjectsByType<WorldDescriptionUI>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            if (!descriptionUI.ShouldShowHint)
            {
                continue;
            }

            descriptionUI.TryShowHint(transform.position);
        }
    }

    private void EnsureViewComponents()
    {
        if (firstPersonView == null)
        {
            firstPersonView = GetComponent<PlayerFirstPersonView>();
        }

        if (firstPersonView == null)
        {
            firstPersonView = gameObject.AddComponent<PlayerFirstPersonView>();
        }

        firstPersonView.Initialize();
        Camera sharedCamera = firstPersonView.PlayerCamera;

        if (thirdPersonView == null)
        {
            thirdPersonView = GetComponent<PlayerThirdPersonView>();
        }

        if (thirdPersonView == null)
        {
            thirdPersonView = gameObject.AddComponent<PlayerThirdPersonView>();
        }

        thirdPersonView.SetPlayerCamera(sharedCamera);
        thirdPersonView.Initialize();

        if (fixedRouteRoamView == null)
        {
            fixedRouteRoamView = GetComponent<PlayerFixedRouteRoamView>();
        }

        if (fixedRouteRoamView == null)
        {
            fixedRouteRoamView = gameObject.AddComponent<PlayerFixedRouteRoamView>();
        }

        fixedRouteRoamView.SetPlayerCamera(sharedCamera);
        fixedRouteRoamView.Initialize();

        if (fixedCameraView == null)
        {
            fixedCameraView = GetComponent<PlayerFixedCameraView>();
        }

        if (fixedCameraView == null)
        {
            fixedCameraView = gameObject.AddComponent<PlayerFixedCameraView>();
        }

        fixedCameraView.SetPlayerCamera(sharedCamera);
        fixedCameraView.Initialize();

        if (minimapTeleportView == null)
        {
            minimapTeleportView = GetComponent<PlayerMinimapTeleportView>();
        }

        if (minimapTeleportView == null)
        {
            minimapTeleportView = gameObject.AddComponent<PlayerMinimapTeleportView>();
        }

        minimapTeleportView.SetPlayerCamera(sharedCamera);
        minimapTeleportView.Initialize();

        if (detailInspectView == null)
        {
            detailInspectView = GetComponent<PlayerDetailInspectView>();
        }

        if (detailInspectView == null)
        {
            detailInspectView = gameObject.AddComponent<PlayerDetailInspectView>();
        }

        detailInspectView.SetPlayerCamera(sharedCamera);
        detailInspectView.Initialize();
    }

    private void ExitCurrentViewMode()
    {
        if (currentViewMode == PlayerViewMode.PerspectivePickup)
        {
            ReleasePerspectiveObject();
        }

        switch (currentViewMode)
        {
            case PlayerViewMode.FirstPerson:
            case PlayerViewMode.PerspectivePickup:
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

    private void SyncViewAnglesBeforeLeavingCurrentMode(PlayerViewMode nextViewMode)
    {
        if (currentViewMode != PlayerViewMode.FixedRouteRoam || nextViewMode == PlayerViewMode.FixedRouteRoam)
        {
            return;
        }

        yaw = transform.eulerAngles.y;
        if (fixedRouteRoamView != null && !fixedRouteRoamView.UsesFreeLook)
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

    private void ResetViewMovementState()
    {
        if (thirdPersonView != null)
        {
            thirdPersonView.ResetMovementState();
        }
    }

    private void RefreshModeDisplay()
    {
        bool detailInspectActive = detailInspectView != null && detailInspectView.IsActive;
        RefreshModeDisplay(currentViewMode, detailInspectActive);
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

    private void RefreshInteractionPrompt()
    {
        if (!AllowsNearbyInteraction())
        {
            HideInteractionPrompt();
            return;
        }

        InteractableArea nearestArea = FindNearestInteractableArea();
        if (nearestArea == null || string.IsNullOrWhiteSpace(nearestArea.PromptText))
        {
            HideInteractionPrompt();
            return;
        }

        PlayerInteractionPromptDisplay panel = UIManager.GetPanel<PlayerInteractionPromptDisplay>(
            UIPanelNames.InteractionPrompt);
        if (panel == null)
        {
            return;
        }

        panel.SetMessage(nearestArea.PromptText);
        UIManager.ShowPanel(UIPanelNames.InteractionPrompt);
    }

    private bool AllowsNearbyInteraction()
    {
        return currentViewMode == PlayerViewMode.FirstPerson
            || currentViewMode == PlayerViewMode.ThirdPerson
            || currentViewMode == PlayerViewMode.PerspectivePickup;
    }

    private void HideInteractionPrompt()
    {
        if (UIManager.GetPanel<PlayerInteractionPromptDisplay>(UIPanelNames.InteractionPrompt) != null)
        {
            UIManager.HidePanel(UIPanelNames.InteractionPrompt);
        }
    }

    private void EnsureFlashlight()
    {
        Camera activeCamera = ResolveActiveCamera();
        if (flashlightLight == null && createFlashlightWhenMissing && activeCamera != null)
        {
            GameObject flashlightObject = new GameObject("Flashlight");
            flashlightObject.transform.SetParent(activeCamera.transform, false);
            flashlightLight = flashlightObject.AddComponent<Light>();
            flashlightLight.type = LightType.Spot;
        }

        if (flashlightLight == null)
        {
            return;
        }

        AttachFlashlightToActiveView();
        flashlightLight.intensity = flashlightIntensity;
        flashlightLight.range = flashlightRange;
        flashlightLight.spotAngle = flashlightSpotAngle;
        flashlightLight.enabled = flashlightStartsOn;
    }

    private void AttachFlashlightToActiveView()
    {
        if (flashlightLight == null)
        {
            return;
        }

        if (currentViewMode == PlayerViewMode.ThirdPerson)
        {
            flashlightLight.transform.SetParent(transform, false);
            flashlightLight.transform.localPosition = thirdPersonFlashlightLocalPosition;
            flashlightLight.transform.localRotation = Quaternion.Euler(thirdPersonFlashlightLocalEulerAngles);
            return;
        }

        Camera activeCamera = ResolveActiveCamera();
        if (activeCamera == null)
        {
            return;
        }

        flashlightLight.transform.SetParent(activeCamera.transform, false);
        flashlightLight.transform.localPosition = flashlightLocalPosition;
        flashlightLight.transform.localRotation = Quaternion.identity;
    }

    private Camera ResolveActiveCamera()
    {
        switch (currentViewMode)
        {
            case PlayerViewMode.ThirdPerson:
                return thirdPersonView != null ? thirdPersonView.PlayerCamera : null;
            case PlayerViewMode.FixedRouteRoam:
                return fixedRouteRoamView != null ? fixedRouteRoamView.PlayerCamera : null;
            case PlayerViewMode.FixedCamera:
                return fixedCameraView != null ? fixedCameraView.PlayerCamera : null;
            case PlayerViewMode.MinimapTeleport:
                return minimapTeleportView != null ? minimapTeleportView.PlayerCamera : null;
            default:
                return firstPersonView != null ? firstPersonView.PlayerCamera : null;
        }
    }

    private static void SetCursorLocked(bool locked)
    {
        GameController.SetCursorLocked(locked);
    }
}
