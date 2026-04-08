using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persists the detective's selected appearance across scenes and re-applies it
/// to the current scene's detective whenever a scene loads.
/// </summary>
public sealed class DetectiveAppearanceManager : MonoBehaviour
{
    static DetectiveAppearanceManager _instance;

    DetectiveAccessory _selectedAccessory;

    public static void SelectAccessory(DetectiveAccessory accessory)
    {
        EnsureExists();
        _instance._selectedAccessory = accessory;
        _instance.ApplyToCurrentPlayer();
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToCurrentPlayer();
    }

    void ApplyToCurrentPlayer()
    {
        var player = FindDetectiveGameObject();
        if (player == null) return;

        // Remove hat if the player chose "no hat" or hasn't chosen yet.
        if (_selectedAccessory == null)
        {
            DetectiveHatAccessory.Instance.Remove(player);
            return;
        }

        _selectedAccessory.Apply(player);
    }

    static GameObject FindDetectiveGameObject()
    {
        // Prefer a PlayerController if present (scene doesn't consistently tag the detective as "Player").
#if UNITY_2023_1_OR_NEWER
        var pc = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Exclude);
#else
        var pc = Object.FindObjectOfType<PlayerController>();
#endif
        if (pc != null) return pc.gameObject;

        // Fallback: common tag.
        var tagged = GameObject.FindWithTag("Player");
        if (tagged != null) return tagged;

        return null;
    }

    static void EnsureExists()
    {
        if (_instance != null) return;

        var existing = FindFirstObjectByType<DetectiveAppearanceManager>(FindObjectsInactive.Include);
        if (existing != null)
        {
            if (!existing.gameObject.activeInHierarchy)
                existing.gameObject.SetActive(true);
            _instance = existing;
            return;
        }

        var go = new GameObject("DetectiveAppearanceManager");
        go.AddComponent<DetectiveAppearanceManager>();
    }
}

