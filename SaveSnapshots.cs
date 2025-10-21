#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class SaveSnapshots : MonoBehaviour
{
    [ContextMenu("Save Scene Snapshot Prefabs")]
    void SaveSnapshotPrefabs()
    {
        string folderPath = "Assets/Snapshots";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // Get all root objects in the scene
        GameObject[] rootObjects = gameObject.scene.GetRootGameObjects();

        foreach (var root in rootObjects)
        {
            SaveGameObjectAsPrefab(root, folderPath);
        }

        Debug.Log("Scene snapshot prefabs saved!");
    }

    void SaveGameObjectAsPrefab(GameObject go, string folderPath)
    {
        // Clone the object first to avoid prefab instance issues
        GameObject clone = Instantiate(go);
        clone.name = go.name;

        // Generate a unique path
        string prefabPath = Path.Combine(folderPath, clone.name + ".prefab");
        prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

        // Save the cloned object as a prefab (children are included automatically)
        PrefabUtility.SaveAsPrefabAsset(clone, prefabPath);

        // Destroy the temporary clone
        DestroyImmediate(clone);
    }
}
#endif
