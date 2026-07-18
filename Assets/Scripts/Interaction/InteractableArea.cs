using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public sealed class InteractableArea : MonoBehaviour
{
    [Header("交互配置")]
    [LabelText("交互按键")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [LabelText("提示文本")]
    [SerializeField] private string promptText = "按 E 互动";

    private SimplePlayerController playerInRange;
    private InteractableObj interactableObject;
    private int overlappingPlayerColliders;
    private bool isPlayerInRange;
    private bool isPromptVisible;

    private void Reset()
    {
        EnsureTriggerCollider();
    }

    private void OnEnable()
    {
        ResolveInteractableObject();
    }

    private void OnDisable()
    {
        ClearPlayerState();
    }

    private void Update()
    {
        if (!isPlayerInRange)
        {
            return;
        }

        if (!CanInteract(playerInRange))
        {
            HidePrompt();
            return;
        }

        ShowPrompt();
        if (interactionKey != KeyCode.None && RuntimeInput.GetKeyDown(interactionKey))
        {
            Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        SimplePlayerController player = other.GetComponentInParent<SimplePlayerController>();
        if (player == null || (playerInRange != null && playerInRange != player))
        {
            return;
        }

        playerInRange = player;
        overlappingPlayerColliders++;
        isPlayerInRange = true;

        if (CanInteract(playerInRange))
        {
            ShowPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SimplePlayerController player = other.GetComponentInParent<SimplePlayerController>();
        if (player == null || player != playerInRange)
        {
            return;
        }

        overlappingPlayerColliders = Mathf.Max(0, overlappingPlayerColliders - 1);
        if (overlappingPlayerColliders == 0)
        {
            ClearPlayerState();
        }
    }

    private void OnValidate()
    {
        EnsureTriggerCollider();
    }

    public void Interact()
    {
        ResolveInteractableObject();
        if (interactableObject == null)
        {
            Debug.LogWarning("交互区域所在物体缺少 InteractableObj 交互脚本。", this);
            return;
        }

        interactableObject.ObjectClicked();
    }

    private void ResolveInteractableObject()
    {
        if (interactableObject == null)
        {
            interactableObject = GetComponent<InteractableObj>();
        }
    }

    private static bool CanInteract(SimplePlayerController player)
    {
        return player != null
            && player.isActiveAndEnabled
            && GameController.PlayerControlEnabled
            && player.AllowsTriggerInteraction;
    }

    private void ShowPrompt()
    {
        if (isPromptVisible || string.IsNullOrWhiteSpace(promptText))
        {
            return;
        }

        PlayerInteractionPromptDisplay panel = UIManager.GetPanel<PlayerInteractionPromptDisplay>(
            UIPanelNames.InteractionPrompt);
        if (panel == null)
        {
            return;
        }

        panel.SetMessage(promptText);
        UIManager.ShowPanel(UIPanelNames.InteractionPrompt);
        isPromptVisible = true;
    }

    private void HidePrompt()
    {
        if (!isPromptVisible)
        {
            return;
        }

        UIManager.HidePanel(UIPanelNames.InteractionPrompt);
        isPromptVisible = false;
    }

    private void ClearPlayerState()
    {
        HidePrompt();
        playerInRange = null;
        overlappingPlayerColliders = 0;
        isPlayerInRange = false;
    }

    private void EnsureTriggerCollider()
    {
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            return;
        }

        sphereCollider.isTrigger = true;
        if (Mathf.Approximately(sphereCollider.radius, 0f))
        {
            sphereCollider.radius = 2f;
        }
    }
}
