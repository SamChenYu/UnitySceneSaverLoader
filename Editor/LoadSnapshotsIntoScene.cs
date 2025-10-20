using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public class LoadSnapshotsIntoScene : EditorWindow
{
    [MenuItem("Tools/Load Snapshots Into New Scene")]
    static void LoadSnapshots()
    {
        // 1. Create a new empty scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 2. Path to your snapshots folder
        string folderPath = "Assets/Snapshots";

        if (!Directory.Exists(folderPath))
        {
            Debug.LogError($"Folder not found: {folderPath}");
            return;
        }

        // 3. Load all prefab assets in the folder
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        foreach (string guid in prefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab != null)
            {
                // Instantiate prefab into the scene
                PrefabUtility.InstantiatePrefab(prefab);
                Debug.Log($"Loaded prefab: {prefab.name}");
            }
        }

        // 4. Save the scene
        string scenePath = "Assets/Scenes/GeneratedFromSnapshots.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);
        Debug.Log($"Scene saved to {scenePath}");
    }
}