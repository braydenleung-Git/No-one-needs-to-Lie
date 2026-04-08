using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class Level3Intro : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject owner;

    [Header("Dark Overlay")]
    [SerializeField] private float lightFadeDuration = 2f;

    [Header("Owner Tracking")]
    [SerializeField] private GameObject cmCamPlayer;
    [SerializeField] private GameObject cmCamOwner;
    private GameObject visionCone;

    private Image darkOverlay;

    private void Start()
    {
        if (GameState.Instance == null)
        {
            var go = new GameObject("GameStateManager");
            go.AddComponent<GameState>();
        }

        if (owner != null) owner.SetActive(false);
        if (cmCamOwner != null) cmCamOwner.SetActive(false);

        visionCone = owner.transform.Find("VisionCone")?.gameObject;
        if (visionCone != null) visionCone.SetActive(false);

        if (GameState.Instance.Level3IntroSeen) return;

        darkOverlay = CreateDarkOverlay();

        DialogueManager.Instance?.StartDialogue(
            "",
            new[] {
                "John has left the house. Time to snoop around and find clues before he comes back...",
                "Let me turn on the lights first."
            },
            onComplete: OnDialogueDone
        );
    }

    private void OnDialogueDone()
    {
        GameState.Instance.Level3IntroSeen = true;
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        yield return StartCoroutine(FadeLightsOn());

        yield return new WaitForSeconds(3f);

        if (owner != null) owner.SetActive(true);
        if (visionCone != null) visionCone.SetActive(false);

        if (cmCamPlayer != null) cmCamPlayer.SetActive(false);
        if (cmCamOwner != null) cmCamOwner.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        DialogueManager.Instance?.StartDialogue(
            "John",
            new[] {
                "Huh... I turned off the light and went out.",
                "Looks like someone broke into my house...",
                "I'll kill whoever did this!"
            },
            onComplete: OnOwnerDialogueDone
        );
    }

    private void OnOwnerDialogueDone()
    {
        if (cmCamOwner != null) cmCamOwner.SetActive(false);
        if (cmCamPlayer != null) cmCamPlayer.SetActive(true);
        if (visionCone != null) visionCone.SetActive(true);

        if (owner != null)
        {
            var patrol = owner.GetComponent<Owner_Patrol>();
            if (patrol != null) patrol.enabled = true;
        }
    }

    private IEnumerator FadeLightsOn()
    {
        if (darkOverlay == null) yield break;

        float elapsed = 0f;
        float startAlpha = darkOverlay.color.a;

        while (elapsed < lightFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsed / lightFadeDuration);
            SetOverlayAlpha(alpha);
            yield return null;
        }

        Destroy(darkOverlay.gameObject);
    }

    private Image CreateDarkOverlay()
    {
        var canvasGO = new GameObject("DarkOverlay");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9;

        canvasGO.AddComponent<CanvasScaler>();

        var panelGO = new GameObject("OverlayPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);

        var rt = panelGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = panelGO.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.82f);

        SceneManager.MoveGameObjectToScene(canvasGO, gameObject.scene);

        return img;
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (darkOverlay == null) return;
        Color c = darkOverlay.color;
        c.a = alpha;
        darkOverlay.color = c;
    }
}