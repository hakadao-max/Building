using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerFirstPersonView : MonoBehaviour
{
    private const string CameraPitchPivotName = "Camera Pitch Pivot";

    [LabelText("第一人称按键")]
    [SerializeField] private KeyCode activationKey = KeyCode.Alpha1;

    [LabelText("玩家相机")]
    [SerializeField] private Camera playerCamera;

    [LabelText("第一人称相机位置")]
    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0f, 1.65f, 0f);

    [LabelText("鼠标灵敏度")]
    [SerializeField] private float mouseSensitivity = 2.2f;

    [LabelText("俯仰最小角度")]
    [SerializeField] private float minPitch = -55f;

    [LabelText("俯仰最大角度")]
    [SerializeField] private float maxPitch = 70f;

    private Transform cameraPitchPivot;
    private SimplePlayerController controller;
    private PlayerLocomotion locomotion;

    public Camera PlayerCamera => playerCamera;
    public bool IsActivationRequested => activationKey != KeyCode.None && RuntimeInput.GetKeyDown(activationKey);

    private void OnValidate()
    {
        maxPitch = Mathf.Max(minPitch, maxPitch);
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
        EnsureCameraRig();
    }

    public void Enter(ref float yaw, ref float pitch)
    {
        Initialize();
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        RefreshCamera(yaw, pitch);
    }

    public void Exit()
    {
    }

    public void HandleLookInput(ref float yaw, ref float pitch)
    {
        Vector2 mouseDelta = RuntimeInput.GetMouseDelta();
        yaw += mouseDelta.x * mouseSensitivity;
        pitch -= mouseDelta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    public void TickLook(ref float yaw, ref float pitch, bool lockCursorOnClick)
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (lockCursorOnClick && RuntimeInput.GetMouseButtonDown(0))
            {
                GameController.SetCursorLocked(true);
            }

            return;
        }

        HandleLookInput(ref yaw, ref pitch);
    }

    private void Update()
    {
        if (controller == null || !GameController.PlayerControlEnabled)
        {
            return;
        }

        if (IsActivationRequested)
        {
            controller.ApplyViewMode(PlayerViewMode.FirstPerson);
        }

        if (!controller.IsViewInputBlocked
            && (controller.CurrentViewMode == PlayerViewMode.FirstPerson
                || controller.CurrentViewMode == PlayerViewMode.PerspectivePickup))
        {
            controller.TickFirstPersonView(this, locomotion);
        }
    }

    public void Bind(SimplePlayerController owner, PlayerLocomotion playerLocomotion)
    {
        controller = owner;
        locomotion = playerLocomotion;
    }

    public void Tick(ref float yaw, ref float pitch, bool lockCursorOnClick, PlayerLocomotion locomotion)
    {
        TickLook(ref yaw, ref pitch, lockCursorOnClick);
        if (locomotion != null)
        {
            Vector3 input = locomotion.ReadMoveInput();
            locomotion.Tick(GetMoveDirection(input));
        }

        RefreshCamera(yaw, pitch);
    }

    public Vector3 GetMoveDirection(Vector3 input)
    {
        if (input.sqrMagnitude <= 0.0001f)
        {
            return Vector3.zero;
        }

        return transform.TransformDirection(input);
    }

    public void RefreshCamera(float yaw, float pitch)
    {
        if (cameraPitchPivot == null || playerCamera == null)
        {
            return;
        }

        cameraPitchPivot.localPosition = cameraLocalPosition;
        cameraPitchPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        playerCamera.transform.localPosition = Vector3.zero;
        playerCamera.transform.localRotation = Quaternion.identity;
    }

    private void EnsureCameraRig()
    {
        cameraPitchPivot = transform.Find(CameraPitchPivotName);
        if (cameraPitchPivot == null)
        {
            GameObject pivotObject = new GameObject(CameraPitchPivotName);
            cameraPitchPivot = pivotObject.transform;
            cameraPitchPivot.SetParent(transform, false);
        }

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>(true);
        }

        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera == null)
        {
            Debug.LogError("cannot find Main Camera");
        }

        playerCamera.transform.SetParent(cameraPitchPivot, false);
        TrySetMainCameraTag(playerCamera.gameObject);

    }

    private static void TrySetMainCameraTag(GameObject cameraObject)
    {
        try
        {
            cameraObject.tag = "MainCamera";
        }
        catch (UnityException)
        {
            // MainCamera 是 Unity 默认标签；如果项目标签异常缺失，保留相机可用性。
        }
    }
}
