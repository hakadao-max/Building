using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LabelTextAttribute))]
public sealed class LabelTextDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (attribute is LabelTextAttribute labelTextAttribute)
        {
            label.text = labelTextAttribute.Text;
        }

        bool wasEnabled = GUI.enabled;
        if (fieldInfo != null && fieldInfo.GetCustomAttribute<ReadOnlyAttribute>() != null)
        {
            GUI.enabled = false;
        }

        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = wasEnabled;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
