using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public sealed class InteractableArea : MonoBehaviour
{
    private static readonly HashSet<InteractableArea> ActiveAreas = new HashSet<InteractableArea>();
    private static readonly Dictionary<SimplePlayerController, InteractableArea> FocusedAreas =
        new Dictionary<SimplePlayerController, InteractableArea>();
    private static InteractableArea promptOwner;

    private readonly Dictionary<SimplePlayerController, int> overlappingPlayers =
        new Dictionary<SimplePlayerController, int>();

    [Header("交互配置")]
    [LabelText("交互目标")]
    [SerializeField] private GameObject interactionTarget;

    [LabelText("广播给子物体")]
    [SerializeField] private bool broadcastToChildren = true;

    [LabelText("交互方法名")]
    [SerializeField] private string interactionMessage = "ObjectClicked";

    [LabelText("交互按键")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [LabelText("提示文本")]
    [SerializeField] private string promptText = "按 E 互动";

    [Header("提示配置")]
    [LabelText("提示目标")]
    [SerializeField] private Transform hintTarget;

    [LabelText("提示颜色")]
    [SerializeField] private Color hintColor = Color.green;

    [LabelText("提示显示距离")]
    [SerializeField] private float hintRevealDistance = 8f;

    [LabelText("提示持续时间")]
    [SerializeField] private float hintDuration = 3f;

    [Header("自动配置")]
    [LabelText("自动使用父物体")]
    [SerializeField] private bool useParentWhenTargetEmpty = true;

    public static IEnumerable<InteractableArea> ActiveInstances => ActiveAreas;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ClearActiveAreas()
    {
        ActiveAreas.Clear();
        FocusedAreas.Clear();
        promptOwner = null;
    }

    private void Reset()
    {
        EnsureTriggerCollider();

        if (transform.parent != null)
        {
            interactionTarget = transform.parent.gameObject;
        }
    }

    private void OnEnable()
    {
        ActiveAreas.Add(this);
    }

    private void OnDisable()
    {
        ActiveAreas.Remove(this);

        foreach (SimplePlayerController player in new List<SimplePlayerController>(overlappingPlayers.Keys))
        {
            if (FocusedAreas.TryGetValue(player, out InteractableArea focusedArea) && focusedArea == this)
            {
                SetFocusedArea(player, FindNearestOverlappingArea(player));
            }
        }

        overlappingPlayers.Clear();
        HidePromptIfOwned();
    }

    private void Update()
    {
        foreach (SimplePlayerController player in overlappingPlayers.Keys)
        {
            if (!FocusedAreas.TryGetValue(player, out InteractableArea focusedArea) || focusedArea != this)
            {
                continue;
            }

            if (!CanInteract(player))
            {
                HidePromptIfOwned();
                continue;
            }

            ShowPrompt();
            if (interactionKey != KeyCode.None && RuntimeInput.GetKeyDown(interactionKey))
            {
                Interact(player.gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        SimplePlayerController player = other.GetComponentInParent<SimplePlayerController>();
        if (player == null)
        {
            return;
        }

        overlappingPlayers.TryGetValue(player, out int overlapCount);
        overlappingPlayers[player] = overlapCount + 1;
        SetFocusedArea(player, this);
    }

    private void OnTriggerExit(Collider other)
    {
        SimplePlayerController player = other.GetComponentInParent<SimplePlayerController>();
        if (player == null || !overlappingPlayers.TryGetValue(player, out int overlapCount))
        {
            return;
        }

        if (overlapCount > 1)
        {
            overlappingPlayers[player] = overlapCount - 1;
            return;
        }

        overlappingPlayers.Remove(player);
        if (FocusedAreas.TryGetValue(player, out InteractableArea focusedArea) && focusedArea == this)
        {
            SetFocusedArea(player, FindNearestOverlappingArea(player));
        }
    }

    private void OnValidate()
    {
        hintRevealDistance = Mathf.Max(0f, hintRevealDistance);
        hintDuration = Mathf.Max(0.1f, hintDuration);
        EnsureTriggerCollider();
    }

    public void Interact(GameObject interactor)
    {
        GameObject target = ResolveTarget();
        if (target == null || string.IsNullOrWhiteSpace(interactionMessage))
        {
            return;
        }

        SendMessageOptions options = SendMessageOptions.DontRequireReceiver;
        if (broadcastToChildren)
        {
            target.BroadcastMessage(interactionMessage, options);
        }
        else
        {
            target.SendMessage(interactionMessage, options);
        }
    }

    public void TryShowHint(Vector3 observerPosition)
    {
        Transform target = ResolveHintTarget();
        float sqrRevealDistance = hintRevealDistance * hintRevealDistance;
        if ((target.position - observerPosition).sqrMagnitude <= sqrRevealDistance)
        {
            TemporaryColorHint.Show(target, hintDuration, hintColor);
        }
    }

    private GameObject ResolveTarget()
    {
        if (interactionTarget != null)
        {
            return interactionTarget;
        }

        if (useParentWhenTargetEmpty && transform.parent != null)
        {
            return transform.parent.gameObject;
        }

        return gameObject;
    }

    private Transform ResolveHintTarget()
    {
        return hintTarget != null ? hintTarget : transform;
    }

    private bool ContainsPlayer(SimplePlayerController player)
    {
        return player != null && overlappingPlayers.ContainsKey(player);
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
        if (string.IsNullOrWhiteSpace(promptText))
        {
            HidePromptIfOwned();
            return;
        }

        PlayerInteractionPromptDisplay panel = UIManager.GetPanel<PlayerInteractionPromptDisplay>(
            UIPanelNames.InteractionPrompt);
        if (panel == null)
        {
            return;
        }

        promptOwner = this;
        panel.SetMessage(promptText);
        UIManager.ShowPanel(UIPanelNames.InteractionPrompt);
    }

    private void HidePromptIfOwned()
    {
        if (promptOwner != this)
        {
            return;
        }

        UIManager.HidePanel(UIPanelNames.InteractionPrompt);
        promptOwner = null;
    }

    private static void SetFocusedArea(SimplePlayerController player, InteractableArea area)
    {
        if (player == null)
        {
            return;
        }

        if (FocusedAreas.TryGetValue(player, out InteractableArea previousArea) && previousArea != area)
        {
            previousArea.HidePromptIfOwned();
        }

        if (area == null)
        {
            FocusedAreas.Remove(player);
            return;
        }

        FocusedAreas[player] = area;
        if (CanInteract(player))
        {
            area.ShowPrompt();
        }
    }

    private static InteractableArea FindNearestOverlappingArea(SimplePlayerController player)
    {
        InteractableArea nearestArea = null;
        float nearestSqrDistance = float.PositiveInfinity;

        foreach (InteractableArea area in ActiveAreas)
        {
            if (area == null || !area.ContainsPlayer(player))
            {
                continue;
            }

            float sqrDistance = (area.transform.position - player.transform.position).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearestArea = area;
            }
        }

        return nearestArea;
    }

    private void EnsureTriggerCollider()
    {
        Collider areaCollider = GetComponent<SphereCollider>();
        if (areaCollider == null)
        {
            return;
        }

        areaCollider.isTrigger = true;

        if (areaCollider is SphereCollider sphereCollider && Mathf.Approximately(sphereCollider.radius, 0f))
        {
            sphereCollider.radius = 2f;
        }
    }

}
