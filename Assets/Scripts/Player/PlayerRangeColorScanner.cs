using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerRangeColorScanner : MonoBehaviour
{
    private const int MaxScanResults = 128;

    [LabelText("检测半径")]
    [SerializeField] private float scanRadius = 8f;

    [LabelText("检测层级")]
    [SerializeField] private LayerMask targetLayers = ~0;

    [LabelText("提示颜色")]
    [SerializeField] private Color hintColor = Color.green;

    [LabelText("提示持续时间")]
    [SerializeField] private float hintDuration = 3f;

    private readonly Collider[] scanResults = new Collider[MaxScanResults];
    private readonly HashSet<InteractableArea> scannedAreas = new HashSet<InteractableArea>();

    private void OnValidate()
    {
        scanRadius = Mathf.Max(0f, scanRadius);
        hintDuration = Mathf.Max(0.1f, hintDuration);
    }

    public void ScanAndShowColorHints()
    {
        scannedAreas.Clear();
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            scanRadius,
            scanResults,
            targetLayers,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = scanResults[i];
            InteractableArea area = hit != null ? hit.GetComponentInParent<InteractableArea>() : null;
            if (area == null || !scannedAreas.Add(area))
            {
                continue;
            }

            TemporaryColorHint.Show(area.transform, hintDuration, hintColor);
        }

        for (int i = 0; i < hitCount; i++)
        {
            scanResults[i] = null;
        }
    }
}
