public interface IXPRewardable
{
    int XPValue { get; }
    bool HasBeenRewarded { get; }
    void AwardXP();
}