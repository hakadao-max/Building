using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public sealed class InteractableArea : MonoBehaviour
{
    private static readonly HashSet<InteractableArea> ActiveAreas = new HashSet<InteractableArea>();

    [Header("交互配置")]
    [LabelText("交互目标")]
    [SerializeField] private GameObject interactionTarget;

    [LabelText("广播给子物体")]
    [SerializeField] private bool broadcastToChildren = true;

    [LabelText("交互方法名")]
    [SerializeField] private string interactionMessage = "ObjectClicked";

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
    public string PromptText => promptText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ClearActiveAreas()
    {
        ActiveAreas.Clear();
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

    public bool TryGetInteractionDistance(Vector3 interactorPosition, out float sqrDistance)
    {
        SphereCollider areaCollider = GetComponent<SphereCollider>();
        if (!isActiveAndEnabled || areaCollider == null || !areaCollider.enabled)
        {
            sqrDistance = float.PositiveInfinity;
            return false;
        }

        Vector3 closestPoint = areaCollider.ClosestPoint(interactorPosition);
        bool isInsideArea = (closestPoint - interactorPosition).sqrMagnitude <= 0.0001f;
        sqrDistance = (transform.position - interactorPosition).sqrMagnitude;
        return isInsideArea;
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
