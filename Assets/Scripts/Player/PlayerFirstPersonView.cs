using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerFirstPersonView : MonoBehaviour
{
    private const string CameraPitchPivotName = "Camera Pitch Pivot";

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

    public Camera PlayerCamera => playerCamera;

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
            GameObject cameraObject = new GameObject("Player Camera");
            playerCamera = cameraObject.AddComponent<Camera>();
        }

        playerCamera.transform.SetParent(cameraPitchPivot, false);
        TrySetMainCameraTag(playerCamera.gameObject);

        if (playerCamera.GetComponent<AudioListener>() == null && FindObjectOfType<AudioListener>() == null)
        {
            playerCamera.gameObject.AddComponent<AudioListener>();
        }
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
