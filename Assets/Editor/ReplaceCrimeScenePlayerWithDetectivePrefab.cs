using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ReplaceCrimeScenePlayerWithDetectivePrefab
{
    const string CrimeSceneName = "Crime Scene";
    const string DetectivePrefabPath = "Assets/Prefabs/Detective.prefab";

    [MenuItem("Tools/Crime Scene/Replace Player With Detective Prefab")]
    public static void Replace()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.name != CrimeSceneName)
        {
            EditorUtility.DisplayDialog(
                "Wrong scene",
                $"Open the '{CrimeSceneName}' scene first, then run this again.",
                "OK");
            return;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(DetectivePrefabPath);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog(
                "Missing prefab",
                $"Could not find '{DetectivePrefabPath}'.",
                "OK");
            return;
        }

        // Find existing player object in the scene (tagged Player).
        var existing = GameObject.FindGameObjectWithTag("Player");

        // Instantiate prefab into the active scene.
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        inst.name = "Detective";

        if (existing != null)
        {
            inst.transform.position = existing.transform.position;
            inst.transform.rotation = existing.transform.rotation;
            inst.transform.localScale = existing.transform.localScale;

            // Delete the old one.
            Object.DestroyImmediate(existing);
        }
        else
        {
            // Reasonable default spawn if none existed.
            inst.transform.position = new Vector3(-1.5f, -0.5f, 0f);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorUtility.DisplayDialog("Done", "Replaced scene Player with Detective prefab instance.", "OK");
    }
}

