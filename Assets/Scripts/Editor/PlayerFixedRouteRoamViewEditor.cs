using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerFixedRouteRoamView))]
public sealed class PlayerFixedRouteRoamViewEditor : Editor
{
    private PlayerFixedRouteRoamView roamView;

    private void OnEnable()
    {
        roamView = (PlayerFixedRouteRoamView)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("曲线编辑", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("控制点为绝对世界坐标。选中组件后，可在 Scene 视图直接拖拽控制点编辑漫游曲线。", MessageType.Info);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("追加控制点"))
            {
                Undo.RecordObject(roamView, "追加固定路线漫游控制点");
                roamView.AddControlPointAfterLast();
                EditorUtility.SetDirty(roamView);
            }

            if (GUILayout.Button("重置默认路线"))
            {
                Undo.RecordObject(roamView, "重置固定路线漫游曲线");
                roamView.ResetDefaultRoute();
                EditorUtility.SetDirty(roamView);
            }
        }
    }

    private void OnSceneGUI()
    {
        if (roamView == null)
        {
            return;
        }

        for (int i = 0; i < roamView.ControlPointCount; i++)
        {
            Vector3 worldPosition = roamView.GetControlPointWorldPosition(i);
            float handleSize = HandleUtility.GetHandleSize(worldPosition) * 0.12f;

            Handles.color = Color.yellow;
            Handles.SphereHandleCap(0, worldPosition, Quaternion.identity, handleSize, EventType.Repaint);
            Handles.Label(worldPosition + Vector3.up * handleSize, $"漫游点 {i + 1}");

            EditorGUI.BeginChangeCheck();
            Vector3 newWorldPosition = Handles.PositionHandle(worldPosition, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(roamView, "编辑固定路线漫游控制点");
                roamView.SetControlPointWorldPosition(i, newWorldPosition);
                EditorUtility.SetDirty(roamView);
            }
        }
    }
}
