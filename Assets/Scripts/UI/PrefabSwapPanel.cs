using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class PrefabSwapPanel : UIPanel
{
    public override string PanelName => UIPanelNames.PrefabSwap;

    [LabelText("选择面板根节点")]
    [SerializeField] private GameObject panelRoot;

    [LabelText("退出按钮")]
    [SerializeField] private Button closeButton;

    [LabelText("选项模板按钮")]
    [SerializeField] private Button templateButton;

    private readonly List<Button> generatedButtons = new List<Button>();
    private bool legacyButtonsHidden;

    public GameObject PanelRoot => panelRoot;
    public Button CloseButton => closeButton;

    protected override GameObject VisibilityRoot => panelRoot != null ? panelRoot : gameObject;

    public void SetOptions(IReadOnlyList<PrefabSwapOption> options, UnityAction<int> onSelected)
    {
        EnsureTemplateButton();
        if (templateButton == null)
        {
            Debug.LogWarning("预制体交换面板未绑定选项模板按钮。", this);
            return;
        }

        HideLegacyOptionButtons();
        int optionCount = options != null ? options.Count : 0;
        EnsureGeneratedButtonCount(optionCount);

        for (int index = 0; index < generatedButtons.Count; index++)
        {
            Button button = generatedButtons[index];
            button.onClick.RemoveAllListeners();

            bool hasOption = index < optionCount;
            PrefabSwapOption option = hasOption ? options[index] : null;
            bool available = option != null && option.Prefab != null;
            button.gameObject.SetActive(available);

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = available ? option.DisplayName : string.Empty;
            }

            if (available && onSelected != null)
            {
                int capturedIndex = index;
                button.onClick.AddListener(() => onSelected(capturedIndex));
            }
        }

        templateButton.gameObject.SetActive(false);
    }

    public void ClearOptions()
    {
        foreach (Button button in generatedButtons)
        {
            if (button == null)
            {
                continue;
            }

            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false);
        }
    }

    protected override void OnVisibilityChanged(bool visible)
    {
        GameController.SetPlayerControlEnabled(!visible);
        if (visible)
        {
            GameController.SetCursorLocked(false);
        }
    }

    private void EnsureTemplateButton()
    {
        if (templateButton != null)
        {
            return;
        }

        GameObject searchRoot = panelRoot != null ? panelRoot : gameObject;
        foreach (Button button in searchRoot.GetComponentsInChildren<Button>(true))
        {
            if (button != closeButton)
            {
                templateButton = button;
                return;
            }
        }
    }

    private void HideLegacyOptionButtons()
    {
        if (legacyButtonsHidden || templateButton == null)
        {
            return;
        }

        legacyButtonsHidden = true;
        Transform container = templateButton.transform.parent;
        foreach (Button button in container.GetComponentsInChildren<Button>(true))
        {
            if (button != closeButton)
            {
                button.gameObject.SetActive(false);
            }
        }
    }

    private void EnsureGeneratedButtonCount(int count)
    {
        while (generatedButtons.Count < count)
        {
            Button button = Instantiate(templateButton, templateButton.transform.parent);
            button.name = $"Option {generatedButtons.Count + 1}";
            button.onClick = new Button.ButtonClickedEvent();
            generatedButtons.Add(button);
        }
    }
}
