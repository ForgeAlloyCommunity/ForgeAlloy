using System.Collections.Generic;
using System;
using Forge.Networking.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Forge.Factory;
using Forge.Networking.Unity.Serialization;
using System.Linq;
using Forge.Serialization;
using System.Diagnostics;

[CustomEditor(typeof(ForgeEngineFacade))]
public class ForgeEngineFacadeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ForgeEngineFacade script = (ForgeEngineFacade)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scene Management", EditorStyles.boldLabel);

        // Scene selection dropdown
        string[] scenes = AssetDatabase.FindAssets("t:Scene")
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => System.IO.Path.GetFileNameWithoutExtension(path))
            .ToArray();

        int currentIndex = Array.IndexOf(scenes, script.CurrentMap);
        int selectedIndex = EditorGUILayout.Popup("Select Scene", currentIndex, scenes);
        if (selectedIndex >= 0 && selectedIndex < scenes.Length)
            script.CurrentMap = scenes[selectedIndex];

        // Add scene button
        if (GUILayout.Button("Add Selected Scene to Build Settings"))
        {
            AddSceneToBuildSettings(script.CurrentMap);
        }
    }

    private void AddSceneToBuildSettings(string sceneName)
    {
        // Find the path of the scene by its name
        string scenePath = AssetDatabase.FindAssets(sceneName + " t:Scene")
            .Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault();

        // If a scene with the specified name is found
        if (!string.IsNullOrEmpty(scenePath))
        {
            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            // Check if the scene is already in the build settings
            if (!buildScenes.Any(s => s.path == scenePath))
            {
                // Add the scene to the build settings
                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = buildScenes.ToArray();
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("Scene '" + sceneName + "' not found in the project");
        }
    }

}

