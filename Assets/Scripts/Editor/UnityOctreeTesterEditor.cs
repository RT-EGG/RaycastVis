using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnityOctreeTester))]
public class UnityOctreeTesterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.BeginVertical();
        {
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("generate")) {
                    (target as UnityOctreeTester).GenerateObjects();
                }
                if (GUILayout.Button("clear ")) {
                    (target as UnityOctreeTester).Clear();
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        return;
    }
}
