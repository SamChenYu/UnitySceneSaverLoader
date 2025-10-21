#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class SaveDeepSnapshots : MonoBehaviour
{
    [ContextMenu("Save Full Deep Scene Snapshot Prefabs")]
    void SaveDeepSnapshotPrefabs()
    {
        string folderPath = "Assets/Snapshots/DeepCopy";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        GameObject[] rootObjects = gameObject.scene.GetRootGameObjects();

        foreach (var root in rootObjects)
        {
            GameObject clone = Instantiate(root);
            clone.name = root.name;

            Dictionary<Object, Object> assetMap = new Dictionary<Object, Object>();
            DeepCopyAssets(clone, folderPath, assetMap);

            string prefabPath = Path.Combine(folderPath, clone.name + ".prefab");
            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);
            PrefabUtility.SaveAsPrefabAsset(clone, prefabPath);

            DestroyImmediate(clone);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Full deep scene snapshot prefabs saved!");
    }

    void DeepCopyAssets(GameObject go, string folderPath, Dictionary<Object, Object> assetMap)
    {
        // Copy Meshes
        foreach (var meshFilter in go.GetComponentsInChildren<MeshFilter>(true))
        {
            if (meshFilter.sharedMesh == null) continue;
            meshFilter.sharedMesh = DuplicateAsset(meshFilter.sharedMesh, folderPath, assetMap) as Mesh;
        }

        // Copy SkinnedMeshRenderer meshes
        foreach (var skinned in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (skinned.sharedMesh == null) continue;
            skinned.sharedMesh = DuplicateAsset(skinned.sharedMesh, folderPath, assetMap) as Mesh;
        }

        // Copy Materials and Textures
        foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
        {
            var mats = renderer.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                Material matCopy = DuplicateAsset(mats[i], folderPath, assetMap) as Material;
                CopyMaterialTextures(matCopy, folderPath, assetMap);
                mats[i] = matCopy;
            }
            renderer.sharedMaterials = mats;
        }

        // Copy AudioClips
        foreach (var audio in go.GetComponentsInChildren<AudioSource>(true))
        {
            if (audio.clip == null) continue;
            audio.clip = DuplicateAsset(audio.clip, folderPath, assetMap) as AudioClip;
        }

        // Copy ScriptableObjects in MonoBehaviours
        foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb == null) continue;
            var soFields = mb.GetType().GetFields(System.Reflection.BindingFlags.Public |
                                                  System.Reflection.BindingFlags.NonPublic |
                                                  System.Reflection.BindingFlags.Instance);
            foreach (var field in soFields)
            {
                if (typeof(ScriptableObject).IsAssignableFrom(field.FieldType))
                {
                    ScriptableObject original = field.GetValue(mb) as ScriptableObject;
                    if (original != null)
                    {
                        ScriptableObject copy = DuplicateAsset(original, folderPath, assetMap) as ScriptableObject;
                        field.SetValue(mb, copy);
                    }
                }
            }
        }

        // Recursively process children
        foreach (Transform child in go.transform)
        {
            DeepCopyAssets(child.gameObject, folderPath, assetMap);
        }
    }

    void CopyMaterialTextures(Material mat, string folderPath, Dictionary<Object, Object> assetMap)
    {
        if (mat == null) return;

        var texProps = mat.GetTexturePropertyNames();
        foreach (var prop in texProps)
        {
            Texture tex = mat.GetTexture(prop);
            if (tex == null) continue;

            Texture copyTex = DuplicateAsset(tex, folderPath, assetMap) as Texture;
            mat.SetTexture(prop, copyTex);
        }
    }

    Object DuplicateAsset(Object original, string folderPath, Dictionary<Object, Object> assetMap)
    {
        if (original == null) return null;

        // Return already copied asset
        if (assetMap.ContainsKey(original))
            return assetMap[original];

        string ext = GetAssetExtension(original);
        string path = Path.Combine(folderPath, original.name + "_copy" + ext);
        path = AssetDatabase.GenerateUniqueAssetPath(path);

        Object copy = null;

        if (original is Mesh)
            copy = Object.Instantiate(original) as Mesh;
        else if (original is Material)
            copy = new Material(original as Material);
        else if (original is Texture2D)
            copy = DuplicateTexture(original as Texture2D);
        else if (original is ScriptableObject)
            copy = Object.Instantiate(original) as ScriptableObject;
        else if (original is AudioClip)
            copy = Object.Instantiate(original) as AudioClip;
        else
        {
            Debug.LogWarning("Unsupported asset type: " + original.GetType().Name);
            return original;
        }

        AssetDatabase.CreateAsset(copy, path);
        assetMap[original] = copy;
        return copy;
    }

    Texture2D DuplicateTexture(Texture2D original)
    {
        Texture2D copy = new Texture2D(original.width, original.height, original.format, original.mipmapCount > 1);
        copy.SetPixels(original.GetPixels());
        copy.Apply();
        copy.name = original.name + "_copy";
        return copy;
    }

    string GetAssetExtension(Object obj)
    {
        if (obj is Mesh) return ".asset";
        if (obj is Material) return ".mat";
        if (obj is Texture2D) return ".asset";
        if (obj is ScriptableObject) return ".asset";
        if (obj is AudioClip) return ".asset";
        return ".asset";
    }
}
#endif
