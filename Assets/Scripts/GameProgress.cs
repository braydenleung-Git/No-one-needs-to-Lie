// Tracks progress across scenes within one play session.
// Intentionally NOT backed by PlayerPrefs — resets cleanly each time you press Play
// so playtesting never carries over stale state from a previous run.
public static class GameProgress
{
    static bool _hasUvFlashlight;

    public static bool HasUvFlashlight => _hasUvFlashlight;

    public static void SetUvFlashlightOwned()
    {
        _hasUvFlashlight = true;
    }

    // resets all progress — called at scene load so each playtest starts clean
    public static void Reset()
    {
        _hasUvFlashlight = false;
    }

    // kept for backwards compat — same as Reset() now that PlayerPrefs is gone
    public static void ClearUvFlashlightForTesting() => Reset();
}
