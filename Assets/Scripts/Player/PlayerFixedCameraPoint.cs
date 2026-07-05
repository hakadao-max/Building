using System;
using UnityEngine;

[Serializable]
public sealed class PlayerFixedCameraPoint
{
    [LabelText("显示名称")]
    [SerializeField] private string displayName = "固定视角";

    [LabelText("固定视角点")]
    [SerializeField] private Transform viewPoint;

    [LabelText("相机图标")]
    [SerializeField] private Sprite icon;

    public string DisplayName => string.IsNullOrEmpty(displayName) ? "固定视角" : displayName;
    public Transform ViewPoint => viewPoint;
    public Sprite Icon => icon;
    public bool HasViewPoint => viewPoint != null;

    public bool TryGetPose(out Vector3 position, out Quaternion rotation)
    {
        if (viewPoint == null)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }

        position = viewPoint.position;
        rotation = viewPoint.rotation;
        return true;
    }
}
