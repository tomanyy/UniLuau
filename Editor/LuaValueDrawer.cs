using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LuaValue))]
public sealed class LuaAttributeDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var type = property.FindPropertyRelative("type");
        string field = ValueField((LuaValueType)type.enumValueIndex);
        float valueHeight = field != null ? EditorGUI.GetPropertyHeight(property.FindPropertyRelative(field), true) : EditorGUIUtility.singleLineHeight;
        bool isVector = (LuaValueType)type.enumValueIndex == LuaValueType.Vector2 || (LuaValueType)type.enumValueIndex == LuaValueType.Vector3;
        if (isVector) valueHeight = EditorGUIUtility.singleLineHeight + 14f;
        return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + valueHeight + (isVector ? 14f : 8f);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        float line = EditorGUIUtility.singleLineHeight;
        float gap = EditorGUIUtility.standardVerticalSpacing;
        Rect card = new Rect(position.x, position.y + 2f, position.width, position.height - 4f);
        EditorGUI.DrawRect(card, EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.045f) : new Color(0f, 0f, 0f, 0.045f));
        Rect row = new Rect(position.x + 6f, position.y + 3f, position.width - 12f, line);
        var name = property.FindPropertyRelative("name");
        var type = property.FindPropertyRelative("type");
        float typeWidth = Mathf.Min(130f, row.width * 0.4f);
        EditorGUI.PropertyField(new Rect(row.x, row.y, row.width - typeWidth - 6f, line), name, GUIContent.none);
        EditorGUI.PropertyField(new Rect(row.xMax - typeWidth, row.y, typeWidth, line), type, GUIContent.none);
        row.y += line + gap;
        row.width = card.width - 12f;
        string field = ValueField((LuaValueType)type.enumValueIndex);
        if (field != null)
        {
            var value = property.FindPropertyRelative(field);
            var valueType = (LuaValueType)type.enumValueIndex;
            row.height = valueType == LuaValueType.Vector2 || valueType == LuaValueType.Vector3 ? line + 14f : EditorGUI.GetPropertyHeight(value, true);
            var valueLabel = new GUIContent(ValueLabel(valueType));
            if (valueType == LuaValueType.Vector2) value.vector2Value = EditorGUI.Vector2Field(row, valueLabel, value.vector2Value);
            else if (valueType == LuaValueType.Vector3) value.vector3Value = EditorGUI.Vector3Field(row, valueLabel, value.vector3Value);
            else EditorGUI.PropertyField(row, value, valueLabel, true);
        }
        EditorGUI.EndProperty();
    }

    static string ValueField(LuaValueType type)
    {
        switch (type)
        {
            case LuaValueType.Number: return "number";
            case LuaValueType.Boolean: return "boolean";
            case LuaValueType.String: return "text";
            case LuaValueType.Vector2: return "vector2";
            case LuaValueType.Vector3: return "vector";
            case LuaValueType.Color: return "color";
            case LuaValueType.Object: return "target";
            case LuaValueType.Integer: return "integer";
            default: return null;
        }
    }

    static string ValueLabel(LuaValueType type)
    {
        switch (type)
        {
            case LuaValueType.Number: return "Number";
            case LuaValueType.Boolean: return "Boolean";
            case LuaValueType.String: return "Text";
            case LuaValueType.Vector2: return "Vector 2";
            case LuaValueType.Vector3: return "Vector 3";
            case LuaValueType.Color: return "Color";
            case LuaValueType.Object: return "Game Object";
            case LuaValueType.Integer: return "Integer";
            default: return "Value";
        }
    }
}
