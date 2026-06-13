using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SimonLine))]
public class SimonLineDrawer : PropertyDrawer
{
    private const float Padding = 4f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = Padding;

        // text (TextArea — variable height)
        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("text")) + Padding;

        // type
        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("type")) + Padding;

        // delayBeforeAction
        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("delayBeforeAction")) + Padding;

        SimonLineType type = (SimonLineType)property.FindPropertyRelative("type").enumValueIndex;
        switch (type)
        {
            case SimonLineType.Action:
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("actionTimeLimit")) + Padding;
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("requiredInputAction")) + Padding;
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("failureMessage")) + Padding;
                break;
            case SimonLineType.Rule:
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("rule")) + Padding;
                break;
            case SimonLineType.Event:
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("onTriggered")) + Padding;
                break;
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect rect = new Rect(position.x, position.y + Padding, position.width, 0f);

        // Text
        SerializedProperty textProp = property.FindPropertyRelative("text");
        rect.height = EditorGUI.GetPropertyHeight(textProp);
        EditorGUI.PropertyField(rect, textProp, GUIContent.none);
        rect.y += rect.height + Padding;

        // Type
        SerializedProperty typeProp = property.FindPropertyRelative("type");
        rect.height = EditorGUI.GetPropertyHeight(typeProp);
        EditorGUI.PropertyField(rect, typeProp);
        rect.y += rect.height + Padding;

        SimonLineType type = (SimonLineType)typeProp.enumValueIndex;

        // Delay
        SerializedProperty delayProp = property.FindPropertyRelative("delayBeforeAction");
        rect.height = EditorGUI.GetPropertyHeight(delayProp);
        EditorGUI.PropertyField(rect, delayProp);
        rect.y += rect.height + Padding;

        switch (type)
        {
            case SimonLineType.Action:
                SerializedProperty timeLimitProp = property.FindPropertyRelative("actionTimeLimit");
                rect.height = EditorGUI.GetPropertyHeight(timeLimitProp);
                EditorGUI.PropertyField(rect, timeLimitProp);
                rect.y += rect.height + Padding;

                SerializedProperty inputProp = property.FindPropertyRelative("requiredInputAction");
                rect.height = EditorGUI.GetPropertyHeight(inputProp);
                EditorGUI.PropertyField(rect, inputProp);
                rect.y += rect.height + Padding;

                SerializedProperty failProp = property.FindPropertyRelative("failureMessage");
                rect.height = EditorGUI.GetPropertyHeight(failProp);
                EditorGUI.PropertyField(rect, failProp);
                rect.y += rect.height + Padding;
                break;

            case SimonLineType.Rule:
                SerializedProperty ruleProp = property.FindPropertyRelative("rule");
                rect.height = EditorGUI.GetPropertyHeight(ruleProp);
                EditorGUI.PropertyField(rect, ruleProp);
                rect.y += rect.height + Padding;
                break;

            case SimonLineType.Event:
                SerializedProperty eventProp = property.FindPropertyRelative("onTriggered");
                rect.height = EditorGUI.GetPropertyHeight(eventProp);
                EditorGUI.PropertyField(rect, eventProp);
                rect.y += rect.height + Padding;
                break;
        }

        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(RuleDefinition))]
public class RuleDefinitionDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        // Draw type dropdown
        SerializedProperty typeProp = property.FindPropertyRelative("type");
        EditorGUI.PropertyField(rect, typeProp);
        rect.y += EditorGUIUtility.singleLineHeight + 2;

        // Draw float value with a label that describes what it does
        SerializedProperty floatProp = property.FindPropertyRelative("floatValue");
        RuleType ruleType = (RuleType)typeProp.enumValueIndex;
        string valueLabel = ruleType switch
        {
            RuleType.JumpBoost => "Jump Multiplier",
            RuleType.SpeedBoost => "Speed Multiplier",
            RuleType.GravityChange => "Gravity Multiplier",
            _ => "Float Value"
        };
        EditorGUI.PropertyField(rect, floatProp, new GUIContent(valueLabel));

        EditorGUI.EndProperty();
    }
}