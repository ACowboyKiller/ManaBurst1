using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TransparencyCaptureToFile))]
public class TransparencyCaptureToFileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Snapshot")) ((TransparencyCaptureToFile)target).Snap();
    }
}