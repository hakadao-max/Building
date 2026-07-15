using UnityEngine;
using TMPro;
using UnityEngine.UI;

public sealed class WorldDescriptionUI : MonoBehaviour
{
    [LabelText("标题")]
    [SerializeField] private string title = "场景元素";

    [LabelText("说明内容")]
    [TextArea(2, 6)]
    [SerializeField] private string description = "在这里填写场景元素说明。";

    [LabelText("显示偏移")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);

    [LabelText("画布缩放")]
    [SerializeField] private float canvasScale = 0.01f;

    [LabelText("面板尺寸")]
    [SerializeField] private Vector2 panelSize = new Vector2(320f, 160f);

    [LabelText("标题字号")]
    [SerializeField] private int titleFontSize = 28;

    [LabelText("正文字号")]
    [SerializeField] private int descriptionFontSize = 20;

    [LabelText("始终面向相机")]
    [SerializeField] private bool faceCamera = true;

    [LabelText("只绕Y轴旋转")]
    [SerializeField] private bool rotateOnlyYAxis;

    [LabelText("仅详情查看时显示")]
    [SerializeField] private bool onlyShowInDetailInspect = true;

    [LabelText("最大显示距离")]
    [SerializeField] private float maxVisibleDistance = 0f;

    [LabelText("距离检测目标")]
    [SerializeField] private Transform distanceTarget;

    [LabelText("提示目标")]
    [SerializeField] private Transform hintTarget;

    [LabelText("提示颜色")]
    [SerializeField] private Color hintColor = Color.green;

    [LabelText("世界画布")]
    [SerializeField] private Canvas worldCanvas;

    [LabelText("标题文本")]
    [SerializeField] private TMP_Text titleText;

    [LabelText("说明文本")]
    [SerializeField] private TMP_Text descriptionText;

    private Transform cachedPlayerTransform;
    private bool isDetailInspectHighlighted;

    public bool RequiresDetailInspect => onlyShowInDetailInspect;

