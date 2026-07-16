using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerThirdPersonView : MonoBehaviour
{
    private const string CameraPitchPivotName = "Camera Pitch Pivot";

    [Header("视角参数")]
    [LabelText("玩家相机")]
    [SerializeField] private Camera playerCamera;

    [LabelText("第三人称观察点")]
    [SerializeField] private Vector3 pivotLocalPosition = new Vector3(0f, 1.45f, 0f);

    [LabelText("第三人称距离")]
    [SerializeField] private float cameraDistance = 4.5f;

    [LabelText("鼠标灵敏度")]
    [SerializeField] private float mouseSensitivity = 2.2f;

    [LabelText("俯仰最小角度")]
    [SerializeField] private float minPitch = -55f;

    [LabelText("俯仰最大角度")]
    [SerializeField] private float maxPitch = 70f;

    [Header("模型参数")]
    [LabelText("第三人称可视模型")]
    [SerializeField] private Transform visualRoot;

    [LabelText("第三人称模型预制体")]
    [SerializeField] private GameObject visualPrefab;

    [Header("动画参数")]
    [LabelText("待机状态名")]
    [SerializeField] private string idleStateName = "idle1";

    [LabelText("行走状态名")]
    [SerializeField] private string walkStateName = "walk";

    [LabelText("奔跑状态名")]
    [SerializeField] private string runStateName = "run";

    [LabelText("切换过渡时间")]
    [SerializeField] private float crossFadeDuration = 0.15f;

    [LabelText("移动判定阈值")]
    [SerializeField] private float moveThreshold = 0.05f;

    [LabelText("移动时朝向移动方向")]
    [SerializeField] private bool faceMoveDirection = true;

    [LabelText("转向速度")]
    [SerializeField] private float rotationSpeed = 12f;

    [LabelText("模型朝向偏移")]
    [SerializeField] private float modelForwardYawOffset;

    private Transform cameraPitchPivot;
    private Animator animator;
    private int idleStateHash;
    private int walkStateHash;
    private int runStateHash;
    private int currentStateHash;

    public Camera PlayerCamera => playerCamera;

    private void OnValidate()
    {
        cameraDistance = Mathf.Max(0.5f, cameraDistance);
        maxPitch = Mathf.Max(minPitch, maxPitch);
        crossFadeDuration = Mathf.Max(0f, crossFadeDuration);
        moveThreshold = Mathf.Max(0f, moveThreshold);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
        RefreshAnimationHashes();
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
        EnsureThirdPersonVisual();
        EnsureAnimatorFromVisual();
        RefreshAnimationHashes();
    }

    public void Enter(float yaw, float pitch)
    {
        Initialize();
        SetVisualVisible(true);
        RefreshCamera(yaw, pitch);
    }

    public void Exit()
    {
        ResetMovementState();
        SetVisualVisible(false);
    }

    public void HandleLookInput(ref float yaw, ref float pitch)
    {
        Vector2 mouseDelta = RuntimeInput.GetMouseDelta();
        yaw += mouseDelta.x * mouseSensitivity;
        pitch -= mouseDelta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
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

    public Vector3 GetMoveDirection(Vector3 input, float yaw)
    {
        if (input.sqrMagnitude <= 0.0001f)
        {
            return Vector3.zero;
        }

        Quaternion cameraYawRotation = Quaternion.Euler(0f, yaw, 0f);
        Vector3 forward = cameraYawRotation * Vector3.forward;
        Vector3 right = cameraYawRotation * Vector3.right;
        return Vector3.ClampMagnitude(forward * input.z + right * input.x, 1f);
    }

    public void SetMovementState(Vector3 moveDirection, float horizontalSpeed, bool isRunning, bool isGrounded)
    {
        EnsureAnimatorFromVisual();

        bool isMoving = isGrounded
            && horizontalSpeed > moveThreshold
            && moveDirection.sqrMagnitude > moveThreshold * moveThreshold;

        if (!isMoving)
        {
            PlayAnimationState(idleStateHash, false);
            return;
        }

        PlayAnimationState(isRunning ? runStateHash : walkStateHash, false);

        if (faceMoveDirection)
        {
            FaceMoveDirection(moveDirection);
        }
    }

    public void ResetMovementState()
    {
        EnsureAnimatorFromVisual();
        PlayAnimationState(idleStateHash, true);
    }

    public void RefreshCamera(float yaw, float pitch)
    {
        if (cameraPitchPivot == null || playerCamera == null)
        {
            return;
        }

        float localYaw = Mathf.DeltaAngle(transform.eulerAngles.y, yaw);
        cameraPitchPivot.localPosition = pivotLocalPosition;
        cameraPitchPivot.localRotation = Quaternion.Euler(pitch, localYaw, 0f);
        playerCamera.transform.localPosition = new Vector3(0f, 0f, -cameraDistance);
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

        if (playerCamera != null)
        {
            playerCamera.transform.SetParent(cameraPitchPivot, false);
        }
    }

    private void EnsureThirdPersonVisual()
    {
        if (visualRoot == null && visualPrefab != null)
        {
            GameObject visualObject = InstantiateVisualPrefab();
            if (visualObject == null)
            {
                Debug.LogWarning($"第三人称模型预制体无法实例化为 GameObject：{visualPrefab.name}", this);
            }
            else
            {
                visualObject.transform.SetParent(transform, false);
                visualObject.name = visualPrefab.name;
                visualObject.transform.localPosition = Vector3.zero;
                visualObject.transform.localRotation = Quaternion.identity;
                visualObject.transform.localScale = Vector3.one;
                visualRoot = visualObject.transform;
                DisableEmbeddedCameras(visualObject);
            }
        }

        if (visualRoot == null)
        {
            GameObject visualObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visualObject.name = "Third Person Visual";
            visualObject.transform.SetParent(transform, false);
            visualObject.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            visualObject.transform.localScale = new Vector3(0.65f, 0.9f, 0.65f);
            visualRoot = visualObject.transform;

            if (visualObject.TryGetComponent(out Collider visualCollider))
            {
                Destroy(visualCollider);
            }
        }

        SetVisualVisible(false);
    }

    private GameObject InstantiateVisualPrefab()
    {
        UnityEngine.Object clonedObject = Instantiate((UnityEngine.Object)visualPrefab);

        if (clonedObject is GameObject clonedGameObject)
        {
            return clonedGameObject;
        }

        if (clonedObject is Component clonedComponent)
        {
            return clonedComponent.gameObject;
        }

        if (clonedObject != null)
        {
            Destroy(clonedObject);
        }

        return null;
    }

    private void EnsureAnimatorFromVisual()
    {
        if (animator != null || visualRoot == null)
        {
            return;
        }

        animator = visualRoot.GetComponentInChildren<Animator>(true);
    }

    private void RefreshAnimationHashes()
    {
        idleStateHash = string.IsNullOrEmpty(idleStateName) ? 0 : Animator.StringToHash(idleStateName);
        walkStateHash = string.IsNullOrEmpty(walkStateName) ? 0 : Animator.StringToHash(walkStateName);
        runStateHash = string.IsNullOrEmpty(runStateName) ? 0 : Animator.StringToHash(runStateName);
    }

    private void PlayAnimationState(int stateHash, bool force)
    {
        if (animator == null || animator.runtimeAnimatorController == null || stateHash == 0)
        {
            return;
        }

        if (!animator.isActiveAndEnabled || !animator.HasState(0, stateHash))
        {
            return;
        }

        if (!force && currentStateHash == stateHash)
        {
            return;
        }

        if (crossFadeDuration <= 0f)
        {
            animator.Play(stateHash, 0);
        }
        else
        {
            animator.CrossFade(stateHash, crossFadeDuration, 0);
        }

        currentStateHash = stateHash;
    }

    private void FaceMoveDirection(Vector3 moveDirection)
    {
        if (visualRoot == null)
        {
            return;
        }

        moveDirection.y = 0f;
        if (moveDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion targetWorldRotation = Quaternion.LookRotation(moveDirection.normalized, Vector3.up)
            * Quaternion.Euler(0f, modelForwardYawOffset, 0f);
        Quaternion targetLocalRotation = Quaternion.Inverse(transform.rotation) * targetWorldRotation;
        visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, targetLocalRotation, Time.deltaTime * rotationSpeed);
    }

    private void SetVisualVisible(bool visible)
    {
        if (visualRoot != null)
        {
            visualRoot.gameObject.SetActive(visible);
        }
    }

    private static void DisableEmbeddedCameras(GameObject visualObject)
    {
        foreach (Camera embeddedCamera in visualObject.GetComponentsInChildren<Camera>(true))
        {
            embeddedCamera.enabled = false;
        }

        foreach (AudioListener embeddedListener in visualObject.GetComponentsInChildren<AudioListener>(true))
        {
            embeddedListener.enabled = false;
        }
    }
}
