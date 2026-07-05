using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class PrefabPackageSizeEstimatorWindow : EditorWindow
{
    private const float TypeColumnWidth = 120f;
    private const float SizeColumnWidth = 90f;
    private const float ButtonColumnWidth = 44f;
    private const float ReferenceButtonColumnWidth = 64f;

    private readonly List<Object> exportTargets = new List<Object>();
    private readonly List<PackageAssetInfo> packageAssets = new List<PackageAssetInfo>();

    private Object objectToAdd;
    private Vector2 targetScrollPosition;
    private Vector2 assetScrollPosition;
    private string searchText = string.Empty;
    private string lastResult = "请添加要导出的预制体或资源，然后点击“分析Package大小”。";
    private bool includeDependencies = true;
    private bool includeScriptsAndAssemblies = true;
    private long sourceTotalSize;
    private long assetFileTotalSize;
    private long metaFileTotalSize;
    private long measuredPackageSize = -1L;
    private int skippedExternalDependencyCount;

    [MenuItem("工具/预制体Package大小预估")]
    private static void Open()
    {
        GetWindow<PrefabPackageSizeEstimatorWindow>("Package大小预估");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("预制体Package大小预估", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "用于导出 .unitypackage 前预估体积。快速分析会统计目标资源及依赖的源文件和 .meta 大小；精确测量会临时导出一个 package 到系统临时目录，读取大小后删除。",
            MessageType.Info);

        DrawTargetControls();

        EditorGUILayout.Space(8f);
        includeDependencies = EditorGUILayout.ToggleLeft("包含依赖资源", includeDependencies);
        includeScriptsAndAssemblies = EditorGUILayout.ToggleLeft("包含脚本和程序集文件", includeScriptsAndAssemblies);

        EditorGUILayout.BeginHorizontal();
        using (new EditorGUI.DisabledScope(exportTargets.Count <= 0))
        {
            if (GUILayout.Button("分析Package大小", GUILayout.Height(30f)))
            {
                AnalyzePackageSize();
            }

            if (GUILayout.Button("临时导出并测量真实Package大小", GUILayout.Height(30f)))
            {
                MeasurePackageSizeByTemporaryExport();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("执行结果", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(lastResult, MessageType.None);

        if (packageAssets.Count <= 0)
        {
            return;
        }

        DrawSummary();
        DrawAssetList();
    }

    private void DrawTargetControls()
    {
        EditorGUILayout.LabelField("导出目标", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        objectToAdd = EditorGUILayout.ObjectField("预制体/资源", objectToAdd, typeof(Object), false);
        using (new EditorGUI.DisabledScope(objectToAdd == null))
        {
            if (GUILayout.Button("添加", GUILayout.Width(64f)))
            {
                AddTarget(objectToAdd);
                objectToAdd = null;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("添加当前Project选择", GUILayout.Height(26f)))
        {
            AddSelectionTargets();
        }

        using (new EditorGUI.DisabledScope(exportTargets.Count <= 0))
        {
            if (GUILayout.Button("清空目标", GUILayout.Width(90f), GUILayout.Height(26f)))
            {
                exportTargets.Clear();
                packageAssets.Clear();
                sourceTotalSize = 0L;
                measuredPackageSize = -1L;
                lastResult = "已清空导出目标。";
            }
        }
        EditorGUILayout.EndHorizontal();

        if (exportTargets.Count <= 0)
        {
            EditorGUILayout.HelpBox("可以拖入 Project 里的 Prefab，或先在 Project 面板选中一个或多个 Prefab 后点击“添加当前Project选择”。", MessageType.None);
            return;
        }

        targetScrollPosition = EditorGUILayout.BeginScrollView(targetScrollPosition, GUILayout.MaxHeight(96f));
        for (int i = exportTargets.Count - 1; i >= 0; i--)
        {
            Object target = exportTargets[i];
            string assetPath = ResolveAssetPath(target);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(target, typeof(Object), false);
            EditorGUILayout.SelectableLabel(assetPath, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            if (GUILayout.Button("移除", GUILayout.Width(54f)))
            {
                exportTargets.RemoveAt(i);
                packageAssets.Clear();
                measuredPackageSize = -1L;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawSummary()
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("大小汇总", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("资源源文件", FormatSize(assetFileTotalSize));
        EditorGUILayout.LabelField(".meta 文件", FormatSize(metaFileTotalSize));
        EditorGUILayout.LabelField("源文件估算合计", FormatSize(sourceTotalSize));
        EditorGUILayout.LabelField("临时导出Package实测", measuredPackageSize >= 0L ? FormatSize(measuredPackageSize) : "未测量");

        if (skippedExternalDependencyCount > 0)
        {
            EditorGUILayout.HelpBox(
                $"已跳过 {skippedExternalDependencyCount} 个非 Assets 目录依赖，这些资源通常不会被打进 .unitypackage。",
                MessageType.Warning);
        }
    }

    private void DrawAssetList()
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.BeginHorizontal();
        searchText = EditorGUILayout.TextField("筛选", searchText);
        if (GUILayout.Button("复制结果", GUILayout.Width(80f)))
        {
            CopyReportToClipboard();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUILayout.LabelField("资源路径", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("类型", EditorStyles.boldLabel, GUILayout.Width(TypeColumnWidth));
        EditorGUILayout.LabelField("源文件", EditorStyles.boldLabel, GUILayout.Width(SizeColumnWidth));
        EditorGUILayout.LabelField(".meta", EditorStyles.boldLabel, GUILayout.Width(SizeColumnWidth));
        EditorGUILayout.LabelField("合计", EditorStyles.boldLabel, GUILayout.Width(SizeColumnWidth));
        GUILayout.Space(ButtonColumnWidth + ReferenceButtonColumnWidth);
        EditorGUILayout.EndHorizontal();

        assetScrollPosition = EditorGUILayout.BeginScrollView(assetScrollPosition);
        foreach (PackageAssetInfo packageAsset in packageAssets)
        {
            if (!IsMatchSearch(packageAsset))
            {
                continue;
            }

            DrawAssetRow(packageAsset);
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawAssetRow(PackageAssetInfo packageAsset)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.SelectableLabel(packageAsset.AssetPath, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        EditorGUILayout.LabelField(packageAsset.AssetType, GUILayout.Width(TypeColumnWidth));
        EditorGUILayout.LabelField(FormatSize(packageAsset.AssetFileSize), GUILayout.Width(SizeColumnWidth));
        EditorGUILayout.LabelField(FormatSize(packageAsset.MetaFileSize), GUILayout.Width(SizeColumnWidth));
        EditorGUILayout.LabelField(FormatSize(packageAsset.TotalSize), GUILayout.Width(SizeColumnWidth));

        if (GUILayout.Button("定位", GUILayout.Width(ButtonColumnWidth)))
        {
            Object asset = AssetDatabase.LoadMainAssetAtPath(packageAsset.AssetPath);
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        if (GUILayout.Button("查引用", GUILayout.Width(ReferenceButtonColumnWidth)))
        {
            FindReferencesInScene(packageAsset.AssetPath);
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

    private void AddSelectionTargets()
    {
        int addedCount = 0;
        foreach (Object selectedObject in Selection.objects)
        {
            if (AddTarget(selectedObject))
            {
                addedCount++;
            }
        }

        lastResult = addedCount > 0
            ? $"已添加 {addedCount} 个导出目标。"
            : "当前选择中没有可添加的项目资源或预制体实例。";
    }

    private bool AddTarget(Object target)
    {
        string assetPath = ResolveAssetPath(target);
        if (string.IsNullOrEmpty(assetPath) || !assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            lastResult = "只能添加 Assets 目录下的项目资源；如果选中的是场景内预制体实例，会自动解析到它的Prefab资源。";
            return false;
        }

        foreach (Object existingTarget in exportTargets)
        {
            if (string.Equals(ResolveAssetPath(existingTarget), assetPath, StringComparison.OrdinalIgnoreCase))
            {
                lastResult = $"目标已存在：{assetPath}";
                return false;
            }
        }

        Object mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
        if (mainAsset == null)
        {
            lastResult = $"无法加载资源：{assetPath}";
            return false;
        }

        exportTargets.Add(mainAsset);
        packageAssets.Clear();
        measuredPackageSize = -1L;
        lastResult = $"已添加目标：{assetPath}";
        return true;
    }

    private void AnalyzePackageSize()
    {
        string[] exportAssetPaths = BuildExportAssetPaths();
        packageAssets.Clear();
        sourceTotalSize = 0L;
        assetFileTotalSize = 0L;
        metaFileTotalSize = 0L;
        measuredPackageSize = -1L;

        foreach (string assetPath in exportAssetPaths)
        {
            PackageAssetInfo packageAsset = CreatePackageAssetInfo(assetPath);
            packageAssets.Add(packageAsset);
            assetFileTotalSize += packageAsset.AssetFileSize;
            metaFileTotalSize += packageAsset.MetaFileSize;
            sourceTotalSize += packageAsset.TotalSize;
        }

        packageAssets.Sort((left, right) => right.TotalSize.CompareTo(left.TotalSize));
        lastResult = $"分析完成：导出目标 {exportTargets.Count} 个，资源 {packageAssets.Count} 个，源文件估算合计 {FormatSize(sourceTotalSize)}。";
    }

    private void MeasurePackageSizeByTemporaryExport()
    {
        string[] exportAssetPaths = BuildExportAssetPaths();
        if (exportAssetPaths.Length <= 0)
        {
            lastResult = "没有可导出的资源。";
            return;
        }

        AnalyzePackageSize();

        string packagePath = Path.Combine(Path.GetTempPath(), $"PrefabPackageSize_{Guid.NewGuid():N}.unitypackage");
        try
        {
            EditorUtility.DisplayProgressBar("测量Package大小", "正在临时导出 .unitypackage ...", 0.5f);
            AssetDatabase.ExportPackage(exportAssetPaths, packagePath, ExportPackageOptions.Default);
            measuredPackageSize = File.Exists(packagePath) ? new FileInfo(packagePath).Length : -1L;
            lastResult = measuredPackageSize >= 0L
                ? $"测量完成：临时导出的Package大小为 {FormatSize(measuredPackageSize)}。临时文件已删除。"
                : "测量失败：Unity没有生成临时Package文件。";
        }
        catch (Exception exception)
        {
            measuredPackageSize = -1L;
            lastResult = $"测量失败：{exception.Message}";
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    private string[] BuildExportAssetPaths()
    {
        var exportAssetPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        skippedExternalDependencyCount = 0;

        foreach (Object target in exportTargets)
        {
            string rootAssetPath = ResolveAssetPath(target);
            if (string.IsNullOrEmpty(rootAssetPath))
            {
                continue;
            }

            if (!includeDependencies)
            {
                AddExportAssetPath(exportAssetPaths, rootAssetPath);
                continue;
            }

            string[] dependencies = AssetDatabase.GetDependencies(rootAssetPath, true);
            foreach (string dependencyPath in dependencies)
            {
                AddExportAssetPath(exportAssetPaths, dependencyPath);
            }
        }

        var sortedPaths = new List<string>(exportAssetPaths);
        sortedPaths.Sort(StringComparer.OrdinalIgnoreCase);
        return sortedPaths.ToArray();
    }

    private void AddExportAssetPath(HashSet<string> exportAssetPaths, string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            return;
        }

        if (!assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            skippedExternalDependencyCount++;
            return;
        }

        if (!includeScriptsAndAssemblies && IsCodeOrAssemblyFile(assetPath))
        {
            return;
        }

        exportAssetPaths.Add(assetPath);
    }

    private static PackageAssetInfo CreatePackageAssetInfo(string assetPath)
    {
        string fullPath = GetFullProjectPath(assetPath);
        long assetFileSize = File.Exists(fullPath) ? new FileInfo(fullPath).Length : 0L;

        string metaPath = $"{fullPath}.meta";
        long metaFileSize = File.Exists(metaPath) ? new FileInfo(metaPath).Length : 0L;

        Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
        string assetType = asset != null ? asset.GetType().Name : Path.GetExtension(assetPath);

        return new PackageAssetInfo(assetPath, assetType, assetFileSize, metaFileSize);
    }

    private static string ResolveAssetPath(Object asset)
    {
        if (asset == null)
        {
            return string.Empty;
        }

        string assetPath = AssetDatabase.GetAssetPath(asset);
        if (!string.IsNullOrEmpty(assetPath))
        {
            return assetPath;
        }

        if (asset is GameObject gameObject)
        {
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
            if (!string.IsNullOrEmpty(prefabPath))
            {
                return prefabPath;
            }
        }

        return string.Empty;
    }

    private static string GetFullProjectPath(string assetPath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
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

    private bool IsMatchSearch(PackageAssetInfo packageAsset)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }

        return packageAsset.AssetPath.ToLowerInvariant().Contains(searchText.ToLowerInvariant())
            || packageAsset.AssetType.ToLowerInvariant().Contains(searchText.ToLowerInvariant());
    }

    private void CopyReportToClipboard()
    {
        var report = new StringBuilder();
        report.AppendLine("合计\t源文件\t.meta\t类型\t资源路径");

        foreach (PackageAssetInfo packageAsset in packageAssets)
        {
            if (!IsMatchSearch(packageAsset))
            {
                continue;
            }

            report.Append(FormatSize(packageAsset.TotalSize));
            report.Append('\t');
            report.Append(FormatSize(packageAsset.AssetFileSize));
            report.Append('\t');
            report.Append(FormatSize(packageAsset.MetaFileSize));
            report.Append('\t');
            report.Append(packageAsset.AssetType);
            report.Append('\t');
            report.AppendLine(packageAsset.AssetPath);
        }

        EditorGUIUtility.systemCopyBuffer = report.ToString();
        lastResult = "已复制当前筛选结果到剪贴板。";
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 0L)
        {
            return "未知";
        }

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

    private readonly struct PackageAssetInfo
    {
        public PackageAssetInfo(string assetPath, string assetType, long assetFileSize, long metaFileSize)
        {
            AssetPath = assetPath;
            AssetType = assetType;
            AssetFileSize = assetFileSize;
            MetaFileSize = metaFileSize;
        }

        public string AssetPath { get; }

        public string AssetType { get; }

        public long AssetFileSize { get; }

        public long MetaFileSize { get; }

        public long TotalSize => AssetFileSize + MetaFileSize;
    }
}
