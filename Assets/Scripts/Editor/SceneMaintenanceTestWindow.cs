using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneMaintenanceTestWindow : EditorWindow
{
    private MonoScript sourceScript;
    private MonoScript scriptToAdd;
    private Vector2 scrollPosition;
    private string lastResult = "等待执行操作。";

    [MenuItem("测试/场景维护测试窗口")]
    private static void Open()
    {
        GetWindow<SceneMaintenanceTestWindow>("场景维护测试");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.LabelField("场景清理", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("清理当前已打开场景中所有 GameObject 上丢失的脚本引用。执行前建议保存场景。", MessageType.Info);

        if (GUILayout.Button("一键清理场景中丢失的脚本", GUILayout.Height(32f)))
        {
            CleanMissingScriptsInOpenScenes();
        }

        EditorGUILayout.Space(12f);
        EditorGUILayout.LabelField("交互区域修复", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("将当前已打开场景中所有 InteractableArea 同对象上的 SphereCollider 修复为 Is Trigger。", MessageType.Info);

        if (GUILayout.Button("一键修复 InteractableArea 的 SphereCollider 为 Trigger", GUILayout.Height(32f)))
        {
            FixInteractableAreaSphereColliders();
        }

        EditorGUILayout.Space(12f);
        EditorGUILayout.LabelField("按脚本批量挂载", EditorStyles.boldLabel);
        sourceScript = (MonoScript)EditorGUILayout.ObjectField("含有这个脚本", sourceScript, typeof(MonoScript), false);
        scriptToAdd = (MonoScript)EditorGUILayout.ObjectField("自动挂载脚本", scriptToAdd, typeof(MonoScript), false);

        EditorGUILayout.HelpBox("会遍历当前已打开场景：如果 GameObject 已挂“含有这个脚本”，且尚未挂“自动挂载脚本”，就自动添加后者。", MessageType.Info);

        using (new EditorGUI.DisabledScope(sourceScript == null || scriptToAdd == null))
        {
            if (GUILayout.Button("为匹配对象自动挂载脚本", GUILayout.Height(32f)))
            {
                AddScriptToObjectsWithSourceScript();
            }
        }

        EditorGUILayout.Space(12f);
        EditorGUILayout.LabelField("执行结果", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(lastResult, MessageType.None);

        EditorGUILayout.EndScrollView();
    }

    private void CleanMissingScriptsInOpenScenes()
    {
        int cleanedGameObjectCount = 0;
        int removedScriptCount = 0;

        foreach (GameObject gameObject in EnumerateLoadedSceneObjects())
        {
            int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
            if (missingCount <= 0)
            {
                continue;
            }

            Undo.RegisterCompleteObjectUndo(gameObject, "清理丢失脚本");
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
            EditorSceneManager.MarkSceneDirty(gameObject.scene);

            cleanedGameObjectCount++;
            removedScriptCount += missingCount;
        }

        lastResult = $"清理完成：处理 {cleanedGameObjectCount} 个对象，移除 {removedScriptCount} 个丢失脚本引用。";
    }

    private void FixInteractableAreaSphereColliders()
    {
        int interactableAreaCount = 0;
        int missingColliderCount = 0;
        int fixedColliderCount = 0;

        foreach (GameObject gameObject in EnumerateLoadedSceneObjects())
        {
            InteractableArea interactableArea = gameObject.GetComponent<InteractableArea>();
            if (interactableArea == null)
            {
                continue;
            }

            interactableAreaCount++;

            SphereCollider sphereCollider = gameObject.GetComponent<SphereCollider>();
            if (sphereCollider == null)
            {
                missingColliderCount++;
                continue;
            }

            if (sphereCollider.isTrigger)
            {
                continue;
            }

            Undo.RecordObject(sphereCollider, "修复交互区域 Trigger");
            sphereCollider.isTrigger = true;
            EditorUtility.SetDirty(sphereCollider);
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
            fixedColliderCount++;
        }

        lastResult = $"修复完成：找到 {interactableAreaCount} 个 InteractableArea，修复 {fixedColliderCount} 个 SphereCollider，缺少 SphereCollider 的对象 {missingColliderCount} 个。";
    }

    private void AddScriptToObjectsWithSourceScript()
    {
        Type sourceType = GetMonoBehaviourType(sourceScript, "含有这个脚本");
        Type addType = GetMonoBehaviourType(scriptToAdd, "自动挂载脚本");

        if (sourceType == null || addType == null)
        {
            return;
        }

        int matchedCount = 0;
        int addedCount = 0;

        foreach (GameObject gameObject in EnumerateLoadedSceneObjects())
        {
            if (gameObject.GetComponent(sourceType) == null)
            {
                continue;
            }

            matchedCount++;

            if (gameObject.GetComponent(addType) != null)
            {
                continue;
            }

            Undo.AddComponent(gameObject, addType);
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
            addedCount++;
        }

        lastResult = $"挂载完成：匹配 {matchedCount} 个对象，新增 {addedCount} 个 {addType.Name} 组件。";
    }

    private Type GetMonoBehaviourType(MonoScript monoScript, string fieldName)
    {
        Type scriptType = monoScript != null ? monoScript.GetClass() : null;
        if (scriptType == null)
        {
            lastResult = $"{fieldName} 无法解析脚本类型，请选择一个有效的 MonoBehaviour 脚本。";
            return null;
        }

        if (!typeof(MonoBehaviour).IsAssignableFrom(scriptType))
        {
            lastResult = $"{fieldName} 必须是继承 MonoBehaviour 的脚本：{scriptType.Name}";
            return null;
        }

        if (scriptType.IsAbstract)
        {
            lastResult = $"{fieldName} 不能是抽象类：{scriptType.Name}";
            return null;
        }

        return scriptType;
    }

    private static GameObject[] EnumerateLoadedSceneObjects()
    {
        var allObjects = new System.Collections.Generic.List<GameObject>();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded)
            {
                continue;
            }

            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                foreach (Transform transform in rootObject.GetComponentsInChildren<Transform>(true))
                {
                    allObjects.Add(transform.gameObject);
                }
            }
        }

        return allObjects.ToArray();
    }
}
