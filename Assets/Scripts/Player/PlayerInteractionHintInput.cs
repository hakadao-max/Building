using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerInteractionHintInput : MonoBehaviour
{
    [LabelText("显示交互提示按键")]
    [SerializeField] private KeyCode revealKey = KeyCode.R;

    public KeyCode RevealKey => revealKey;

    private SimplePlayerController controller;
    private PlayerRangeColorScanner colorScanner;

    private void Update()
    {
        if (controller != null
            && GameController.PlayerControlEnabled
            && controller.AllowsManualAbilities)
        {
            TickInput();
        }
    }

    public void Bind(SimplePlayerController owner, PlayerRangeColorScanner scanner)
    {
        controller = owner;
        colorScanner = scanner;
    }

    public void TickInput()
    {
        if (revealKey == KeyCode.None || !RuntimeInput.GetKeyDown(revealKey))
        {
            return;
        }

        colorScanner?.ScanAndShowColorHints();
    }
}
