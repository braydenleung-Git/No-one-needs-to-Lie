using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CourtEvidenceUI : MonoBehaviour
{
    public static CourtEvidenceUI Instance;

    [Header("UI References")]
    [SerializeField] private GameObject evidencePanel;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private TextMeshProUGUI judgeResponseText;
    [SerializeField] private Button closeButton;

    [Header("Player")]
    [SerializeField] private PlayerInventory playerInventory;

    // add more responses here matching your GameItem ItemNames exactly
    private Dictionary<string, string> judgeResponses = new Dictionary<string, string>()
    {
        { "Alibi Photograph", "That timestamp is three hours off. Anyone could have edited it. OBJECTION!" },
        { "Witness Statement", "Your witness never saw the defendant directly. That is hearsay!" },
        { "Medical Report", "These injuries are inconsistent with your account. Explain that." },
        { "Security Footage", "There is a 40-minute gap in this footage. What happened in that window?" },
    };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        evidencePanel.SetActive(false);
        closeButton.onClick.AddListener(() => evidencePanel.SetActive(false));
    }

    public void OpenPanel()
    {
        // clear any old buttons from last time
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        List<GameItem> items = playerInventory.ListItems();

        if (items.Count == 0)
        {
            judgeResponseText.text = "You have no evidence to present.";
        }
        else
        {
            judgeResponseText.text = "Choose your evidence, counselor.";
            foreach (GameItem item in items)
            {
                Button btn = Instantiate(buttonPrefab, buttonContainer);
                btn.GetComponentInChildren<TextMeshProUGUI>().text = item.ItemName;
                GameItem captured = item;
                btn.onClick.AddListener(() => PresentEvidence(captured));
            }
        }

        evidencePanel.SetActive(true);
    }

    private void PresentEvidence(GameItem item)
    {
        if (judgeResponses.TryGetValue(item.ItemName, out string response))
            judgeResponseText.text = "Higuruma: \"" + response + "\"";
        else
            judgeResponseText.text = "Higuruma: \"Hmm... I will need to consider this evidence.\"";
    }
}