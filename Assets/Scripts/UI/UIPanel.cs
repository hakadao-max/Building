using UnityEngine;

public abstract class UIPanel : MonoBehaviour
{
    private bool hasAppliedVisibility;

    public abstract string PanelName { get; }

    protected virtual GameObject VisibilityRoot => gameObject;

    public bool IsVisible => VisibilityRoot != null && VisibilityRoot.activeSelf;

    internal void SetVisible(bool visible)
    {
        GameObject target = VisibilityRoot;
        if (target == null)
        {
            return;
        }

        bool changed = target.activeSelf != visible;
        if (changed)
        {
            target.SetActive(visible);
        }

        if (!hasAppliedVisibility || changed)
        {
            hasAppliedVisibility = true;
            OnVisibilityChanged(visible);
        }
    }

    protected virtual void OnVisibilityChanged(bool visible)
    {
    }
}
