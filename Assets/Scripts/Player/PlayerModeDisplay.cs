using UnityEngine;
using TMPro;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PlayerModeDisplay : MonoBehaviour
{
    [LabelText("模式文字组件")]
    [SerializeField] private TMP_Text modeText;

    [LabelText("第一人称信息")]
    [SerializeField] private string firstPersonText = "模式 1：第一人称";

    [LabelText("第三人称信息")]
    [SerializeField] private string thirdPersonText = "模式 2：第三人称";

    [LabelText("固定路线信息")]
    [SerializeField] private string fixedRouteText = "模式 3：固定路线漫游";

    [LabelText("固定视角信息")]
    [SerializeField] private string fixedCameraText = "模式 4：固定视角";

    [LabelText("小地图信息")]
    [SerializeField] private string minimapText = "模式 5：小地图传送";

    [LabelText("详情查看信息")]
    [SerializeField] private string detailInspectText = "模式 6：详情查看";

    [LabelText("透视拾取信息")]
    [SerializeField] private string perspectivePickupText = "模式 7：透视拾取";

    [LabelText("文字颜色")]
    [SerializeField] private Color textColor = Color.white;

    [LabelText("文字大小")]
    [SerializeField] private int fontSize = 24;

    [LabelText("左上角偏移")]
    [SerializeField] private Vector2 topLeftOffset = new Vector2(24f, -24f);

    [LabelText("显示区域大小")]
    [SerializeField] private Vector2 displaySize = new Vector2(520f, 60f);

    private void Awake()
    {
        Initialize();
    }

    private void OnValidate()
    {
        fontSize = Mathf.Max(1, fontSize);
        displaySize.x = Mathf.Max(1f, displaySize.x);
        displaySize.y = Mathf.Max(1f, displaySize.y);
        ApplyStyle();
    }

    public void Initialize()
    {
        EnsureText();
        ApplyStyle();
    }

    public void Refresh(PlayerViewMode viewMode, bool detailInspectActive)
    {
        Initialize();
        modeText.text = detailInspectActive ? detailInspectText : ResolveText(viewMode);
    }

    private string ResolveText(PlayerViewMode viewMode)
    {
        switch (viewMode)
        {
            case PlayerViewMode.ThirdPerson:
                return thirdPersonText;
            case PlayerViewMode.FixedRouteRoam:
                return fixedRouteText;
            case PlayerViewMode.FixedCamera:
                return fixedCameraText;
            case PlayerViewMode.MinimapTeleport:
                return minimapText;
            case PlayerViewMode.PerspectivePickup:
                return perspectivePickupText;
            default:
                return firstPersonText;
        }
    }

    private void EnsureText()
    {
        if (modeText != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("Player Mode Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject textObject = new GameObject("Current Mode Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(canvasObject.transform, false);
        modeText = textObject.GetComponent<TextMeshProUGUI>();
        modeText.alignment = TextAlignmentOptions.TopLeft;
        modeText.textWrappingMode = TextWrappingModes.Normal;
        modeText.overflowMode = TextOverflowModes.Overflow;
        modeText.raycastTarget = false;
    }

    private void ApplyStyle()
    {
        if (modeText == null)
        {
            return;
        }

        modeText.color = textColor;
        modeText.fontSize = fontSize;

        RectTransform rectTransform = modeText.rectTransform;
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = topLeftOffset;
        rectTransform.sizeDelta = displaySize;
    }
}
