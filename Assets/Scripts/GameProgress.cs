// Tracks progress across scenes within one play session.
// Intentionally NOT backed by PlayerPrefs — resets cleanly each time you press Play
// so playtesting never carries over stale state from a previous run.
public static class GameProgress
{
    static bool _hasUvFlashlight;
    static bool _wantsHat;

    public static bool HasUvFlashlight => _hasUvFlashlight;
    public static bool WantsHat => _wantsHat;

    public static void SetUvFlashlightOwned()
    {
        _hasUvFlashlight = true;
    }

    public static void SetWantsHat(bool value)
    {
        _wantsHat = value;
    }

    // resets all progress — called at scene load so each playtest starts clean
    public static void Reset()
    {
        _hasUvFlashlight = false;
        _wantsHat = false;
    }

    // kept for backwards compat — same as Reset() now that PlayerPrefs is gone
    public static void ClearUvFlashlightForTesting() => Reset();
}
