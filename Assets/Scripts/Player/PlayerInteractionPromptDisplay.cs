using UnityEngine;
using TMPro;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PlayerInteractionPromptDisplay : MonoBehaviour
{
    [LabelText("提示文字组件")]
    [SerializeField] private TMP_Text promptText;

    [LabelText("文字颜色")]
    [SerializeField] private Color textColor = Color.white;

    [LabelText("文字大小")]
    [SerializeField] private int fontSize = 28;

    [LabelText("底部偏移")]
    [SerializeField] private float bottomOffset = 120f;

    [LabelText("显示区域大小")]
    [SerializeField] private Vector2 displaySize = new Vector2(720f, 80f);

    [LabelText("文字阴影")]
    [SerializeField] private bool useShadow = true;

    private void Awake()
    {
        Initialize();
        Hide();
    }

    private void OnValidate()
    {
        fontSize = Mathf.Max(1, fontSize);
        bottomOffset = Mathf.Max(0f, bottomOffset);
        displaySize.x = Mathf.Max(1f, displaySize.x);
        displaySize.y = Mathf.Max(1f, displaySize.y);
        ApplyStyle();
    }

    public void Initialize()
    {
        EnsureText();
        ApplyStyle();
    }

    public void Show(string message)
    {
        Initialize();
        promptText.text = message;
        promptText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    private void EnsureText()
    {
        if (promptText != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("Interaction Prompt Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 110;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject textObject = new GameObject("Interaction Prompt Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(canvasObject.transform, false);
        promptText = textObject.GetComponent<TextMeshProUGUI>();
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.textWrappingMode = TextWrappingModes.Normal;
        promptText.overflowMode = TextOverflowModes.Overflow;
        promptText.raycastTarget = false;
    }

    private void ApplyStyle()
    {
        if (promptText == null)
        {
            return;
        }

        promptText.color = textColor;
        promptText.fontSize = fontSize;

        promptText.outlineWidth = useShadow ? 0.16f : 0f;
        promptText.outlineColor = new Color32(0, 0, 0, 210);

        RectTransform rectTransform = promptText.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = new Vector2(0f, bottomOffset);
        rectTransform.sizeDelta = displaySize;
    }
}