    private void Awake()
    {
        EnsureCanvas();
        BindTextReferences();
        RefreshText();

        if (onlyShowInDetailInspect && worldCanvas != null)
        {
            worldCanvas.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (worldCanvas == null)
        {
            return;
        }

        Transform canvasTransform = worldCanvas.transform;
        canvasTransform.position = transform.TransformPoint(worldOffset);
        canvasTransform.localScale = Vector3.one * canvasScale;

        Camera targetCamera = Camera.main;
        if (targetCamera == null)
        {
            return;
        }

        if (onlyShowInDetailInspect)
        {
            worldCanvas.gameObject.SetActive(isDetailInspectHighlighted);
        }
        else if (maxVisibleDistance > 0f)
        {
            Transform targetTransform = ResolveDistanceTarget(targetCamera);
            float distance = Vector3.Distance(targetTransform.position, canvasTransform.position);
            worldCanvas.gameObject.SetActive(distance <= maxVisibleDistance);
        }
        else if (!worldCanvas.gameObject.activeSelf)
        {
            worldCanvas.gameObject.SetActive(true);
        }

        if (faceCamera)
        {
            RotateTowardsCamera(canvasTransform, targetCamera.transform);
        }
    }

    private void OnValidate()
    {
        canvasScale = Mathf.Max(0.001f, canvasScale);
        panelSize.x = Mathf.Max(80f, panelSize.x);
        panelSize.y = Mathf.Max(50f, panelSize.y);
        titleFontSize = Mathf.Max(1, titleFontSize);
        descriptionFontSize = Mathf.Max(1, descriptionFontSize);
        maxVisibleDistance = Mathf.Max(0f, maxVisibleDistance);
        BindTextReferences();
        RefreshText();
    }

    public void SetDescription(string newTitle, string newDescription)
    {
        title = newTitle;
        description = newDescription;
        RefreshText();
    }

    public Transform HintTarget => ResolveHintTarget();

    public bool ShouldShowHint => worldCanvas == null || !worldCanvas.gameObject.activeSelf;

    public void ShowHint(float duration)
    {
        TemporaryColorHint.Show(ResolveHintTarget(), duration, hintColor);
    }

    public void SetDetailInspectHighlighted(bool highlighted)
    {
        isDetailInspectHighlighted = highlighted;
        if (worldCanvas != null)
        {
            worldCanvas.gameObject.SetActive(!onlyShowInDetailInspect || highlighted);
        }
    }

    private void EnsureCanvas()
    {
        BindTextReferences();

        if (worldCanvas != null)
        {
            ApplyCanvasLayout();
            return;
        }

        GameObject canvasObject = new GameObject("World Description Canvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvasObject.transform.localPosition = worldOffset;
        canvasObject.transform.localScale = Vector3.one * canvasScale;

        worldCanvas = canvasObject.GetComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.sortingOrder = 50;

        ApplyCanvasLayout();

        GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
        backgroundObject.transform.SetParent(canvasObject.transform, false);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        Image background = backgroundObject.GetComponent<Image>();
        background.color = new Color(0.05f, 0.05f, 0.05f, 0.78f);

        titleText = CreateText(canvasObject.transform, "Title", titleFontSize, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.68f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(18f, 0f);
        titleRect.offsetMax = new Vector2(-18f, -8f);

        descriptionText = CreateText(canvasObject.transform, "Description", descriptionFontSize, FontStyles.Normal, TextAlignmentOptions.Top);
        RectTransform descriptionRect = descriptionText.GetComponent<RectTransform>();
        descriptionRect.anchorMin = new Vector2(0f, 0f);
        descriptionRect.anchorMax = new Vector2(1f, 0.68f);
        descriptionRect.offsetMin = new Vector2(22f, 16f);
        descriptionRect.offsetMax = new Vector2(-22f, -4f);
    }

    private void BindTextReferences()
    {
        if (worldCanvas == null)
        {
            worldCanvas = GetComponentInChildren<Canvas>(true);
        }

        if (worldCanvas == null)
        {
            return;
        }

        if (titleText == null)
        {
            titleText = FindChildText("Title");
        }

        if (descriptionText == null)
        {
            descriptionText = FindChildText("Description");
        }
    }

    private TMP_Text FindChildText(string childName)
    {
        Transform textTransform = worldCanvas.transform.Find(childName);
        if (textTransform != null && textTransform.TryGetComponent(out TMP_Text foundText))
        {
            return foundText;
        }

        foreach (TMP_Text candidate in worldCanvas.GetComponentsInChildren<TMP_Text>(true))
        {
            if (candidate.name == childName)
            {
                return candidate;
            }
        }

        return null;
    }

    private TMP_Text CreateText(Transform parent, string objectName, int fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMP_Text uiText = textObject.GetComponent<TextMeshProUGUI>();
        uiText.fontSize = fontSize;
        uiText.fontStyle = fontStyle;
        uiText.alignment = alignment;
        uiText.color = Color.white;
        uiText.textWrappingMode = TextWrappingModes.Normal;
        uiText.overflowMode = TextOverflowModes.Truncate;
        uiText.raycastTarget = false;

        return uiText;
    }

    private void RefreshText()
    {
        BindTextReferences();

        if (titleText != null)
        {
            titleText.text = title;
            titleText.fontSize = titleFontSize;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            titleText.textWrappingMode = TextWrappingModes.Normal;
            titleText.overflowMode = TextOverflowModes.Truncate;
        }

        if (descriptionText != null)
        {
            descriptionText.text = description;
            descriptionText.fontSize = descriptionFontSize;
            descriptionText.fontStyle = FontStyles.Normal;
            descriptionText.alignment = TextAlignmentOptions.Top;
            descriptionText.color = Color.white;
            descriptionText.textWrappingMode = TextWrappingModes.Normal;
            descriptionText.overflowMode = TextOverflowModes.Truncate;
        }

        if (worldCanvas != null)
        {
            ApplyCanvasLayout();
        }
    }

    private void ApplyCanvasLayout()
    {
        if (worldCanvas == null)
        {
            return;
        }

        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.sortingOrder = 50;

        RectTransform canvasRect = worldCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = panelSize;
    }

    private void RotateTowardsCamera(Transform canvasTransform, Transform cameraTransform)
    {
        Vector3 lookDirection = canvasTransform.position - cameraTransform.position;
        if (rotateOnlyYAxis)
        {
            lookDirection.y = 0f;
        }

        if (lookDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        if (rotateOnlyYAxis)
        {
            Vector3 eulerAngles = targetRotation.eulerAngles;
            canvasTransform.rotation = Quaternion.Euler(0f, eulerAngles.y, 0f);
            return;
        }

        canvasTransform.rotation = targetRotation;
    }

    private Transform ResolveDistanceTarget(Camera targetCamera)
    {
        if (distanceTarget != null)
        {
            return distanceTarget;
        }

        if (cachedPlayerTransform == null)
        {
            SimplePlayerController playerController = GameController.PlayerController;
            if (playerController != null)
            {
                cachedPlayerTransform = playerController.transform;
            }
        }

        return cachedPlayerTransform != null ? cachedPlayerTransform : targetCamera.transform;
    }

    private Transform ResolveHintTarget()
    {
        if (hintTarget != null)
        {
            return hintTarget;
        }

        if (HasRenderableMesh(transform))
        {
            return transform;
        }

        if (transform.parent != null)
        {
            return transform.parent;
        }

        return transform;
    }

    private static bool HasRenderableMesh(Transform target)
    {
        MeshRenderer meshRenderer = target.GetComponent<MeshRenderer>();
        MeshFilter meshFilter = target.GetComponent<MeshFilter>();
        return meshRenderer != null && meshRenderer.enabled && meshFilter != null && meshFilter.sharedMesh != null;
    }
}
