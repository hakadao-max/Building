using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerFixedCameraView : MonoBehaviour
{
    [Header("视角参数")]
    [LabelText("玩家相机")]
    [SerializeField] private Camera playerCamera;

    [LabelText("固定视角点")]
    [SerializeField] private List<PlayerFixedCameraPoint> cameraPoints = new List<PlayerFixedCameraPoint>();

    [LabelText("鼠标灵敏度")]
    [SerializeField] private float mouseSensitivity = 1.2f;

    [LabelText("最大水平旋转")]
    [SerializeField] private float maxYawOffset = 20f;

    [LabelText("最大垂直旋转")]
    [SerializeField] private float maxPitchOffset = 12f;

    [LabelText("选择后隐藏面板")]
    [SerializeField] private bool hidePanelAfterSelection;

    private Action<int> onPointSelected;
    private PlayerFixedCameraPoint activePoint;
    private Quaternion baseRotation = Quaternion.identity;
    private float yawOffset;
    private float pitchOffset;
    private int activePointIndex = -1;
    private bool isActive;
    private bool isSelectionPanelVisible;
    private bool initialized;

    public Camera PlayerCamera => playerCamera;
    public bool IsActive => isActive;
    public bool IsSelectionPanelVisible => isSelectionPanelVisible;

    private void OnValidate()
    {
        mouseSensitivity = Mathf.Max(0f, mouseSensitivity);
        maxYawOffset = Mathf.Max(0f, maxYawOffset);
        maxPitchOffset = Mathf.Max(0f, maxPitchOffset);
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
        HideSelectionPanel();
    }

    public bool ToggleSelectionPanel(Action<int> selectCallback)
    {
        if (isSelectionPanelVisible)
        {
            HideSelectionPanel();
            return false;
        }

        ShowSelectionPanel(selectCallback);
        return true;
    }

    public void ShowSelectionPanel(Action<int> selectCallback)
    {
        onPointSelected = selectCallback;
        FixedCameraPanel panel = UIManager.GetPanel<FixedCameraPanel>(UIPanelNames.FixedCamera);
        if (panel == null)
        {
            isSelectionPanelVisible = false;
            return;
        }

        panel.Configure(cameraPoints, activePointIndex, SelectPoint);
        isSelectionPanelVisible = UIManager.ShowPanel(UIPanelNames.FixedCamera);
    }

    public void HideSelectionPanel()
    {
        if (UIManager.GetPanel<FixedCameraPanel>(UIPanelNames.FixedCamera) != null)
        {
            UIManager.HidePanel(UIPanelNames.FixedCamera);
        }

        isSelectionPanelVisible = false;
    }

    public bool Enter(int pointIndex)
    {
        if (!TryGetPoint(pointIndex, out PlayerFixedCameraPoint point) || !point.TryGetPose(out Vector3 position, out Quaternion rotation))
        {
            return false;
        }

        EnsureCamera();
        if (hidePanelAfterSelection)
        {
            HideSelectionPanel();
        }

        activePoint = point;
        activePointIndex = pointIndex;
        baseRotation = rotation;
        yawOffset = 0f;
        pitchOffset = 0f;
        isActive = true;

        ApplyCameraPose(position);
        RefreshButtonSelection();
        return true;
    }

    public bool HasUsablePoint(int pointIndex)
    {
        return TryGetPoint(pointIndex, out PlayerFixedCameraPoint point) && point.HasViewPoint;
    }

    public void Exit()
    {
        isActive = false;
        activePoint = null;
        activePointIndex = -1;
        HideSelectionPanel();
    }

    public void HandleLookInput()
    {
        if (!isActive)
        {
            return;
        }

        Vector2 mouseDelta = RuntimeInput.GetMouseDelta();
        yawOffset = Mathf.Clamp(yawOffset + mouseDelta.x * mouseSensitivity, -maxYawOffset, maxYawOffset);
        pitchOffset = Mathf.Clamp(pitchOffset - mouseDelta.y * mouseSensitivity, -maxPitchOffset, maxPitchOffset);
    }

    public void RefreshCamera()
    {
        if (!isActive || activePoint == null || !activePoint.TryGetPose(out Vector3 position, out Quaternion rotation))
        {
            return;
        }

        baseRotation = rotation;
        ApplyCameraPose(position);
    }

    private void ApplyCameraPose(Vector3 position)
    {
        if (playerCamera == null)
        {
            return;
        }

        playerCamera.transform.SetParent(null, true);
        playerCamera.transform.SetPositionAndRotation(position, baseRotation * Quaternion.Euler(pitchOffset, yawOffset, 0f));
    }

    private bool TryGetPoint(int pointIndex, out PlayerFixedCameraPoint point)
    {
        if (cameraPoints != null && pointIndex >= 0 && pointIndex < cameraPoints.Count && cameraPoints[pointIndex] != null)
        {
            point = cameraPoints[pointIndex];
            return true;
        }

        point = null;
        return false;
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

    private void SelectPoint(int pointIndex)
    {
        if (hidePanelAfterSelection)
        {
            HideSelectionPanel();
        }

        onPointSelected?.Invoke(pointIndex);
    }

    private void RefreshButtonSelection()
    {
        FixedCameraPanel panel = UIManager.GetPanel<FixedCameraPanel>(UIPanelNames.FixedCamera);
        if (panel != null)
        {
            panel.RefreshSelection(activePointIndex);
        }
    }

}
