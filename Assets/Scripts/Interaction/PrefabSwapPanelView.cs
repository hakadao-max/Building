using UnityEngine;
using TMPro;
using UnityEngine.UI;

public sealed class PrefabSwapPanelView : MonoBehaviour
{
    [LabelText("交互提示根节点")]
    [SerializeField] private GameObject promptRoot;

    [LabelText("选择面板根节点")]
    [SerializeField] private GameObject panelRoot;

    [LabelText("退出按钮")]
    [SerializeField] private Button closeButton;

    [LabelText("预制体选择按钮")]
    [SerializeField] private Button[] optionButtons;

    [LabelText("选择按钮文字")]
    [SerializeField] private TMP_Text[] optionLabels;

    public GameObject PromptRoot => promptRoot;
    public GameObject PanelRoot => panelRoot;
    public Button CloseButton => closeButton;
    public int OptionSlotCount => optionButtons != null ? optionButtons.Length : 0;

    public Button GetOptionButton(int index)
    {
        return optionButtons != null && index >= 0 && index < optionButtons.Length
            ? optionButtons[index]
            : null;
    }

    public void ConfigureOptionSlot(int index, string displayName, bool available)
    {
        Button button = GetOptionButton(index);
        if (button != null)
        {
            button.gameObject.SetActive(available);
        }

        if (optionLabels != null && index >= 0 && index < optionLabels.Length && optionLabels[index] != null)
        {
            optionLabels[index].text = available ? displayName : string.Empty;
        }
    }

    public void SetPromptVisible(bool visible)
    {
        if (promptRoot != null)
        {
            promptRoot.SetActive(visible);
        }
    }

    public void SetPanelVisible(bool visible)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
        }
    }
}
