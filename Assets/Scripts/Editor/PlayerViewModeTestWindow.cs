using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class PlayerViewModeTestWindow : EditorWindow
{
    private SimplePlayerController controller;
    private string lastResult = "等待执行操作。";

    [MenuItem("测试/玩家视角与漫游测试窗口")]
    private static void Open()
    {
        GetWindow<PlayerViewModeTestWindow>("玩家视角与漫游测试");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("玩家控制器", EditorStyles.boldLabel);
        controller = (SimplePlayerController)EditorGUILayout.ObjectField("控制器", controller, typeof(SimplePlayerController), true);

        EditorGUILayout.HelpBox("可给当前选中对象补齐玩家生命周期控制器、各视角组件，以及移动、手电筒和提示能力组件。", MessageType.Info);

        if (GUILayout.Button("从当前选择获取控制器", GUILayout.Height(28f)))
        {
            PickControllerFromSelection();
        }

        using (new EditorGUI.DisabledScope(controller == null))
        {
            if (GUILayout.Button("补齐并绑定视角组件", GUILayout.Height(32f)))
            {
                EnsureViewComponents();
            }
        }

        EditorGUILayout.Space(12f);
        EditorGUILayout.LabelField("执行结果", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(lastResult, MessageType.None);
    }

    private void PickControllerFromSelection()
    {
        if (Selection.activeGameObject == null)
        {
            lastResult = "未选中任何场景对象。";
            return;
        }

        controller = Selection.activeGameObject.GetComponent<SimplePlayerController>();
        if (controller == null)
        {
            controller = Undo.AddComponent<SimplePlayerController>(Selection.activeGameObject);
            lastResult = $"已给 {Selection.activeGameObject.name} 添加 SimplePlayerController。";
            return;
        }

        lastResult = $"已获取控制器：{controller.name}。";
    }

    private void EnsureViewComponents()
    {
        GameObject controllerObject = controller.gameObject;
        PlayerFirstPersonView firstPersonView = EnsureComponent<PlayerFirstPersonView>(controllerObject);
        PlayerThirdPersonView thirdPersonView = EnsureComponent<PlayerThirdPersonView>(controllerObject);
        PlayerFixedRouteRoamView fixedRouteRoamView = EnsureComponent<PlayerFixedRouteRoamView>(controllerObject);
        PlayerFixedCameraView fixedCameraView = EnsureComponent<PlayerFixedCameraView>(controllerObject);
        PlayerMinimapTeleportView minimapTeleportView = EnsureComponent<PlayerMinimapTeleportView>(controllerObject);
        PlayerDetailInspectView detailInspectView = EnsureComponent<PlayerDetailInspectView>(controllerObject);
        PlayerPerspectivePickupView perspectivePickupView = EnsureComponent<PlayerPerspectivePickupView>(controllerObject);
        PlayerLocomotion locomotion = EnsureComponent<PlayerLocomotion>(controllerObject);
        PlayerFlashlight flashlight = EnsureComponent<PlayerFlashlight>(controllerObject);
        PlayerRangeColorScanner rangeColorScanner = EnsureComponent<PlayerRangeColorScanner>(controllerObject);
        PlayerInteractionHintInput hintInput = EnsureComponent<PlayerInteractionHintInput>(controllerObject);

        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(firstPersonView);
        EditorUtility.SetDirty(thirdPersonView);
        EditorUtility.SetDirty(fixedRouteRoamView);
        EditorUtility.SetDirty(fixedCameraView);
        EditorUtility.SetDirty(minimapTeleportView);
        EditorUtility.SetDirty(detailInspectView);
        EditorUtility.SetDirty(perspectivePickupView);
        EditorUtility.SetDirty(locomotion);
        EditorUtility.SetDirty(flashlight);
        EditorUtility.SetDirty(rangeColorScanner);
        EditorUtility.SetDirty(hintInput);
        EditorSceneManager.MarkSceneDirty(controllerObject.scene);

        lastResult = $"已补齐 {controllerObject.name} 的玩家视角与能力组件。运行后按 1/2/3/4/5/7 切换模式，按 6 开关详情查看。";
    }

    private static T EnsureComponent<T>(GameObject targetObject) where T : Component
    {
        T component = targetObject.GetComponent<T>();
        if (component != null)
        {
            return component;
        }

        return Undo.AddComponent<T>(targetObject);
    }
}
