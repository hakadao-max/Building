using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerModeDisplay : UIPanel
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

    public override string PanelName => UIPanelNames.PlayerMode;

    public void Refresh(PlayerViewMode viewMode, bool detailInspectActive)
    {
        if (modeText == null)
        {
            modeText = GetComponentInChildren<TMP_Text>(true);
        }

        if (modeText != null)
        {
            modeText.text = detailInspectActive ? detailInspectText : ResolveText(viewMode);
        }
    }

    private string ResolveText(PlayerViewMode viewMode)
    {
        return viewMode switch
        {
            PlayerViewMode.ThirdPerson => thirdPersonText,
            PlayerViewMode.FixedRouteRoam => fixedRouteText,
            PlayerViewMode.FixedCamera => fixedCameraText,
            PlayerViewMode.MinimapTeleport => minimapText,
            PlayerViewMode.PerspectivePickup => perspectivePickupText,
            _ => firstPersonText
        };
    }
}
