using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerFixedRouteRoamView : MonoBehaviour
{
    private const string CameraPitchPivotName = "Camera Pitch Pivot";
    private const float TangentSampleOffset = 0.0025f;
    private const float MinimumRouteLength = 0.001f;

    [LabelText("玩家相机")]
    [SerializeField] private Camera playerCamera;

    [LabelText("控制点")]
    [SerializeField] private List<Vector3> controlPoints = new List<Vector3>
    {
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 4f),
        new Vector3(3f, 0f, 8f),
        new Vector3(0f, 0f, 12f)
    };

    [LabelText("漫游速度")]
    [SerializeField] private float roamSpeed = 2f;

    [LabelText("循环播放")]
    [SerializeField] private bool loop;

    [LabelText("首尾相连")]
    [SerializeField] private bool connectLastToFirst = true;

    [LabelText("进入时对齐起点")]
    [SerializeField] private bool snapToStart = true;

    [LabelText("沿路线方向旋转")]
    [SerializeField] private bool rotateAlongPath = true;

    [LabelText("鼠标灵敏度")]
    [SerializeField] private float mouseSensitivity = 2.2f;

    [LabelText("自由视角俯仰最小角度")]
    [SerializeField] private float minPitch = -55f;

    [LabelText("自由视角俯仰最大角度")]
    [SerializeField] private float maxPitch = 70f;

    [LabelText("相机局部位置")]
    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0f, 1.65f, 0f);

    [LabelText("结束后回到第一人称")]
    [SerializeField] private bool returnToFirstPersonWhenFinished = true;

    [LabelText("显示路线预览")]
    [SerializeField] private bool showPreview = true;

    [LabelText("预览采样数")]
    [SerializeField] private int previewSampleCount = 48;

    [LabelText("长度采样数")]
    [SerializeField] private int lengthSampleCount = 128;

    [LabelText("预览颜色")]
    [SerializeField] private Color previewColor = new Color(1f, 0.68f, 0.1f, 1f);

    [LabelText("预览线宽")]
    [SerializeField] private float previewLineWidth = 4f;

    [LabelText("控制点半径")]
    [SerializeField] private float controlPointRadius = 0.2f;

    private CharacterController characterController;
    private Transform cameraPitchPivot;
    private float routeLength;
    private float traveledDistance;
    private bool isRoaming;

    public Camera PlayerCamera => playerCamera;
    public bool ReturnToFirstPersonWhenFinished => returnToFirstPersonWhenFinished;
    public bool UsesFreeLook => !rotateAlongPath;
    public int ControlPointCount => controlPoints != null ? controlPoints.Count : 0;

    private void OnValidate()
    {
        roamSpeed = Mathf.Max(0.01f, roamSpeed);
        maxPitch = Mathf.Max(minPitch, maxPitch);
        previewSampleCount = Mathf.Max(4, previewSampleCount);
        lengthSampleCount = Mathf.Max(8, lengthSampleCount);
        previewLineWidth = Mathf.Max(1f, previewLineWidth);
        controlPointRadius = Mathf.Max(0.05f, controlPointRadius);

        if (controlPoints == null)
        {
            controlPoints = new List<Vector3>();
        }
    }

    private void OnDrawGizmos()
    {
        DrawPreview();
    }

    private void OnDrawGizmosSelected()
    {
        DrawPreview();
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
        characterController = GetComponent<CharacterController>();
        EnsureCameraRig();
    }

    public void Enter(float yaw, float pitch)
    {
        Initialize();
        routeLength = CalculateRouteLength();
        traveledDistance = 0f;
        isRoaming = HasUsableRoute;

        if (!isRoaming || routeLength <= MinimumRouteLength)
        {
            isRoaming = false;
            return;
        }

        if (snapToStart)
        {
            ApplyRoutePose(0f, yaw);
        }

        RefreshCamera(pitch);
    }

    public void Exit()
    {
        isRoaming = false;
    }

    public void HandleLookInput(ref float yaw, ref float pitch)
    {
        if (rotateAlongPath)
        {
            return;
        }

        Vector2 mouseDelta = RuntimeInput.GetMouseDelta();
        yaw += mouseDelta.x * mouseSensitivity;
        pitch -= mouseDelta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    public bool TickRoam(float yaw, float pitch, out bool finished)
    {
        finished = false;

        if (!isRoaming || !HasUsableRoute || routeLength <= MinimumRouteLength)
        {
            return false;
        }

        traveledDistance += roamSpeed * Time.deltaTime;
        float routeDistance = traveledDistance;
        float normalizedTime;

        if (loop)
        {
            traveledDistance = Mathf.Repeat(traveledDistance, routeLength);
            routeDistance = traveledDistance;
            normalizedTime = GetNormalizedTimeAtDistance(routeDistance);
        }
        else if (traveledDistance >= routeLength)
        {
            traveledDistance = routeLength;
            routeDistance = routeLength;
            normalizedTime = 1f;
            finished = true;
        }
        else
        {
            normalizedTime = GetNormalizedTimeAtDistance(routeDistance);
        }

        ApplyRoutePose(normalizedTime, yaw);
        RefreshCamera(pitch);

        if (finished)
        {
            isRoaming = false;
        }

        return true;
    }

    public Vector3 GetControlPointWorldPosition(int index)
    {
        if (!IsValidControlPointIndex(index))
        {
            return transform.position;
        }

        return controlPoints[index];
    }

    public void SetControlPointWorldPosition(int index, Vector3 worldPosition)
    {
        if (!IsValidControlPointIndex(index))
        {
            return;
        }

        controlPoints[index] = worldPosition;
    }

    public void AddControlPointAfterLast()
    {
        if (controlPoints == null)
        {
            controlPoints = new List<Vector3>();
        }

        Vector3 nextPoint = controlPoints.Count > 0
            ? controlPoints[controlPoints.Count - 1] + Vector3.forward * 3f
            : Vector3.zero;
        controlPoints.Add(nextPoint);
    }

    public void ResetDefaultRoute()
    {
        controlPoints = new List<Vector3>
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 0f, 4f),
            new Vector3(3f, 0f, 8f),
            new Vector3(0f, 0f, 12f)
        };
    }

    public bool TryEvaluatePreviewPosition(float normalizedTime, out Vector3 position)
    {
        if (!HasUsableRoute)
        {
            position = transform.position;
            return false;
        }

        position = EvaluateWorldPosition(normalizedTime);
        return true;
    }

    private bool HasUsableRoute => controlPoints != null && controlPoints.Count >= 2;

    private void ApplyRoutePose(float normalizedTime, float yaw)
    {
        Vector3 position = EvaluateWorldPosition(normalizedTime);
        Quaternion rotation = rotateAlongPath ? transform.rotation : Quaternion.Euler(0f, yaw, 0f);

        if (rotateAlongPath && TryGetRuntimeTangent(normalizedTime, out Vector3 tangent))
        {
            tangent.y = 0f;
            if (tangent.sqrMagnitude > 0.0001f)
            {
                rotation = Quaternion.LookRotation(tangent.normalized, Vector3.up);
            }
        }

        SetPlayerPose(position, rotation);
    }

    private void RefreshCamera(float pitch)
    {
        if (cameraPitchPivot == null || playerCamera == null)
        {
            return;
        }

        cameraPitchPivot.localPosition = cameraLocalPosition;
        cameraPitchPivot.localRotation = rotateAlongPath ? Quaternion.identity : Quaternion.Euler(pitch, 0f, 0f);
        playerCamera.transform.localPosition = Vector3.zero;
        playerCamera.transform.localRotation = Quaternion.identity;
    }

    private Vector3 EvaluateWorldPosition(float normalizedTime)
    {
        if (controlPoints == null || controlPoints.Count == 0)
        {
            return Vector3.zero;
        }

        if (controlPoints.Count == 1)
        {
            return controlPoints[0];
        }

        normalizedTime = Mathf.Clamp01(normalizedTime);

        if (connectLastToFirst)
        {
            float scaledTime = normalizedTime * controlPoints.Count;
            int pointIndex = Mathf.FloorToInt(scaledTime);
            float segmentTime = scaledTime - Mathf.Floor(scaledTime);

            if (pointIndex >= controlPoints.Count)
            {
                pointIndex = controlPoints.Count - 1;
                segmentTime = 1f;
            }

            return CatmullRom(
                GetLoopedControlPoint(pointIndex - 1),
                GetLoopedControlPoint(pointIndex),
                GetLoopedControlPoint(pointIndex + 1),
                GetLoopedControlPoint(pointIndex + 2),
                segmentTime);
        }

        float clampedScaledTime = normalizedTime * (controlPoints.Count - 1);
        int segmentIndex = Mathf.Min(Mathf.FloorToInt(clampedScaledTime), controlPoints.Count - 2);
        float clampedSegmentTime = clampedScaledTime - segmentIndex;

        return CatmullRom(
            GetClampedControlPoint(segmentIndex - 1),
            GetClampedControlPoint(segmentIndex),
            GetClampedControlPoint(segmentIndex + 1),
            GetClampedControlPoint(segmentIndex + 2),
            clampedSegmentTime);
    }

    private bool TryGetRuntimeTangent(float normalizedTime, out Vector3 tangent)
    {
        if (!HasUsableRoute)
        {
            tangent = Vector3.zero;
            return false;
        }

        float beforeTime = normalizedTime - TangentSampleOffset;
        float afterTime = normalizedTime + TangentSampleOffset;

        if (!loop)
        {
            beforeTime = Mathf.Clamp01(beforeTime);
            afterTime = Mathf.Clamp01(afterTime);
        }

        tangent = EvaluateWorldPosition(afterTime) - EvaluateWorldPosition(beforeTime);
        return tangent.sqrMagnitude > 0.0001f;
    }

    private float CalculateRouteLength()
    {
        if (!HasUsableRoute)
        {
            return 0f;
        }

        int sampleCount = Mathf.Max(8, lengthSampleCount);
        Vector3 previous = EvaluateWorldPosition(0f);
        float length = 0f;

        for (int i = 1; i <= sampleCount; i++)
        {
            float normalizedTime = i / (float)sampleCount;
            Vector3 current = EvaluateWorldPosition(normalizedTime);
            length += Vector3.Distance(previous, current);
            previous = current;
        }

        return length;
    }

    private float GetNormalizedTimeAtDistance(float targetDistance)
    {
        if (targetDistance <= 0f)
        {
            return 0f;
        }

        if (targetDistance >= routeLength)
        {
            return 1f;
        }

        int sampleCount = Mathf.Max(8, lengthSampleCount);
        Vector3 previous = EvaluateWorldPosition(0f);
        float accumulatedDistance = 0f;

        for (int i = 1; i <= sampleCount; i++)
        {
            float currentTime = i / (float)sampleCount;
            Vector3 current = EvaluateWorldPosition(currentTime);
            float segmentLength = Vector3.Distance(previous, current);
            float nextDistance = accumulatedDistance + segmentLength;

            if (targetDistance <= nextDistance)
            {
                float previousTime = (i - 1) / (float)sampleCount;
                float segmentRatio = segmentLength <= 0.0001f ? 0f : (targetDistance - accumulatedDistance) / segmentLength;
                return Mathf.Lerp(previousTime, currentTime, segmentRatio);
            }

            accumulatedDistance = nextDistance;
            previous = current;
        }

        return 1f;
    }

    private Vector3 GetClampedControlPoint(int index)
    {
        return controlPoints[Mathf.Clamp(index, 0, controlPoints.Count - 1)];
    }

    private Vector3 GetLoopedControlPoint(int index)
    {
        int count = controlPoints.Count;
        return controlPoints[(index % count + count) % count];
    }

    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * ((2f * p1)
            + (-p0 + p2) * t
            + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2
            + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }

    private bool IsValidControlPointIndex(int index)
    {
        return controlPoints != null && index >= 0 && index < controlPoints.Count;
    }

    private void SetPlayerPose(Vector3 position, Quaternion rotation)
    {
        if (characterController != null)
        {
            bool wasEnabled = characterController.enabled;
            characterController.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            characterController.enabled = wasEnabled;
            return;
        }

        transform.SetPositionAndRotation(position, rotation);
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

    private void DrawPreview()
    {
        if (!showPreview || !HasUsableRoute)
        {
            return;
        }

        Gizmos.color = previewColor;

        for (int i = 0; i < controlPoints.Count; i++)
        {
            Gizmos.DrawSphere(controlPoints[i], controlPointRadius);
        }

        Vector3 previous = EvaluateWorldPosition(0f);
        int sampleCount = Mathf.Max(4, previewSampleCount);
        for (int i = 1; i <= sampleCount; i++)
        {
            float normalizedTime = i / (float)sampleCount;
            Vector3 current = EvaluateWorldPosition(normalizedTime);
            DrawPreviewLine(previous, current);
            previous = current;
        }
    }

    private void DrawPreviewLine(Vector3 from, Vector3 to)
    {
#if UNITY_EDITOR
        Handles.color = previewColor;
        Handles.DrawAAPolyLine(previewLineWidth, from, to);
#else
        Gizmos.DrawLine(from, to);
#endif
    }
}
