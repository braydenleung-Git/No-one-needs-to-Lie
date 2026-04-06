#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Crime Scene: places Assets/Prefabs/Owner with dialogue + interaction (no patrol / vision).
public static class PlaceOwnerDialogueNpc
{
    const string PrefabPath = "Assets/Prefabs/Owner.prefab";

    [MenuItem("Tools/Crime Scene/Place Owner (dialogue NPC)")]
    static void PlaceOwner()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[PlaceOwner] Missing prefab at {PrefabPath}");
            return;
        }

        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.scene.isLoaded && go.transform.parent == null && go.name == "Owner")
            {
                Debug.Log("[PlaceOwner] An object named Owner already exists in this scene — delete it first or skip.");
                Selection.activeGameObject = go;
                return;
            }
        }

        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        inst.name = "Owner";
        inst.transform.SetPositionAndRotation(new Vector3(3f, 0f, 0f), Quaternion.identity);
        inst.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

        NpcPrefabUtility.ConfigureOwnerPrefabForDialogueNpc(inst);

        if (inst.GetComponent<NPCController>() == null)
        {
            var ctrl = inst.AddComponent<NPCController>();
            ctrl.npcName = "Old Man";
            ctrl.dialogueLines = new[]
            {
                "Oh... a visitor. Haven't seen one of those in a while.",
                "They say the old manor has a secret. I wouldn't go snooping if I were you.",
                "...But what do I know? I'm just an old man."
            };
        }

        Undo.RegisterCreatedObjectUndo(inst, "Place Owner (dialogue)");
        Selection.activeGameObject = inst;
        EditorSceneManager.MarkSceneDirty(inst.scene);
        Debug.Log("[PlaceOwner] Owner placed at (3, 0, 0). Save the scene (Ctrl+S).");
    }
}
#endif
