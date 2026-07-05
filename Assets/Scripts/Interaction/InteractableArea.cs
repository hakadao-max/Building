using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public sealed class InteractableArea : MonoBehaviour
{
    [LabelText("交互目标")]
    [SerializeField] private GameObject interactionTarget;

    [LabelText("广播给子物体")]
    [SerializeField] private bool broadcastToChildren = true;

    [LabelText("交互方法名")]
    [SerializeField] private string interactionMessage = "ObjectClicked";

    [LabelText("提示文本")]
    [SerializeField] private string promptText = "按 E 互动";

    [LabelText("提示目标")]
    [SerializeField] private Transform hintTarget;

    [LabelText("提示颜色")]
    [SerializeField] private Color hintColor = Color.green;

    [LabelText("自动使用父物体")]
    [SerializeField] private bool useParentWhenTargetEmpty = true;

    public string PromptText => promptText;

    private void Reset()
    {
        EnsureTriggerCollider();

        if (transform.parent != null)
        {
            interactionTarget = transform.parent.gameObject;
        }
    }

    private void OnValidate()
    {
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

    public void ShowHint(float duration)
    {
        TemporaryColorHint.Show(ResolveHintTarget(), duration, hintColor);
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

    private void EnsureTriggerCollider()
    {
        Collider areaCollider = GetComponent<Collider>();
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
