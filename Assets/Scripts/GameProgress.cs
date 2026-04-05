using UnityEngine;

// Persists across scenes and editor play sessions (PlayerPrefs)
public static class GameProgress
{
    const string KeyUv = "GameProgress_HasUvFlashlight";

    public static bool HasUvFlashlight => PlayerPrefs.GetInt(KeyUv, 0) == 1;

    public static void SetUvFlashlightOwned()
    {
        PlayerPrefs.SetInt(KeyUv, 1);
        PlayerPrefs.Save();
    }

    public static void ClearUvFlashlightForTesting()
    {
        PlayerPrefs.DeleteKey(KeyUv);
        PlayerPrefs.Save();
    }
}
