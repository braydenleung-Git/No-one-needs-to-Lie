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

    private void Start()
    {
        ExperienceManager.Instance.OnXPChanged.AddListener(UpdateBar);
        ExperienceManager.Instance.OnLevelUp.AddListener(OnLevelUp);
        RefreshUI();
    }

    // Fires whenever XP changes (including during a level-up sequence)
    private void UpdateBar(int newXP)
    {
        fillImage.fillAmount = ExperienceManager.Instance.GetXPProgress();
        RefreshXPText();
    }

    // Fires after each level-up
    private void OnLevelUp(int newLevel)
    {
        RefreshLevelText(newLevel);
        fillImage.fillAmount = ExperienceManager.Instance.GetXPProgress();
        RefreshXPText();
    }

    // Called once on Start to sync UI with whatever state was saved/loaded
    private void RefreshUI()
    {
        int level = ExperienceManager.Instance.currentLevel;
        RefreshLevelText(level);
        fillImage.fillAmount = ExperienceManager.Instance.GetXPProgress();
        RefreshXPText();
    }

    private void RefreshLevelText(int level)
    {
        levelText.text = level >= ExperienceManager.MAX_LEVEL ? "MAX" : $"LVL {level}";
    }

    private void RefreshXPText()
    {
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