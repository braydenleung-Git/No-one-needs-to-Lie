using UnityEngine;

public class HigurumaNPC : Interactable
{
    [Header("Court Session")]
    [SerializeField] private AudioClip eliteFourTheme;

    private AudioSource audioSource;
    private bool sessionStarted = false;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.clip = eliteFourTheme;
    }

    public override void Interact()
    {
        if (sessionStarted) return;

        DialogueManager.Instance.StartDialogue(
            "Higuruma",
            new string[] { "...", "Court is now in session.", "Begin." },
            OnSessionBegin
        );
    }

    private void OnSessionBegin()
    {
        sessionStarted = true;
        if (eliteFourTheme != null) audioSource.Play();
        CourtEvidenceUI.Instance.OpenPanel();
    }
}