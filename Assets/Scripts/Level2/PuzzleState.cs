// tracks what the player has discovered in the crime scene level
// static so any script can check puzzle progress without needing references
public static class PuzzleState
{
    /// <summary>Four cipher paintings give letters at positions 1–4 (L,U,C,Y) → enter this at the art room safe.</summary>
    public const string ArtRoomCodeSolution = "LUCY";

    public static bool HasCassetteTape  { get; set; }
    public static bool CassettePlayerUsed { get; set; }
    public static bool ArtRoomSafeOpened { get; set; }

    // call this if the scene gets reloaded so state doesn't bleed between playtests
    public static void Reset()
    {
        HasCassetteTape      = false;
        CassettePlayerUsed   = false;
        ArtRoomSafeOpened    = false;
    }
}
