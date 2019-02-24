using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(FABRIK), true)]
public class FABRIKEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FABRIK fabrik = target as FABRIK;

        if (GUILayout.Button("Generate FABRIK Effector System"))
        {
            fabrik.CreateSystem();
        }
    }
}
