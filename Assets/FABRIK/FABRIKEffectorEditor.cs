using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(FABRIKEffector))]
public class FABRIKEffectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SerializedObject serializedObject = new SerializedObject(target);

        serializedObject.Update();

        SerializedProperty swingConstraintProperty = serializedObject.FindProperty("swingConstraint");
        SerializedProperty twistConstraintProperty = serializedObject.FindProperty("twistConstraint");

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

        if (swingConstraint != swingConstraintProperty.floatValue || twistConstraint != twistConstraintProperty.floatValue)
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

        if (transform.parent != null && transform.parent.parent != null)
        {

            // Take our world-space direction and world-space up vector of the constraining rotation
            // Multiply this by the inverse of the constraining rotation to derive a local rotation
            Quaternion rotation = Quaternion.Inverse(transform.parent.parent.rotation) * Quaternion.LookRotation(transform.localPosition, transform.parent.rotation * Vector3.up);

            Quaternion swing, twist;

            // Decompose our local rotation to swing-twist about the forward vector of the constraining rotation
            rotation.Decompose(transform.parent.rotation * Vector3.forward, out swing, out twist);
            
            Handles.color = new Color(1.0f, 0.8f, 0.6f, 0.2f);
            Handles.DrawSolidArc(transform.parent.position, 
                transform.parent.parent.rotation * swing * Vector3.right, 
                transform.parent.parent.rotation * swing * Quaternion.AngleAxis(-effector.swingConstraint * 0.5F, swing * Vector3.right) * Vector3.forward, effector.swingConstraint, 5.0f);

            Handles.color = new Color(0.6f, 0.8f, 1.0f, 0.2f);
            Handles.DrawSolidArc(transform.parent.position, 
                transform.parent.parent.rotation * twist * Vector3.right,
                transform.parent.parent.rotation * twist * Quaternion.AngleAxis(-effector.twistConstraint * 0.5F, twist * Vector3.right) * Vector3.forward, effector.twistConstraint, 5.0f);

            // Constrain the swing and twist quaternions
            if (effector.SwingConstrained)
            {
                swing = swing.Constrain(effector.swingConstraint * Mathf.Deg2Rad);
            }

            if (effector.TwistConstrained)
            {
                twist = twist.Constrain(effector.twistConstraint * Mathf.Deg2Rad);
            }

            // Multiply the constrained swing-twist by our constraining rotation to get a world-space rotation
            //transform.position = transform.parent.position + transform.parent.parent.rotation * swing * twist * Vector3.forward * transform.localPosition.magnitude;
        }
    }
}
