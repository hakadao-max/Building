using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(InteractableArea))]
[DefaultExecutionOrder(1000)]
public sealed class PrefabSwapInteractable : MonoBehaviour
{
    [LabelText("替换目标父物体")]
    [SerializeField] private Transform targetParent;

    [LabelText("可选预制体")]
    [SerializeField] private List<PrefabSwapOption> prefabOptions = new List<PrefabSwapOption>();

    [LabelText("预览相机")]
    [SerializeField] private Camera previewCamera;

    [LabelText("预览相机世界位置")]
    [SerializeField] private Vector3 previewCameraPosition;

    [LabelText("预览相机世界旋转")]
    [SerializeField] private Vector3 previewCameraEulerAngles;

    private bool isPanelOpen;
    private bool hasSavedCameraPose;
    private Vector3 savedCameraPosition;
    private Quaternion savedCameraRotation;

    private PrefabSwapPanel Panel => UIManager.GetPanel<PrefabSwapPanel>(UIPanelNames.PrefabSwap);

    private void Awake()
    {
        BindPanel();
        SetPanelOpen(false);
    }

    private void OnDisable()
    {
        if (isPanelOpen)
        {
            SetPanelOpen(false);
        }

    }

    private void OnDestroy()
    {
        UnbindPanel();
    }

    private void LateUpdate()
    {
        if (isPanelOpen)
        {
            ApplyPreviewCameraPose();
        }
    }

    public void ObjectClicked()
    {
        OpenPanel();
    }

    public void OpenPanel()
    {
        if (Panel == null)
        {
            Debug.LogWarning("预制体交换器未指定界面 Prefab 实例。", this);
            return;
        }

        SetPanelOpen(true);
    }

    public void ClosePanel()
    {
        SetPanelOpen(false);
    }

    public void SelectPrefab(int optionIndex)
    {
        if (targetParent == null)
        {
            Debug.LogWarning("预制体交换器未指定替换目标父物体。", this);
            return;
        }

        if (optionIndex < 0 || optionIndex >= prefabOptions.Count)
        {
            return;
        }

        PrefabSwapOption option = prefabOptions[optionIndex];
        if (option == null || option.Prefab == null)
        {
            return;
        }

        for (int i = targetParent.childCount - 1; i >= 0; i--)
        {
            Destroy(targetParent.GetChild(i).gameObject);
        }

        GameObject instance = Instantiate(option.Prefab, targetParent, false);
        instance.name = option.Prefab.name;
        instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        instance.transform.localScale = Vector3.one;
    }

    private void BindPanel()
    {
        PrefabSwapPanel panelView = Panel;
        if (panelView == null)
        {
            return;
        }

        if (panelView.CloseButton != null)
        {
            panelView.CloseButton.onClick.AddListener(ClosePanel);
        }

        panelView.SetOptions(prefabOptions, SelectPrefab);
    }

    private void UnbindPanel()
    {
        PrefabSwapPanel panelView = Panel;
        if (panelView == null)
        {
            return;
        }

        if (panelView.CloseButton != null)
        {
            panelView.CloseButton.onClick.RemoveListener(ClosePanel);
        }

        panelView.ClearOptions();
    }

    private void SetPanelOpen(bool open)
    {
        PrefabSwapPanel panelView = Panel;
        bool wasOpen = isPanelOpen;
        isPanelOpen = open && panelView != null;

        if (panelView != null)
        {
            UIManager.SetPanelVisible(UIPanelNames.PrefabSwap, isPanelOpen);
        }

        if (isPanelOpen)
        {
            if (!wasOpen)
            {
                BeginCameraPreview();
            }

        }
        else if (wasOpen)
        {
            EndCameraPreview();
        }
    }

    private void BeginCameraPreview()
    {
        ResolvePreviewCamera();
        if (previewCamera == null)
        {
            Debug.LogWarning("预制体交换器未找到预览相机。", this);
            return;
        }

        savedCameraPosition = previewCamera.transform.position;
        savedCameraRotation = previewCamera.transform.rotation;
        hasSavedCameraPose = true;
        ApplyPreviewCameraPose();
    }

    private void ApplyPreviewCameraPose()
    {
        ResolvePreviewCamera();
        if (previewCamera != null)
        {
            previewCamera.transform.SetPositionAndRotation(
                previewCameraPosition,
                Quaternion.Euler(previewCameraEulerAngles));
        }
    }

    private void EndCameraPreview()
    {
        if (previewCamera != null && hasSavedCameraPose)
        {
            previewCamera.transform.SetPositionAndRotation(savedCameraPosition, savedCameraRotation);
        }

        hasSavedCameraPose = false;
    }

    private void ResolvePreviewCamera()
    {
        if (previewCamera == null)
        {
            previewCamera = Camera.main;
        }
    }
}
