using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(InteractableArea))]
[DefaultExecutionOrder(1000)]
public sealed class PrefabSwapInteractable : MonoBehaviour
{
    [LabelText("替换目标父物体")]
    [SerializeField] private Transform targetParent;

    [LabelText("可选预制体")]
    [SerializeField] private List<PrefabSwapOption> prefabOptions = new List<PrefabSwapOption>();

    [LabelText("预制体交换界面")]
    [SerializeField] private PrefabSwapPanelView panelView;

    [LabelText("玩家控制器")]
    [SerializeField] private SimplePlayerController playerController;

    [LabelText("预览相机")]
    [SerializeField] private Camera previewCamera;

    [LabelText("预览相机世界位置")]
    [SerializeField] private Vector3 previewCameraPosition;

    [LabelText("预览相机世界旋转")]
    [SerializeField] private Vector3 previewCameraEulerAngles;

    private readonly List<UnityAction> optionActions = new List<UnityAction>();
    private bool isPanelOpen;
    private bool playerInRange;
    private bool hasSavedCameraPose;
    private Vector3 savedCameraPosition;
    private Quaternion savedCameraRotation;

    private void Awake()
    {
        BindPanel();
        SetPanelOpen(false);
        SetPromptVisible(false);
    }

    private void OnDisable()
    {
        if (isPanelOpen)
        {
            SetPanelOpen(false);
        }

        SetPromptVisible(false);
        playerInRange = false;
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

    private void OnTriggerEnter(Collider other)
    {
        SimplePlayerController controller = other.GetComponentInParent<SimplePlayerController>();
        if (controller == null)
        {
            return;
        }

        playerController = controller;
        playerInRange = true;
        SetPromptVisible(!isPanelOpen);
    }

    private void OnTriggerExit(Collider other)
    {
        SimplePlayerController controller = other.GetComponentInParent<SimplePlayerController>();
        if (controller == null || (playerController != null && controller != playerController))
        {
            return;
        }

        playerInRange = false;
        SetPromptVisible(false);
    }

    public void ObjectClicked()
    {
        OpenPanel();
    }

    public void OpenPanel()
    {
        if (panelView == null)
        {
            Debug.LogWarning("预制体交换器未指定界面 Prefab 实例。", this);
            return;
        }

        ResolvePlayerController();
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
        if (panelView == null)
        {
            return;
        }

        if (panelView.CloseButton != null)
        {
            panelView.CloseButton.onClick.AddListener(ClosePanel);
        }

        int slotCount = panelView.OptionSlotCount;
        for (int i = 0; i < slotCount; i++)
        {
            PrefabSwapOption option = i < prefabOptions.Count ? prefabOptions[i] : null;
            GameObject prefab = option != null ? option.Prefab : null;
            panelView.ConfigureOptionSlot(i, option != null ? option.DisplayName : string.Empty, prefab != null);

            Button button = panelView.GetOptionButton(i);
            if (button == null || prefab == null)
            {
                optionActions.Add(null);
                continue;
            }

            int capturedIndex = i;
            UnityAction action = () => SelectPrefab(capturedIndex);
            button.onClick.AddListener(action);
            optionActions.Add(action);
        }

        if (prefabOptions.Count > slotCount)
        {
            Debug.LogWarning($"界面只有 {slotCount} 个选择按钮，后面的预制体不会显示。", this);
        }
    }

    private void UnbindPanel()
    {
        if (panelView == null)
        {
            return;
        }

        if (panelView.CloseButton != null)
        {
            panelView.CloseButton.onClick.RemoveListener(ClosePanel);
        }

        for (int i = 0; i < optionActions.Count; i++)
        {
            Button button = panelView.GetOptionButton(i);
            UnityAction action = optionActions[i];
            if (button != null && action != null)
            {
                button.onClick.RemoveListener(action);
            }
        }

        optionActions.Clear();
    }

    private void SetPanelOpen(bool open)
    {
        bool wasOpen = isPanelOpen;
        isPanelOpen = open && panelView != null;

        if (panelView != null)
        {
            panelView.SetPanelVisible(isPanelOpen);
            panelView.SetPromptVisible(playerInRange && !isPanelOpen);
        }

        ResolvePlayerController();
        if (playerController != null)
        {
            playerController.SetExternalInputLocked(isPanelOpen);
        }

        if (isPanelOpen)
        {
            if (!wasOpen)
            {
                BeginCameraPreview();
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (wasOpen)
        {
            EndCameraPreview();
        }
    }

    private void SetPromptVisible(bool visible)
    {
        if (panelView != null)
        {
            panelView.SetPromptVisible(visible);
        }
    }

    private void ResolvePlayerController()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<SimplePlayerController>();
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
