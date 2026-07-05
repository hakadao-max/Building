using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneDependencyReportWindow : EditorWindow
{
    private const float SizeColumnWidth = 90f;
    private const float TypeColumnWidth = 120f;
    private const float ButtonColumnWidth = 44f;
    private const float ReferenceButtonColumnWidth = 64f;

    private readonly List<SceneDependencyInfo> dependencies = new List<SceneDependencyInfo>();

    private SceneAsset targetScene;
    private Vector2 scrollPosition;
    private string searchText = string.Empty;
    private string lastResult = "请选择场景后点击“分析依赖资源”。";
    private bool includeSceneFile = true;
    private bool includeScriptsAndAssemblies;
    private long totalSize;

    [MenuItem("工具/场景依赖资源大小排行")]
    private static void Open()
    {
        GetWindow<SceneDependencyReportWindow>("场景依赖排行");
    }

    private void OnEnable()
    {
        if (targetScene == null)
        {
            UseActiveScene();
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("场景依赖资源大小排行", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("统计目标场景通过 AssetDatabase.GetDependencies 找到的项目资源，并按资源源文件的磁盘大小从大到小排列。", MessageType.Info);

        targetScene = (SceneAsset)EditorGUILayout.ObjectField("目标场景", targetScene, typeof(SceneAsset), false);
        includeSceneFile = EditorGUILayout.ToggleLeft("把场景文件本身计入结果", includeSceneFile);
        includeScriptsAndAssemblies = EditorGUILayout.ToggleLeft("包含脚本和程序集文件", includeScriptsAndAssemblies);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("使用当前打开场景", GUILayout.Height(28f)))
        {
            UseActiveScene();
        }

        using (new EditorGUI.DisabledScope(targetScene == null))
        {
            if (GUILayout.Button("分析依赖资源", GUILayout.Height(28f)))
            {
                AnalyzeDependencies();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("执行结果", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(lastResult, MessageType.None);

        if (dependencies.Count <= 0)
        {
            return;
        }

        EditorGUILayout.BeginHorizontal();
        searchText = EditorGUILayout.TextField("筛选", searchText);
        if (GUILayout.Button("复制结果", GUILayout.Width(80f)))
        {
            CopyReportToClipboard();
        }
        EditorGUILayout.EndHorizontal();

        DrawHeader();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach (SceneDependencyInfo dependency in dependencies)
        {
            if (!IsMatchSearch(dependency))
            {
                continue;
            }

            DrawDependencyRow(dependency);
        }
        EditorGUILayout.EndScrollView();
    }

    private void UseActiveScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || string.IsNullOrEmpty(activeScene.path))
        {
            lastResult = "当前活动场景还没有保存，无法作为分析目标。请先保存场景，或手动拖入一个 SceneAsset。";
            return;
        }

        targetScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(activeScene.path);
        lastResult = targetScene != null
            ? $"已选择当前打开场景：{activeScene.path}"
            : "无法从当前活动场景路径加载 SceneAsset。";
    }

    private void AnalyzeDependencies()
    {
        string scenePath = AssetDatabase.GetAssetPath(targetScene);
        if (string.IsNullOrEmpty(scenePath))
        {
            lastResult = "目标场景路径无效，请重新选择一个项目内的场景资源。";
            return;
        }

        string[] dependencyPaths = AssetDatabase.GetDependencies(scenePath, true);
        dependencies.Clear();
        totalSize = 0L;

        foreach (string dependencyPath in dependencyPaths)
        {
            if (ShouldSkipDependency(scenePath, dependencyPath))
            {
                continue;
            }

            SceneDependencyInfo dependency = CreateDependencyInfo(dependencyPath);
            dependencies.Add(dependency);
            totalSize += dependency.Size;
        }

        dependencies.Sort((left, right) => right.Size.CompareTo(left.Size));
        lastResult = $"分析完成：找到 {dependencies.Count} 个依赖资源，总大小 {FormatSize(totalSize)}。";
    }

    private bool ShouldSkipDependency(string scenePath, string dependencyPath)
    {
        if (string.IsNullOrEmpty(dependencyPath))
        {
            return true;
        }

        if (!includeSceneFile && dependencyPath == scenePath)
        {
            return true;
        }

        if (!includeScriptsAndAssemblies && IsCodeOrAssemblyFile(dependencyPath))
        {
            return true;
        }

        return false;
    }

    private static SceneDependencyInfo CreateDependencyInfo(string assetPath)
    {
        string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        long size = File.Exists(fullPath) ? new FileInfo(fullPath).Length : 0L;
        Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
        string assetType = asset != null ? asset.GetType().Name : Path.GetExtension(assetPath);

        return new SceneDependencyInfo(assetPath, assetType, size);
    }

    private static bool IsCodeOrAssemblyFile(string assetPath)
    {
        string extension = Path.GetExtension(assetPath).ToLowerInvariant();
        return extension == ".cs"
            || extension == ".asmdef"
            || extension == ".dll"
            || extension == ".pdb"
            || extension == ".rsp";
    }

    private bool IsMatchSearch(SceneDependencyInfo dependency)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }

        return dependency.AssetPath.ToLowerInvariant().Contains(searchText.ToLowerInvariant())
            || dependency.AssetType.ToLowerInvariant().Contains(searchText.ToLowerInvariant());
    }

    private void DrawHeader()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUILayout.LabelField("资源路径", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("类型", EditorStyles.boldLabel, GUILayout.Width(TypeColumnWidth));
        EditorGUILayout.LabelField("大小", EditorStyles.boldLabel, GUILayout.Width(SizeColumnWidth));
        GUILayout.Space(ButtonColumnWidth * 2f + ReferenceButtonColumnWidth);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawDependencyRow(SceneDependencyInfo dependency)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.SelectableLabel(dependency.AssetPath, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        EditorGUILayout.LabelField(dependency.AssetType, GUILayout.Width(TypeColumnWidth));
        EditorGUILayout.LabelField(FormatSize(dependency.Size), GUILayout.Width(SizeColumnWidth));

        if (GUILayout.Button("定位", GUILayout.Width(ButtonColumnWidth)))
        {
            Object asset = AssetDatabase.LoadMainAssetAtPath(dependency.AssetPath);
            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
        }

        if (GUILayout.Button("打开", GUILayout.Width(ButtonColumnWidth)))
        {
            AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath(dependency.AssetPath));
        }

        if (GUILayout.Button("查引用", GUILayout.Width(ReferenceButtonColumnWidth)))
        {
            FindReferencesInScene(dependency.AssetPath);
        }

        EditorGUILayout.EndHorizontal();
    }

    private static void FindReferencesInScene(string assetPath)
    {
        Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
        if (asset == null)
        {
            EditorUtility.DisplayDialog("查找场景引用", $"无法加载资源：{assetPath}", "确定");
            return;
        }

        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);

        bool executed = EditorApplication.ExecuteMenuItem("Assets/Find References In Scene");
        if (!executed)
        {
            EditorUtility.DisplayDialog(
                "查找场景引用",
                "无法执行 Unity 菜单：Assets/Find References In Scene。请确认当前 Unity 版本支持该菜单，且已打开要搜索的场景。",
                "确定");
        }
    }

    private void CopyReportToClipboard()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("大小\t类型\t资源路径");

        foreach (SceneDependencyInfo dependency in dependencies)
        {
            if (!IsMatchSearch(dependency))
            {
                continue;
            }

            report.Append(FormatSize(dependency.Size));
            report.Append('\t');
            report.Append(dependency.AssetType);
            report.Append('\t');
            report.AppendLine(dependency.AssetPath);
        }

        EditorGUIUtility.systemCopyBuffer = report.ToString();
        lastResult = "已复制当前筛选结果到剪贴板。";
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024L)
        {
            return $"{bytes} B";
        }

        float size = bytes / 1024f;
        if (size < 1024f)
        {
            return $"{size:0.##} KB";
        }

        size /= 1024f;
        if (size < 1024f)
        {
            return $"{size:0.##} MB";
        }

        size /= 1024f;
        return $"{size:0.##} GB";
    }

    private readonly struct SceneDependencyInfo
    {
        public SceneDependencyInfo(string assetPath, string assetType, long size)
        {
            AssetPath = assetPath;
            AssetType = assetType;
            Size = size;
        }

        public string AssetPath { get; }

        public string AssetType { get; }

        public long Size { get; }
    }
}
