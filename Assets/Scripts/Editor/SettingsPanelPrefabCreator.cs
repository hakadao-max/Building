using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class SettingsPanelPrefabCreator
{
    private const string PrefabPath = "Assets/Prefab/UI/Settings Panel.prefab";

    [MenuItem("工具/创建设置面板 Prefab")]
    [MenuItem("测试/创建设置面板 Prefab")]
    private static void CreateSettingsPanelPrefab()
    {
        EnsureFolder("Assets/Prefab");
        EnsureFolder("Assets/Prefab/UI");

        GameObject root = CreateUiObject("Settings Canvas", null);
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();
        root.AddComponent<EventSystem>();
        root.AddComponent<StandaloneInputModule>();

        GameObject panelRoot = CreateUiObject("Settings Panel", root.transform);
        Stretch(panelRoot.GetComponent<RectTransform>());
        Image backdrop = panelRoot.AddComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.68f);

        GameObject content = CreateUiObject("Content", panelRoot.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(520f, 280f);
        Image contentBackground = content.AddComponent<Image>();
        contentBackground.color = new Color(0.12f, 0.14f, 0.18f, 0.98f);

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(50, 50, 42, 42);
        layout.spacing = 28f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Button returnButton = CreateButton("Return Spawn Button", content.transform, "回到出生点", new Color(0.22f, 0.43f, 0.68f, 1f));
        Button closeButton = CreateButton("Close Button", content.transform, "退出", new Color(0.55f, 0.18f, 0.18f, 1f));

        SettingsPanelController controller = root.AddComponent<SettingsPanelController>();
        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("panelRoot").objectReferenceValue = panelRoot;
        serializedController.FindProperty("returnSpawnButton").objectReferenceValue = returnButton;
        serializedController.FindProperty("closeButton").objectReferenceValue = closeButton;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        panelRoot.SetActive(false);
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Debug.Log($"已创建固定设置面板 Prefab：{PrefabPath}");
    }

    private static Button CreateButton(string objectName, Transform parent, string labelText, Color color)
    {
        GameObject buttonObject = CreateUiObject(objectName, parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = color;
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        buttonObject.AddComponent<LayoutElement>().preferredHeight = 72f;

        GameObject textObject = CreateUiObject("Text", buttonObject.transform);
        Stretch(textObject.GetComponent<RectTransform>());
        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = labelText;
        text.fontSize = 28;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
        return button;
    }

    private static GameObject CreateUiObject(string objectName, Transform parent)
    {
        GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
        if (parent != null)
        {
            gameObject.transform.SetParent(parent, false);
        }

        return gameObject;
    }

    private static void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
