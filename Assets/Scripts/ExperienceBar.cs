using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExperienceBar : MonoBehaviour
{
    [Header("UI References")]
    public Slider xpSlider;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;

    private void Start()
    {
        ExperienceManager.Instance.OnXPChanged.AddListener(UpdateBar);
        ExperienceManager.Instance.OnLevelUp.AddListener(OnLevelUp);
        RefreshUI();
    }

    private void UpdateBar(int newXP)
    {
        xpSlider.value = ExperienceManager.Instance.GetXPProgress();
        xpText.text = $"{ExperienceManager.Instance.currentXP} / {ExperienceManager.Instance.xpToNextLevel} XP";
    }

    private void OnLevelUp(int newLevel)
    {
        levelText.text = $"Level {newLevel}";
        xpSlider.value = ExperienceManager.Instance.GetXPProgress();
    }

    private void RefreshUI()
    {
        levelText.text = $"Level {ExperienceManager.Instance.currentLevel}";
        xpSlider.value = ExperienceManager.Instance.GetXPProgress();
        xpText.text = $"{ExperienceManager.Instance.currentXP} / {ExperienceManager.Instance.xpToNextLevel} XP";
    }
}