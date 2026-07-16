using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerInteractionHintInput : MonoBehaviour
{
    [LabelText("显示交互提示按键")]
    [SerializeField] private KeyCode revealKey = KeyCode.R;

    public KeyCode RevealKey => revealKey;

    private SimplePlayerController controller;

    private void Update()
    {
        if (controller != null
            && GameController.PlayerControlEnabled
            && controller.AllowsManualAbilities)
        {
            TickInput();
        }
    }

    public void Bind(SimplePlayerController owner)
    {
        controller = owner;
    }

    public void TickInput()
    {
        if (revealKey == KeyCode.None || !RuntimeInput.GetKeyDown(revealKey))
        {
            return;
        }

        foreach (InteractableArea area in InteractableArea.ActiveInstances)
        {
            if (area != null)
            {
                area.TryShowHint(transform.position);
            }
        }
    }
}
