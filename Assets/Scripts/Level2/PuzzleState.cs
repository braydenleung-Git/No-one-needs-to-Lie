// tracks what the player has discovered in the crime scene level
// static so any script can check puzzle progress without needing references
public static class PuzzleState
{
    /// <summary>Four cipher paintings give letters at positions 1–4 (J,O,H,N) → enter this on the art-room code painting.</summary>
    public const string ArtRoomCodeSolution = "JOHN";

    public static bool HasCassetteTape  { get; set; }
    public static bool CassettePlayerUsed { get; set; }
    /// <summary>True after JOHN is entered correctly on the art-room code painting (or legacy safe).</summary>
    public static bool ArtRoomCodeSolved { get; set; }

    // call this if the scene gets reloaded so state doesn't bleed between playtests
    public static void Reset()
    {
        HasCassetteTape      = false;
        CassettePlayerUsed   = false;
        ArtRoomCodeSolved    = false;
    }
}
