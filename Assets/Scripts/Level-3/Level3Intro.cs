using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class Level3Intro : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject owner;
    [SerializeField] private PlayerController player;

    [Header("UI")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image darkOverlay;          // black panel, Alpha ~0.85

    [Header("Settings")]
    [SerializeField] private float lightFadeDuration = 2f;

    private enum IntroState { WaitingForFirstE, WaitingForSecondE, Done }
    private IntroState state = IntroState.WaitingForFirstE;

    private readonly string[] lines = new string[]
    {
        "John has left the house.\nTime to snoop around and find clues before he comes back...\n\n[Press E to continue]",
        "Let me turn on the lights first.\n[Press E]"
    };

    private void Start()
    {
        // only run intro if first time entering level 3
        if (GameState.Instance != null && GameState.Instance.Level3IntroSeen)
        {
            // skip intro entirely - just hide owner and let player move
            if (owner != null) owner.SetActive(false);
            if (darkOverlay != null) darkOverlay.gameObject.SetActive(false);
            if (dialogueBox != null) dialogueBox.SetActive(false);
            return;
        }

        // hide owner
        if (owner != null) owner.SetActive(false);

        // lock player
        if (player != null) player.canMove = false;

        // darken screen
        if (darkOverlay != null)
        {
            darkOverlay.gameObject.SetActive(true);
            SetOverlayAlpha(0.85f);
        }

        // show first dialogue line
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
            dialogueText.text = lines[0];
        }
    }

    private void Update()
    {
        if (state == IntroState.Done) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (state == IntroState.WaitingForFirstE)
            {
                // show second line
                dialogueText.text = lines[1];
                state = IntroState.WaitingForSecondE;
            }
            else if (state == IntroState.WaitingForSecondE)
            {
                // close dialogue, fade lights on, unlock player
                state = IntroState.Done;
                dialogueBox.SetActive(false);
                StartCoroutine(FadeLightsOn());

                // mark intro as seen so it never plays again
                if (GameState.Instance != null)
                    GameState.Instance.Level3IntroSeen = true;
            }
        }
    }

    private IEnumerator FadeLightsOn()
    {
        float elapsed = 0f;
        float startAlpha = darkOverlay.color.a;

        while (elapsed < lightFadeDuration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 0f, elapsed / lightFadeDuration);
            SetOverlayAlpha(newAlpha);
            yield return null;
        }

        SetOverlayAlpha(0f);
        darkOverlay.gameObject.SetActive(false);

        // give player control
        if (player != null) player.canMove = true;
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (darkOverlay == null) return;
        Color c = darkOverlay.color;
        c.a = alpha;
        darkOverlay.color = c;
    }
}