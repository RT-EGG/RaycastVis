using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnityOctree))]
public class UnityOctreeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.BeginVertical();
        if (GUILayout.Button("Rebuild")) {
            (target as UnityOctree).Rebuild();
        }
        GUILayout.EndVertical();
    }
}
