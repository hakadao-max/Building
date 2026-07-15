using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerInteractionPromptDisplay : UIPanel
{
    [LabelText("提示文字组件")]
    [SerializeField] private TMP_Text promptText;

    public override string PanelName => UIPanelNames.InteractionPrompt;

    public void SetMessage(string message)
    {
        if (promptText == null)
        {
            promptText = GetComponentInChildren<TMP_Text>(true);
        }

        if (promptText != null)
        {
            promptText.text = message;
        }
    }
}
