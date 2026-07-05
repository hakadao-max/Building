using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class PrefabSwapPanelPrefabCreator
{
    private const string PrefabPath = "Assets/Prefab/UI/Prefab Swap Panel.prefab";
    private const int OptionSlotCount = 6;

    [MenuItem("工具/创建预制体交换面板 Prefab")]
    [MenuItem("测试/创建预制体交换面板 Prefab")]
    private static void CreatePanelPrefab()
    {
        EnsureFolder("Assets/Prefab");
        EnsureFolder("Assets/Prefab/UI");

        GameObject root = CreateUiObject("Prefab Swap Canvas", null);
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        GameObject promptRoot = CreateUiObject("Interaction Prompt", root.transform);
        RectTransform promptRect = promptRoot.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0f);
        promptRect.anchorMax = new Vector2(0.5f, 0f);
        promptRect.pivot = new Vector2(0.5f, 0f);
        promptRect.anchoredPosition = new Vector2(0f, 110f);
        promptRect.sizeDelta = new Vector2(360f, 58f);
        Image promptBackground = promptRoot.AddComponent<Image>();
        promptBackground.color = new Color(0f, 0f, 0f, 0.78f);
        CreateText("Prompt Text", promptRoot.transform, "按 E 交互", 28, TextAnchor.MiddleCenter);

        GameObject panelRoot = CreateUiObject("Swap Panel", root.transform);
        Stretch(panelRoot.GetComponent<RectTransform>());

        GameObject content = CreateUiObject("Content", panelRoot.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(1600f, 72f);
        HorizontalLayoutGroup layout = content.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        List<Button> optionButtons = new List<Button>();
        List<Text> optionLabels = new List<Text>();
        for (int i = 0; i < OptionSlotCount; i++)
        {
            Button button = CreateButton($"预制体选项 {i + 1}", content.transform, $"预制体 {i + 1}", out Text label);
            optionButtons.Add(button);
            optionLabels.Add(label);
        }

        Button closeButton = CreateButton("退出按钮", content.transform, "退出", out _);
        ColorBlock closeColors = closeButton.colors;
        closeColors.normalColor = new Color(0.55f, 0.18f, 0.18f, 1f);
        closeButton.colors = closeColors;

        PrefabSwapPanelView view = root.AddComponent<PrefabSwapPanelView>();
        SerializedObject serializedView = new SerializedObject(view);
        serializedView.FindProperty("promptRoot").objectReferenceValue = promptRoot;
        serializedView.FindProperty("panelRoot").objectReferenceValue = panelRoot;
        serializedView.FindProperty("closeButton").objectReferenceValue = closeButton;
        AssignArray(serializedView.FindProperty("optionButtons"), optionButtons);
        AssignArray(serializedView.FindProperty("optionLabels"), optionLabels);
        serializedView.ApplyModifiedPropertiesWithoutUndo();

        promptRoot.SetActive(false);
        panelRoot.SetActive(false);
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Debug.Log($"已创建固定 UI Prefab：{PrefabPath}");
    }

    private static Button CreateButton(string objectName, Transform parent, string labelText, out Text label)
    {
        GameObject buttonObject = CreateUiObject(objectName, parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.22f, 0.43f, 0.68f, 1f);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 190f;
        layoutElement.preferredHeight = 64f;
        label = CreateText("文字", buttonObject.transform, labelText, 26, TextAnchor.MiddleCenter);
        return button;
    }

    private static Text CreateText(string objectName, Transform parent, string value, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = CreateUiObject(objectName, parent);
        Stretch(textObject.GetComponent<RectTransform>());
        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
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

    private static void AssignArray<T>(SerializedProperty property, List<T> values) where T : Object
    {
        property.arraySize = values.Count;
        for (int i = 0; i < values.Count; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string folderName = Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, folderName);
    }
}
