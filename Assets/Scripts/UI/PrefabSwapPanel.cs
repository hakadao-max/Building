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

    private readonly List<Button> optionButtons = new List<Button>();

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

        CacheOptionButtons();
        int optionCount = options != null ? options.Count : 0;

        for (int index = 0; index < optionButtons.Count; index++)
        {
            Button button = optionButtons[index];
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

        if (optionCount > optionButtons.Count)
        {
            Debug.LogWarning(
                $"预制体交换面板只有 {optionButtons.Count} 个预制按钮，无法显示全部 {optionCount} 个选项。",
                this);
        }
    }

    public void ClearOptions()
    {
        CacheOptionButtons();
        foreach (Button button in optionButtons)
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

    private void CacheOptionButtons()
    {
        if (optionButtons.Count > 0)
        {
            return;
        }

        if (templateButton != null)
        {
            optionButtons.Add(templateButton);
        }

        GameObject searchRoot = panelRoot != null ? panelRoot : gameObject;
        foreach (Button button in searchRoot.GetComponentsInChildren<Button>(true))
        {
            if (button != closeButton && !optionButtons.Contains(button))
            {
                optionButtons.Add(button);
            }
        }
    }
}
