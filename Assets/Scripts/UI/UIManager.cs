using System;
using System.Collections.Generic;
using UnityEngine;

public static class UIManager
{
    public const string RootName = "UIRoot";
    public const string CanvasRootName = "Canvas";
    public const string WorldCanvasRootName = "WorldCanvas";

    private static readonly Dictionary<string, UIPanel> Panels =
        new Dictionary<string, UIPanel>(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, Transform> WorldUITemplates =
        new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<GameObject> WorldUIInstances = new HashSet<GameObject>();
    private static readonly HashSet<string> MissingWorldUITemplates =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    private static Transform uiRoot;
    private static Transform canvasRoot;
    private static Transform worldCanvasRoot;

    public static Transform UIRoot
    {
        get
        {
            if (uiRoot == null)
            {
                uiRoot = FindUIRoot();
            }

            return uiRoot;
        }
    }

    public static Transform CanvasRoot
    {
        get
        {
            if (canvasRoot == null)
            {
                Transform root = UIRoot;
                canvasRoot = root != null ? FindDirectChild(root, CanvasRootName) : null;
            }

            return canvasRoot;
        }
    }

    public static Transform WorldCanvasRoot
    {
        get
        {
            if (worldCanvasRoot == null)
            {
                Transform root = UIRoot;
                worldCanvasRoot = root != null ? FindDirectChild(root, WorldCanvasRootName) : null;
            }

            return worldCanvasRoot;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeState()
    {
        uiRoot = null;
        canvasRoot = null;
        worldCanvasRoot = null;
        Panels.Clear();
        WorldUITemplates.Clear();
        WorldUIInstances.Clear();
        MissingWorldUITemplates.Clear();
    }

    public static GameObject AddWorldUI(string templateName)
    {
        Transform template = GetWorldUITemplate(templateName);
        Transform root = WorldCanvasRoot;
        if (template == null || root == null)
        {
            if (MissingWorldUITemplates.Add(templateName))
            {
                Debug.LogWarning(
                    $"UIManager did not find world UI template '{templateName}' below "
                    + $"'{RootName}/{WorldCanvasRootName}'.");
            }

            return null;
        }

        template.gameObject.SetActive(false);
        GameObject instance = UnityEngine.Object.Instantiate(template.gameObject, root, false);
        instance.name = $"{templateName} Instance";
        WorldUIInstances.Add(instance);
        instance.SetActive(true);
        return instance;
    }

    public static void RemoveWorldUI(GameObject worldUI)
    {
        if (worldUI == null || !WorldUIInstances.Remove(worldUI))
        {
            return;
        }

        UnityEngine.Object.Destroy(worldUI);
    }

    public static bool ShowPanel(string panelName)
    {
        return SetPanelVisible(panelName, true);
    }

    public static bool HidePanel(string panelName)
    {
        return SetPanelVisible(panelName, false);
    }

    public static bool TogglePanel(string panelName)
    {
        UIPanel panel = GetPanel(panelName);
        return panel != null && SetPanelVisible(panelName, !panel.IsVisible);
    }

    public static bool SetPanelVisible(string panelName, bool visible)
    {
        UIPanel panel = GetPanel(panelName);
        if (panel == null)
        {
            Debug.LogWarning(
                $"UIManager did not find panel '{panelName}' below '{RootName}/{CanvasRootName}'.");
            return false;
        }

        panel.SetVisible(visible);
        return true;
    }

    public static bool IsPanelVisible(string panelName)
    {
        UIPanel panel = GetPanel(panelName);
        return panel != null && panel.IsVisible;
    }

    public static UIPanel GetPanel(string panelName)
    {
        if (string.IsNullOrWhiteSpace(panelName))
        {
            return null;
        }

        if (Panels.TryGetValue(panelName, out UIPanel cachedPanel) && cachedPanel != null)
        {
            return cachedPanel;
        }

        RebuildPanelCache();
        Panels.TryGetValue(panelName, out UIPanel panel);
        return panel;
    }

    public static T GetPanel<T>(string panelName) where T : UIPanel
    {
        return GetPanel(panelName) as T;
    }

    public static void RebuildPanelCache()
    {
        Panels.Clear();
        Transform root = CanvasRoot;
        if (root == null)
        {
            return;
        }

        foreach (UIPanel panel in root.GetComponentsInChildren<UIPanel>(true))
        {
            RegisterAlias(panel.PanelName, panel);
            RegisterAlias(panel.GetType().Name, panel);
            RegisterAlias(panel.gameObject.name, panel);
        }
    }

    private static Transform GetWorldUITemplate(string templateName)
    {
        if (string.IsNullOrWhiteSpace(templateName))
        {
            return null;
        }

        if (WorldUITemplates.TryGetValue(templateName, out Transform cachedTemplate)
            && cachedTemplate != null)
        {
            return cachedTemplate;
        }

        Transform root = WorldCanvasRoot;
        Transform template = root != null ? FindDirectChild(root, templateName) : null;
        if (template != null)
        {
            WorldUITemplates[templateName] = template;
        }

        return template;
    }

    private static void RegisterAlias(string alias, UIPanel panel)
    {
        if (!string.IsNullOrWhiteSpace(alias) && panel != null && !Panels.ContainsKey(alias))
        {
            Panels.Add(alias, panel);
        }
    }

    private static Transform FindUIRoot()
    {
        foreach (Transform candidate in UnityEngine.Object.FindObjectsByType<Transform>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            if (candidate.parent != null || !candidate.gameObject.scene.IsValid())
            {
                continue;
            }

            if (string.Equals(candidate.name, RootName, StringComparison.Ordinal))
            {
                return candidate;
            }
        }

        return null;
    }

    private static Transform FindDirectChild(Transform parent, string childName)
    {
        for (int childIndex = 0; childIndex < parent.childCount; childIndex++)
        {
            Transform child = parent.GetChild(childIndex);
            if (string.Equals(child.name, childName, StringComparison.Ordinal))
            {
                return child;
            }
        }

        return null;
    }
}
