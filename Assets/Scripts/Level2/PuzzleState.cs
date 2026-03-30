// tracks what the player has discovered in the crime scene level
// static so any script can check puzzle progress without needing references
public static class PuzzleState
{
    public static bool HasCassetteTape  { get; set; }
    public static bool CassettePlayerUsed { get; set; }

    // call this if the scene gets reloaded so state doesn't bleed between playtests
    public static void Reset()
    {
        HasCassetteTape     = false;
        CassettePlayerUsed  = false;
    }
}
