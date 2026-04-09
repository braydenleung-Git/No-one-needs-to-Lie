using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExperienceBar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The green fill Image — Image Type must be set to Filled / Horizontal.")]
    public Image fillImage;

    [Tooltip("Shows 'LVL 1' through 'LVL 5' or 'MAX'.")]
    public TextMeshProUGUI levelText;

    [Tooltip("Shows '50 / 100 XP' or 'MAX LEVEL' when capped.")]
    public TextMeshProUGUI xpText;

    private void Awake()
    {
        // Filled images default to fillAmount 1 — sync immediately so the bar doesn't flash full.
        if (fillImage != null && ExperienceManager.Instance != null)
            fillImage.fillAmount = ExperienceManager.Instance.GetXPProgress();
    }

    private void Start()
    {
        var xp = ExperienceManager.Instance;
        if (xp == null || fillImage == null || levelText == null || xpText == null)
        {
            Debug.LogWarning("ExperienceBar: Missing ExperienceManager or UI references — bar will not update.");
            return;
        }

        if (xp.OnXPChanged != null) xp.OnXPChanged.AddListener(UpdateBar);
        if (xp.OnLevelUp != null) xp.OnLevelUp.AddListener(OnLevelUp);
        RefreshUI();
    }

    // Fires whenever XP changes (including during a level-up sequence)
    private void UpdateBar(int newXP)
    {
        if (fillImage == null || ExperienceManager.Instance == null) return;
        fillImage.fillAmount = ExperienceManager.Instance.GetXPProgress();
        RefreshXPText();
    }

    // Fires after each level-up
    private void OnLevelUp(int newLevel)
    {
        if (fillImage == null || ExperienceManager.Instance == null) return;
        RefreshLevelText(newLevel);
        fillImage.fillAmount = ExperienceManager.Instance.GetXPProgress();
        RefreshXPText();
    }

    // Called once on Start to sync UI with whatever state was saved/loaded
    private void RefreshUI()
    {
        if (fillImage == null || ExperienceManager.Instance == null) return;
        int level = ExperienceManager.Instance.currentLevel;
        RefreshLevelText(level);
        fillImage.fillAmount = ExperienceManager.Instance.GetXPProgress();
        RefreshXPText();
    }

    private void RefreshLevelText(int level)
    {
        if (levelText == null) return;
        levelText.text = level >= ExperienceManager.MAX_LEVEL ? "MAX" : $"LVL {level}";
    }

    private void RefreshXPText()
    {
        if (xpText == null || ExperienceManager.Instance == null) return;
        if (ExperienceManager.Instance.IsMaxLevel())
        {
            xpText.text = "MAX LEVEL";
            return;
        }

        int current = ExperienceManager.Instance.currentXP;
        int needed  = ExperienceManager.Instance.xpToNextLevel;
        xpText.text = $"{current} / {needed} XP";
    }
}