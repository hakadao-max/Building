using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;

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

    [Header("底部UI参数")]
    [LabelText("UI画布")]
    [SerializeField] private Canvas selectionCanvas;

    [LabelText("按钮根节点")]
    [SerializeField] private RectTransform buttonContainer;

    [LabelText("按钮尺寸")]
    [SerializeField] private Vector2 buttonSize = new Vector2(84f, 84f);

    [LabelText("按钮间距")]
    [SerializeField] private float buttonSpacing = 16f;

    [LabelText("底部边距")]
    [SerializeField] private float bottomMargin = 42f;

    [LabelText("背景颜色")]
    [SerializeField] private Color panelBackgroundColor = new Color(0f, 0f, 0f, 0.45f);

    [LabelText("按钮颜色")]
    [SerializeField] private Color buttonColor = new Color(1f, 1f, 1f, 0.9f);

    [LabelText("选中按钮颜色")]
    [SerializeField] private Color selectedButtonColor = new Color(0.35f, 0.75f, 1f, 1f);

    private readonly List<Button> runtimeButtons = new List<Button>();
    private Action<int> onPointSelected;
    private PlayerFixedCameraPoint activePoint;
    private Quaternion baseRotation = Quaternion.identity;
    private float yawOffset;
    private float pitchOffset;
    private int activePointIndex = -1;
    private bool isActive;
    private bool isSelectionPanelVisible;

    public Camera PlayerCamera => playerCamera;
    public bool IsActive => isActive;
    public bool IsSelectionPanelVisible => isSelectionPanelVisible;

    private void OnValidate()
    {
        mouseSensitivity = Mathf.Max(0f, mouseSensitivity);
        maxYawOffset = Mathf.Max(0f, maxYawOffset);
        maxPitchOffset = Mathf.Max(0f, maxPitchOffset);
        buttonSize.x = Mathf.Max(32f, buttonSize.x);
        buttonSize.y = Mathf.Max(32f, buttonSize.y);
        buttonSpacing = Mathf.Max(0f, buttonSpacing);
        bottomMargin = Mathf.Max(0f, bottomMargin);
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
        EnsureSelectionCanvas();
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
        EnsureSelectionCanvas();
        RebuildButtons();

        if (selectionCanvas != null)
        {
            selectionCanvas.gameObject.SetActive(true);
        }

        isSelectionPanelVisible = true;
        EnsureEventSystem();
    }

    public void HideSelectionPanel()
    {
        if (selectionCanvas != null)
        {
            selectionCanvas.gameObject.SetActive(false);
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
        HideSelectionPanel();

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

    private void EnsureSelectionCanvas()
    {
        if (selectionCanvas != null && buttonContainer != null)
        {
            return;
        }

        if (selectionCanvas == null)
        {
            GameObject canvasObject = new GameObject("Fixed Camera Selection Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            selectionCanvas = canvasObject.GetComponent<Canvas>();
            selectionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            selectionCanvas.sortingOrder = 950;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (buttonContainer == null)
        {
            GameObject panelObject = new GameObject("Fixed Camera Button Row", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            buttonContainer = panelObject.GetComponent<RectTransform>();
            buttonContainer.SetParent(selectionCanvas.transform, false);
            buttonContainer.anchorMin = new Vector2(0.5f, 0f);
            buttonContainer.anchorMax = new Vector2(0.5f, 0f);
            buttonContainer.pivot = new Vector2(0.5f, 0f);
            buttonContainer.anchoredPosition = new Vector2(0f, bottomMargin);

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = panelBackgroundColor;

            HorizontalLayoutGroup layoutGroup = panelObject.GetComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.spacing = buttonSpacing;
            layoutGroup.padding = new RectOffset(20, 20, 12, 12);
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
        }
    }

    private void RebuildButtons()
    {
        if (buttonContainer == null)
        {
            return;
        }

        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        runtimeButtons.Clear();

        int pointCount = cameraPoints != null ? cameraPoints.Count : 0;
        if (pointCount <= 0)
        {
            CreateMessageItem("未配置固定视角");
            ResizePanel(1);
            return;
        }

        for (int i = 0; i < pointCount; i++)
        {
            CreateButton(i);
        }

        ResizePanel(pointCount);
        RefreshButtonSelection();
    }

    private void CreateButton(int pointIndex)
    {
        PlayerFixedCameraPoint point = cameraPoints[pointIndex];
        GameObject buttonObject = new GameObject($"Fixed Camera {pointIndex + 1:00}", typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.SetParent(buttonContainer, false);
        buttonRect.sizeDelta = buttonSize;

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = buttonColor;

        Button button = buttonObject.GetComponent<Button>();
        button.interactable = point != null && point.HasViewPoint;

        int capturedIndex = pointIndex;
        button.onClick.AddListener(() => SelectPoint(capturedIndex));
        runtimeButtons.Add(button);

        if (point != null && point.Icon != null)
        {
            CreateIcon(buttonRect, point.Icon);
        }
        else
        {
            CreateLabel(buttonRect, point != null ? point.DisplayName : $"视角 {pointIndex + 1}");
        }
    }

    private void CreateIcon(RectTransform parent, Sprite icon)
    {
        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.SetParent(parent, false);
        iconRect.anchorMin = new Vector2(0.12f, 0.12f);
        iconRect.anchorMax = new Vector2(0.88f, 0.88f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;

        Image iconImage = iconObject.GetComponent<Image>();
        iconImage.sprite = icon;
        iconImage.preserveAspect = true;
    }

    private void CreateLabel(RectTransform parent, string text)
    {
        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.SetParent(parent, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(6f, 4f);
        labelRect.offsetMax = new Vector2(-6f, -4f);

        Text label = labelObject.GetComponent<Text>();
        label.text = text;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.black;
        label.font = GetBuiltInFont();
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 12;
        label.resizeTextMaxSize = 22;
    }

    private void CreateMessageItem(string message)
    {
        GameObject itemObject = new GameObject("Fixed Camera Message", typeof(RectTransform), typeof(Text));
        RectTransform itemRect = itemObject.GetComponent<RectTransform>();
        itemRect.SetParent(buttonContainer, false);
        itemRect.sizeDelta = new Vector2(buttonSize.x * 2f, buttonSize.y * 0.55f);

        Text label = itemObject.GetComponent<Text>();
        label.text = message;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.font = GetBuiltInFont();
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 12;
        label.resizeTextMaxSize = 22;
    }

    private void SelectPoint(int pointIndex)
    {
        HideSelectionPanel();
        onPointSelected?.Invoke(pointIndex);
    }

    private void RefreshButtonSelection()
    {
        for (int i = 0; i < runtimeButtons.Count; i++)
        {
            Image image = runtimeButtons[i].GetComponent<Image>();
            if (image != null)
            {
                image.color = i == activePointIndex ? selectedButtonColor : buttonColor;
            }
        }
    }

    private void ResizePanel(int itemCount)
    {
        if (buttonContainer == null)
        {
            return;
        }

        float width = itemCount * buttonSize.x + Mathf.Max(0, itemCount - 1) * buttonSpacing + 40f;
        float height = buttonSize.y + 24f;
        buttonContainer.sizeDelta = new Vector2(width, height);
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = EventSystem.current != null ? EventSystem.current : FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            eventSystem = eventSystemObject.GetComponent<EventSystem>();
            DontDestroyOnLoad(eventSystemObject);
        }

#if ENABLE_INPUT_SYSTEM
        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        foreach (StandaloneInputModule legacyModule in eventSystem.GetComponents<StandaloneInputModule>())
        {
            legacyModule.enabled = false;
        }
#else
        if (eventSystem.GetComponent<StandaloneInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
#endif
    }

    private static Font GetBuiltInFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
