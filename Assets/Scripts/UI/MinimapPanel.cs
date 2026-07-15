using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class MinimapPanel : UIPanel
{
    [LabelText("小地图图片")]
    [SerializeField] private RawImage minimapImage;

    public override string PanelName => UIPanelNames.Minimap;

    public void SetTexture(Texture texture)
    {
        EnsureImage();
        if (minimapImage != null)
        {
            minimapImage.texture = texture;
            minimapImage.color = Color.white;
        }
    }

    public bool TryGetPointerNormalizedPosition(Vector2 pointerPosition, out Vector2 normalizedPosition)
    {
        normalizedPosition = Vector2.zero;
        EnsureImage();
        if (minimapImage == null)
        {
            return false;
        }

        RectTransform rectTransform = minimapImage.rectTransform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                pointerPosition,
                null,
                out Vector2 localPoint))
        {
            return false;
        }

        Rect rect = rectTransform.rect;
        normalizedPosition.x = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        normalizedPosition.y = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);
        return normalizedPosition.x >= 0f && normalizedPosition.x <= 1f
            && normalizedPosition.y >= 0f && normalizedPosition.y <= 1f;
    }

    private void EnsureImage()
    {
        if (minimapImage == null)
        {
            minimapImage = GetComponentInChildren<RawImage>(true);
        }
    }
}
