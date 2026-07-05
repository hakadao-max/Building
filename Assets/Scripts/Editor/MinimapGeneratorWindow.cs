using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class MinimapGeneratorWindow : EditorWindow
{
    private const string DefaultOutputDirectory = "Assets/Generated/Minimap";
    private const string DefaultOutputFileName = "Minimap.png";

    private PlayerMinimapTeleportView targetView;
    private Vector3 areaCenter = Vector3.zero;
    private Vector2 areaSize = new Vector2(50f, 50f);
    private float selectionPlaneY;
    private float captureHeight = 80f;
    private int textureWidth = 1024;
    private int textureHeight = 1024;
    private LayerMask captureLayerMask = ~0;
    private Color backgroundColor = Color.gray;
    private string outputDirectory = DefaultOutputDirectory;
    private string outputFileName = DefaultOutputFileName;
    private string lastResult = "等待生成。";
    private bool isSelectingInScene;
    private bool isDraggingSelection;
    private Vector3 dragStartWorld;

    [MenuItem("工具/小地图生成工具")]
    private static void Open()
    {
        GetWindow<MinimapGeneratorWindow>("小地图生成工具");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("目标组件", EditorStyles.boldLabel);
        targetView = (PlayerMinimapTeleportView)EditorGUILayout.ObjectField("小地图传送组件", targetView, typeof(PlayerMinimapTeleportView), true);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("框选区域", EditorStyles.boldLabel);
        areaCenter = EditorGUILayout.Vector3Field("区域中心", areaCenter);
        areaSize = EditorGUILayout.Vector2Field("区域尺寸 XZ", areaSize);
        selectionPlaneY = EditorGUILayout.FloatField("框选平面高度", selectionPlaneY);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button(isSelectingInScene ? "停止 Scene 框选" : "开启 Scene 框选", GUILayout.Height(28f)))
            {
                isSelectingInScene = !isSelectingInScene;
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("用目标组件区域", GUILayout.Height(28f)))
            {
                PullAreaFromTargetView();
            }
        }

        EditorGUILayout.HelpBox("开启 Scene 框选后，在 Scene 视图按住鼠标左键拖拽矩形区域。生成的小地图按区域 XZ 范围映射，点击图片的位置会对应传送到同一世界坐标。", MessageType.Info);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("截图参数", EditorStyles.boldLabel);
        captureHeight = Mathf.Max(1f, EditorGUILayout.FloatField("截图高度", captureHeight));
        textureWidth = Mathf.Clamp(EditorGUILayout.IntField("图片宽度", textureWidth), 64, 8192);
        textureHeight = Mathf.Clamp(EditorGUILayout.IntField("图片高度", textureHeight), 64, 8192);
        captureLayerMask.value = EditorGUILayout.MaskField("截图层", captureLayerMask.value, UnityEditorInternal.InternalEditorUtility.layers);
        backgroundColor = EditorGUILayout.ColorField("背景颜色", backgroundColor);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("输出", EditorStyles.boldLabel);
        outputDirectory = EditorGUILayout.TextField("输出目录", outputDirectory);
        outputFileName = EditorGUILayout.TextField("文件名", outputFileName);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("生成小地图图片", GUILayout.Height(32f)))
            {
                GenerateMinimap(false);
            }

            using (new EditorGUI.DisabledScope(targetView == null))
            {
                if (GUILayout.Button("生成并应用到组件", GUILayout.Height(32f)))
                {
                    GenerateMinimap(true);
                }
            }
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("执行结果", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(lastResult, MessageType.None);
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        DrawAreaPreview();

        if (!isSelectingInScene)
        {
            return;
        }

        Event currentEvent = Event.current;
        if (currentEvent == null)
        {
            return;
        }

        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        HandleUtility.AddDefaultControl(controlId);

        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && TryGetMouseWorldPoint(currentEvent.mousePosition, out dragStartWorld))
        {
            isDraggingSelection = true;
            GUIUtility.hotControl = controlId;
            SetAreaFromDrag(dragStartWorld, dragStartWorld);
            currentEvent.Use();
        }
        else if (currentEvent.type == EventType.MouseDrag && isDraggingSelection && TryGetMouseWorldPoint(currentEvent.mousePosition, out Vector3 dragCurrentWorld))
        {
            SetAreaFromDrag(dragStartWorld, dragCurrentWorld);
            currentEvent.Use();
            Repaint();
            sceneView.Repaint();
        }
        else if (currentEvent.type == EventType.MouseUp && isDraggingSelection)
        {
            isDraggingSelection = false;
            GUIUtility.hotControl = 0;
            currentEvent.Use();
            Repaint();
        }
    }

    private void DrawAreaPreview()
    {
        Vector3 halfSize = new Vector3(areaSize.x * 0.5f, 0f, areaSize.y * 0.5f);
        Vector3 p0 = areaCenter + new Vector3(-halfSize.x, 0f, -halfSize.z);
        Vector3 p1 = areaCenter + new Vector3(-halfSize.x, 0f, halfSize.z);
        Vector3 p2 = areaCenter + new Vector3(halfSize.x, 0f, halfSize.z);
        Vector3 p3 = areaCenter + new Vector3(halfSize.x, 0f, -halfSize.z);

        Handles.color = Color.cyan;
        Handles.DrawAAPolyLine(4f, p0, p1, p2, p3, p0);
        Handles.Label(areaCenter, $"小地图区域 {areaSize.x:0.##} x {areaSize.y:0.##}");
    }

    private bool TryGetMouseWorldPoint(Vector2 mousePosition, out Vector3 worldPoint)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0f, selectionPlaneY, 0f));
        if (plane.Raycast(ray, out float enter))
        {
            worldPoint = ray.GetPoint(enter);
            return true;
        }

        worldPoint = Vector3.zero;
        return false;
    }

    private void SetAreaFromDrag(Vector3 start, Vector3 end)
    {
        Vector3 min = Vector3.Min(start, end);
        Vector3 max = Vector3.Max(start, end);
        areaCenter = new Vector3((min.x + max.x) * 0.5f, selectionPlaneY, (min.z + max.z) * 0.5f);
        areaSize = new Vector2(Mathf.Max(0.1f, max.x - min.x), Mathf.Max(0.1f, max.z - min.z));
    }

    private void GenerateMinimap(bool applyToTarget)
    {
        areaSize.x = Mathf.Max(0.1f, areaSize.x);
        areaSize.y = Mathf.Max(0.1f, areaSize.y);

        string safeOutputDirectory = string.IsNullOrWhiteSpace(outputDirectory) ? DefaultOutputDirectory : outputDirectory.Trim();
        string safeFileName = string.IsNullOrWhiteSpace(outputFileName) ? DefaultOutputFileName : outputFileName.Trim();
        if (!safeFileName.EndsWith(".png"))
        {
            safeFileName += ".png";
        }

        string assetPath = $"{safeOutputDirectory.TrimEnd('/', '\\')}/{safeFileName}";
        string absolutePath = Path.GetFullPath(assetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));

        Camera captureCamera = CreateCaptureCamera();
        RenderTexture renderTexture = new RenderTexture(textureWidth, textureHeight, 24, RenderTextureFormat.ARGB32);
        Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);

        RenderTexture previousActive = RenderTexture.active;
        RenderTexture previousTarget = captureCamera.targetTexture;

        try
        {
            captureCamera.targetTexture = renderTexture;
            captureCamera.Render();
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0f, 0f, textureWidth, textureHeight), 0, 0);
            texture.Apply();
            File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
        }
        finally
        {
            captureCamera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            DestroyImmediate(renderTexture);
            DestroyImmediate(texture);
            DestroyImmediate(captureCamera.gameObject);
        }

        AssetDatabase.ImportAsset(assetPath);
        ConfigureImportedTexture(assetPath);
        Texture2D generatedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

        if (applyToTarget && targetView != null && generatedTexture != null)
        {
            Undo.RecordObject(targetView, "应用小地图图片");
            SerializedObject serializedView = new SerializedObject(targetView);
            serializedView.FindProperty("minimapTexture").objectReferenceValue = generatedTexture;
            serializedView.FindProperty("mapWorldCenter").vector3Value = areaCenter;
            serializedView.FindProperty("mapWorldSize").vector2Value = areaSize;
            serializedView.ApplyModifiedProperties();
            targetView.SetMap(generatedTexture, areaCenter, areaSize);
            EditorUtility.SetDirty(targetView);
            EditorSceneManager.MarkSceneDirty(targetView.gameObject.scene);
        }

        lastResult = applyToTarget && targetView != null
            ? $"已生成并应用：{assetPath}"
            : $"已生成：{assetPath}";
    }

    private Camera CreateCaptureCamera()
    {
        GameObject cameraObject = new GameObject("Temporary Minimap Capture Camera");
        cameraObject.hideFlags = HideFlags.HideAndDontSave;

        Camera captureCamera = cameraObject.AddComponent<Camera>();
        captureCamera.orthographic = true;
        captureCamera.orthographicSize = areaSize.y * 0.5f;
        captureCamera.aspect = areaSize.x / areaSize.y;
        captureCamera.transform.position = areaCenter + Vector3.up * captureHeight;
        captureCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        captureCamera.clearFlags = CameraClearFlags.SolidColor;
        captureCamera.backgroundColor = backgroundColor;
        captureCamera.cullingMask = captureLayerMask;
        captureCamera.nearClipPlane = 0.01f;
        captureCamera.farClipPlane = Mathf.Max(1f, captureHeight * 2f);
        return captureCamera;
    }

    private static void ConfigureImportedTexture(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.SaveAndReimport();
    }

    private void PullAreaFromTargetView()
    {
        if (targetView == null)
        {
            lastResult = "未指定小地图传送组件。";
            return;
        }

        SerializedObject serializedView = new SerializedObject(targetView);
        areaCenter = serializedView.FindProperty("mapWorldCenter").vector3Value;
        areaSize = serializedView.FindProperty("mapWorldSize").vector2Value;
        selectionPlaneY = areaCenter.y;
        lastResult = $"已读取 {targetView.name} 的小地图区域。";
    }
}
