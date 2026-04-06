public interface ILevelLocked
{
    int RequiredLevel { get; }
    bool IsUnlocked();
    bool TryAccess();
}