using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class FixedCameraPanel : UIPanel
{
    [LabelText("固定视角按钮")]
    [SerializeField] private List<Button> cameraButtons = new List<Button>();

    [LabelText("空状态文字")]
    [SerializeField] private TMP_Text emptyMessage;

    [LabelText("普通按钮颜色")]
    [SerializeField] private Color buttonColor = new Color(1f, 1f, 1f, 0.9f);

    [LabelText("选中按钮颜色")]
    [SerializeField] private Color selectedButtonColor = new Color(0.35f, 0.75f, 1f, 1f);

    public override string PanelName => UIPanelNames.FixedCamera;

    public void Configure(
        IReadOnlyList<PlayerFixedCameraPoint> cameraPoints,
        int activePointIndex,
        Action<int> onSelected)
    {
        EnsureButtonReferences();
        int pointCount = cameraPoints != null ? cameraPoints.Count : 0;

        if (emptyMessage != null)
        {
            emptyMessage.gameObject.SetActive(pointCount == 0);
            emptyMessage.text = pointCount == 0 ? "未配置固定视角" : string.Empty;
        }

        for (int index = 0; index < cameraButtons.Count; index++)
        {
            Button button = cameraButtons[index];
            if (button == null)
            {
                continue;
            }

            button.onClick.RemoveAllListeners();
            bool hasPoint = index < pointCount;
            button.gameObject.SetActive(hasPoint);
            if (!hasPoint)
            {
                continue;
            }

            PlayerFixedCameraPoint point = cameraPoints[index];
            button.interactable = point != null && point.HasViewPoint;
            ApplyButtonContent(button, point, index);

            if (onSelected != null)
            {
                int capturedIndex = index;
                button.onClick.AddListener(() => onSelected(capturedIndex));
            }
        }

        if (pointCount > cameraButtons.Count)
        {
            Debug.LogWarning(
                $"固定视角面板只有 {cameraButtons.Count} 个预制按钮，无法显示全部 {pointCount} 个视角。",
                this);
        }

        RefreshSelection(activePointIndex);
    }

    public void RefreshSelection(int activePointIndex)
    {
        for (int index = 0; index < cameraButtons.Count; index++)
        {
            Button button = cameraButtons[index];
            if (button != null && button.targetGraphic != null)
            {
                button.targetGraphic.color = index == activePointIndex
                    ? selectedButtonColor
                    : buttonColor;
            }
        }
    }

    private void EnsureButtonReferences()
    {
        if (cameraButtons.Count == 0)
        {
            cameraButtons.AddRange(GetComponentsInChildren<Button>(true));
        }
    }

    private static void ApplyButtonContent(Button button, PlayerFixedCameraPoint point, int pointIndex)
    {
        Image iconImage = null;
        foreach (Image image in button.GetComponentsInChildren<Image>(true))
        {
            if (image.gameObject != button.gameObject)
            {
                iconImage = image;
                break;
            }
        }

        Sprite icon = point != null ? point.Icon : null;
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(icon != null);
        }

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.gameObject.SetActive(icon == null);
            label.text = point != null ? point.DisplayName : $"视角 {pointIndex + 1}";
        }
    }
}
