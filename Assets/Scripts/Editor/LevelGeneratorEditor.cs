using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector properties
        DrawDefaultInspector();

        LevelGenerator generator = (LevelGenerator)target;

        EditorGUILayout.Space(15);
        
        if (GUILayout.Button("Generate Level", GUILayout.Height(30)))
        {
            generator.GenerateLevel();
        }

        if (GUILayout.Button("Clear Level", GUILayout.Height(25)))
        {
            generator.ClearLevel();
        }
    }
}
