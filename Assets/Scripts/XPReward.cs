using UnityEngine;

public class XPReward : MonoBehaviour, IXPRewardable
{
    public enum RewardType { PuzzleSolved, LevelCleared, InventoryFilled, Custom }

    [Header("Reward Config")]
    [SerializeField] private int xpValue = 50;
    [SerializeField] private RewardType rewardType;

    private bool hasBeenRewarded = false;

    public int XPValue => xpValue;
    public bool HasBeenRewarded => hasBeenRewarded;

    public void AwardXP()
    {
        if (hasBeenRewarded) return;
        hasBeenRewarded = true;
        ExperienceManager.Instance.AddXP(xpValue);
        Debug.Log($"[Reward] {rewardType} — awarded {xpValue} XP.");
    }
}