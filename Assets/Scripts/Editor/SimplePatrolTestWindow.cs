using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class SimplePatrolTestWindow : EditorWindow
{
    private SimplePatrolRoute route;
    private GameObject prefabToSpawn;
    private int waypointCount = 4;
    private float waypointSpacing = 3f;
    private Vector2 scrollPosition;
    private string lastResult = "等待执行操作。";

    [MenuItem("测试/基础AI巡逻测试窗口")]
    private static void Open()
    {
        GetWindow<SimplePatrolTestWindow>("基础AI巡逻测试");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.LabelField("创建路线", EditorStyles.boldLabel);
        waypointCount = Mathf.Max(2, EditorGUILayout.IntField("路线点数量", waypointCount));
        waypointSpacing = Mathf.Max(0.5f, EditorGUILayout.FloatField("路线点间距", waypointSpacing));
        EditorGUILayout.HelpBox("会在选中对象位置创建路线；未选中对象时创建在世界原点。路线点是子物体，创建后可直接在 Scene 视图拖拽编辑。", MessageType.Info);

        if (GUILayout.Button("创建默认巡逻路线", GUILayout.Height(32f)))
        {
            CreateDefaultRoute();
        }

        EditorGUILayout.Space(12f);
        EditorGUILayout.LabelField("绑定角色", EditorStyles.boldLabel);
        route = (SimplePatrolRoute)EditorGUILayout.ObjectField("巡逻路线", route, typeof(SimplePatrolRoute), true);
        prefabToSpawn = (GameObject)EditorGUILayout.ObjectField("要生成的预制体", prefabToSpawn, typeof(GameObject), false);
        EditorGUILayout.HelpBox("可把预制体生成到路线起点，也可给当前选中的场景对象批量添加 SimplePatrolAgent 并绑定路线。", MessageType.Info);

        using (new EditorGUI.DisabledScope(route == null || prefabToSpawn == null))
        {
            if (GUILayout.Button("在路线起点生成预制体并绑定AI", GUILayout.Height(32f)))
            {
                SpawnPrefabOnRoute();
            }
        }

        using (new EditorGUI.DisabledScope(route == null || Selection.gameObjects.Length == 0))
        {
            if (GUILayout.Button("给选中对象添加并绑定巡逻AI", GUILayout.Height(32f)))
            {
                AddAgentToSelection();
            }
        }

        EditorGUILayout.Space(12f);
        EditorGUILayout.LabelField("执行结果", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(lastResult, MessageType.None);

        EditorGUILayout.EndScrollView();
    }

    private void CreateDefaultRoute()
    {
        Vector3 center = Selection.activeTransform != null ? Selection.activeTransform.position : Vector3.zero;
        GameObject routeObject = new GameObject("Simple Patrol Route");
        Undo.RegisterCreatedObjectUndo(routeObject, "创建默认巡逻路线");
        routeObject.transform.position = center;

        SimplePatrolRoute createdRoute = routeObject.AddComponent<SimplePatrolRoute>();

        for (int i = 0; i < waypointCount; i++)
        {
            GameObject waypointObject = new GameObject($"Waypoint {i + 1:00}");
            waypointObject.transform.SetParent(routeObject.transform, false);
            waypointObject.transform.localPosition = new Vector3(i * waypointSpacing, 0f, 0f);
        }

        route = createdRoute;
        Selection.activeGameObject = routeObject;
        EditorSceneManager.MarkSceneDirty(routeObject.scene);
        lastResult = $"已创建路线：{routeObject.name}，包含 {waypointCount} 个路线点。";
    }

    private void SpawnPrefabOnRoute()
    {
        if (!route.TryGetWaypointPosition(0, out Vector3 spawnPosition))
        {
            lastResult = "生成失败：路线没有可用的起点。";
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
        if (instance == null)
        {
            lastResult = "生成失败：无法实例化指定预制体。";
            return;
        }

        Undo.RegisterCreatedObjectUndo(instance, "生成巡逻AI预制体");
        instance.transform.position = spawnPosition;
        SimplePatrolAgent agent = EnsureAgent(instance);
        agent.SetRoute(route);

        Selection.activeGameObject = instance;
        EditorUtility.SetDirty(agent);
        EditorSceneManager.MarkSceneDirty(instance.scene);
        lastResult = $"已生成并绑定：{instance.name}。速度、停留时间和动画状态可在 Inspector 的 SimplePatrolAgent 中继续调整。";
    }

    private void AddAgentToSelection()
    {
        int addedCount = 0;
        int boundCount = 0;

        foreach (GameObject selectedObject in Selection.gameObjects)
        {
            SimplePatrolAgent agent = selectedObject.GetComponent<SimplePatrolAgent>();
            if (agent == null)
            {
                agent = Undo.AddComponent<SimplePatrolAgent>(selectedObject);
                addedCount++;
            }

            agent.SetRoute(route);
            EditorUtility.SetDirty(agent);
            EditorSceneManager.MarkSceneDirty(selectedObject.scene);
            boundCount++;
        }

        lastResult = $"已处理 {boundCount} 个对象，新增 {addedCount} 个 SimplePatrolAgent，并绑定到路线 {route.name}。";
    }

    private static SimplePatrolAgent EnsureAgent(GameObject instance)
    {
        SimplePatrolAgent agent = instance.GetComponent<SimplePatrolAgent>();
        if (agent != null)
        {
            return agent;
        }

        return Undo.AddComponent<SimplePatrolAgent>(instance);
    }
}
