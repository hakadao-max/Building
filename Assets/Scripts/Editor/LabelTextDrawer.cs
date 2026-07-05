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

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
