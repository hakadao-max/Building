using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SimplePatrolRoute : MonoBehaviour
{
    [LabelText("使用子物体作为路线点")]
    [SerializeField] private bool useChildrenAsWaypoints = true;

    [LabelText("路线点")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>();

    [LabelText("显示路线辅助线")]
    [SerializeField] private bool showGizmos = true;

    [LabelText("首尾相连")]
    [SerializeField] private bool connectLastToFirst = true;

    [LabelText("路线颜色")]
    [SerializeField] private Color routeColor = new Color(0.2f, 0.8f, 1f, 1f);

    [LabelText("路线线宽")]
    [SerializeField] private float routeLineWidth = 4f;

    [LabelText("路线点半径")]
    [SerializeField] private float waypointGizmoRadius = 0.25f;

    public int WaypointCount => useChildrenAsWaypoints ? transform.childCount : waypoints.Count;
    public bool ConnectLastToFirst => connectLastToFirst;

    private void OnValidate()
    {
        routeLineWidth = Mathf.Max(1f, routeLineWidth);
        waypointGizmoRadius = Mathf.Max(0.05f, waypointGizmoRadius);
    }

    [ContextMenu("从子物体刷新路线点")]
    public void RefreshWaypointsFromChildren()
    {
        waypoints.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            waypoints.Add(transform.GetChild(i));
        }
    }

    public bool TryGetWaypointPosition(int index, out Vector3 position)
    {
        Transform waypoint = GetWaypoint(index);
        if (waypoint == null)
        {
            position = transform.position;
            return false;
        }

        position = waypoint.position;
        return true;
    }

    public Transform GetWaypoint(int index)
    {
        if (index < 0 || index >= WaypointCount)
        {
            return null;
        }

        if (useChildrenAsWaypoints)
        {
            return transform.GetChild(index);
        }

        return waypoints[index];
    }

    public int ClampWaypointIndex(int index)
    {
        if (WaypointCount <= 0)
        {
            return 0;
        }

        return Mathf.Clamp(index, 0, WaypointCount - 1);
    }

    private void OnDrawGizmos()
    {
        DrawRouteGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        DrawRouteGizmos();
    }

    private void DrawRouteGizmos()
    {
        if (!showGizmos || WaypointCount <= 0)
        {
            return;
        }

        Gizmos.color = routeColor;
        Vector3? firstPosition = null;
        Vector3? previousPosition = null;

        for (int i = 0; i < WaypointCount; i++)
        {
            if (!TryGetWaypointPosition(i, out Vector3 waypointPosition))
            {
                continue;
            }

            Gizmos.DrawSphere(waypointPosition, waypointGizmoRadius);

            if (!firstPosition.HasValue)
            {
                firstPosition = waypointPosition;
            }

            if (previousPosition.HasValue)
            {
                DrawRouteLine(previousPosition.Value, waypointPosition);
            }

            previousPosition = waypointPosition;
        }

        if (connectLastToFirst && firstPosition.HasValue && previousPosition.HasValue && WaypointCount > 2)
        {
            DrawRouteLine(previousPosition.Value, firstPosition.Value);
        }
    }

    private void DrawRouteLine(Vector3 from, Vector3 to)
    {
#if UNITY_EDITOR
        Handles.color = routeColor;
        Handles.DrawAAPolyLine(routeLineWidth, from, to);
#else
        Gizmos.DrawLine(from, to);
#endif
    }
}
