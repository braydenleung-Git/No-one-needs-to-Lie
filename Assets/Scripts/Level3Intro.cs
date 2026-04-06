using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

public class Level3Intro : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject owner;

    [Header("Dark Overlay")]
    [SerializeField] private float lightFadeDuration = 2f;

    private Image darkOverlay;
    private PlayerInput cachedPlayerInput; // cached on Start

    private void Start()
    {
        if (GameState.Instance == null)
        {
            var go = new GameObject("GameStateManager");
            go.AddComponent<GameState>();
        }

        // cache player input reference immediately
        cachedPlayerInput = Object.FindFirstObjectByType<PlayerInput>();

        // hide owner
        if (owner != null) owner.SetActive(false);

        // if intro already seen, skip everything
        if (GameState.Instance.Level3IntroSeen) return;

        darkOverlay = CreateDarkOverlay();

        SetPlayerMovement(false);

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
        StartCoroutine(FadeLightsOn());
    }

    private IEnumerator FadeLightsOn()
    {
        if (darkOverlay == null)
        {
            SetPlayerMovement(true);
            yield break;
        }

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
        SetPlayerMovement(true);
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

    private void SetPlayerMovement(bool canMove)
    {
        if (cachedPlayerInput != null)
            cachedPlayerInput.enabled = canMove;
    }

    private void OnDestroy()
    {
        // use cached reference - still valid during scene unload
        SetPlayerMovement(true);
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (darkOverlay == null) return;
        Color c = darkOverlay.color;
        c.a = alpha;
        darkOverlay.color = c;
    }
}