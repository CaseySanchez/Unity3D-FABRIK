using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(FABRIKEffector))]
public class FABRIKEffectorEditor : Editor
{
    private Quaternion rotation;

    public void OnEnable()
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty axisConstraintProperty = serializedObject.FindProperty("axisConstraint");

        rotation = Quaternion.LookRotation(axisConstraintProperty.vector3Value);
    }

    public override void OnInspectorGUI()
    {
        bool modified = false;

        DrawDefaultInspector();

        SerializedObject serializedObject = new SerializedObject(target);

        serializedObject.Update();

        SerializedProperty swingConstraintProperty = serializedObject.FindProperty("swingConstraint");
        SerializedProperty twistConstraintProperty = serializedObject.FindProperty("twistConstraint");
        SerializedProperty axisConstraintProperty = serializedObject.FindProperty("axisConstraint");

        bool swingToggle = EditorGUILayout.Toggle("Swing Constraint", !float.IsNaN(swingConstraintProperty.floatValue));

        float swingConstraint;

        if (swingToggle)
        {
            swingConstraint = Mathf.Clamp(EditorGUILayout.FloatField(swingConstraintProperty.floatValue), 0.0F, 360.0F);

            if (float.IsInfinity(swingConstraint) || float.IsNaN(swingConstraint))
            {
                swingConstraint = 0.0F;
            }
        }
        else
        {
            swingConstraint = float.NaN;
        }

        modified = modified || swingConstraint != swingConstraintProperty.floatValue;

        bool twistToggle = EditorGUILayout.Toggle("Twist Constraint", !float.IsNaN(twistConstraintProperty.floatValue));

        float twistConstraint;

        if (twistToggle)
        {
            twistConstraint = Mathf.Clamp(EditorGUILayout.FloatField(twistConstraintProperty.floatValue), 0.0F, 360.0F);

            if (float.IsInfinity(twistConstraint) || float.IsNaN(twistConstraint))
            {
                twistConstraint = 0.0F;
            }
        }
        else
        {
            twistConstraint = float.NaN;
        }

        modified = modified || twistConstraint != twistConstraintProperty.floatValue;

        if (modified)
        {
            swingConstraintProperty.floatValue = swingConstraint;
            twistConstraintProperty.floatValue = twistConstraint;

            serializedObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
        }
    }

    public void OnSceneGUI()
    {
        FABRIKEffector effector = target as FABRIKEffector;
        Transform transform = effector.transform;

        EditorGUI.BeginChangeCheck();

        Quaternion new_rotation = Handles.RotationHandle(rotation, transform.position);

        Handles.ArrowHandleCap(0, transform.position, new_rotation, 1.0F, EventType.Repaint);

        if (EditorGUI.EndChangeCheck())
        {
            rotation = new_rotation;

            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty axisConstraintProperty = serializedObject.FindProperty("axisConstraint");

            axisConstraintProperty.vector3Value = new_rotation * Vector3.forward;

            serializedObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
        }
    }
}
