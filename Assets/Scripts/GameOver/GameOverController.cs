using System.Collections;
using UnityEngine;
using TMPro;

public class GameOverController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject detectivePrefab;
    [SerializeField] private GameObject partnerPrefab;
    [SerializeField] private GameObject gravestonePrefab;
    [SerializeField] private GameObject placeholderImagePrefab;
    [SerializeField] private Canvas uiCanvas;
    
    [Header("Game Over Title Settings")]
    [SerializeField] private GameObject titleImagePrefab;
    [SerializeField] private Vector2 titlePosition = Vector2.zero;
    [SerializeField] private Vector2 titleSize = new Vector2(400f, 100f);
    [SerializeField] private int titleSortingOrder = 4;
    
    [Header("Spawn Settings")]
    [SerializeField] private Vector3 detectiveSpawnPosition = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 partnerSpawnPosition = new Vector3(3f, 0f, 0f);
    [SerializeField] private Vector3 detectiveTargetPosition = new Vector3(1f, 0f, 0f);
    
    [Header("Timing")]
    [SerializeField] private float partnerSpawnDelay = 2f;
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float flashInterval = 0.3f;
    [SerializeField] private int flashCount = 3;
    [SerializeField] private float gravestoneTransformDelay = 1f;
    [SerializeField] private float gravestoneFadeDuration = 2f;
    [SerializeField] private float partnerFadeDuration = 2f;
    [SerializeField] private float gameOverTitleDelay = 1f;
    [SerializeField] private float gameOverTitleFadeIn = 1.5f;
    
    private GameObject detectiveInstance;
    private GameObject partnerInstance;
    private SpriteRenderer detectiveRenderer;
    private Color originalColor;
    
    void Start()
    {
        StartCoroutine(GameOverSequence());
    }
    
    private IEnumerator GameOverSequence()
    {
        // Spawn detective and start dialogue
        SpawnDetective();
        yield return new WaitForSeconds(1f);
        
        DialogueManager.Instance.StartDialogue("Detective", new string[] { "Finally, the case is over" });
        
        // Wait for dialogue to complete, then spawn partner
        yield return new WaitWhile(() => DialogueManager.Instance.IsDialogueActive());
        yield return new WaitForSeconds(partnerSpawnDelay);
        
        SpawnPartner();
        
        // Make partner walk to detective
        yield return StartCoroutine(MovePartnerToDetective());
        
        // Flash detective red 3 times
        yield return StartCoroutine(FlashDetectiveDamage());
        
        
        // Wait for dialogue to complete, then fade partner
        yield return new WaitWhile(() => DialogueManager.Instance.IsDialogueActive());
        yield return StartCoroutine(FadePartner());
        
        
        // Transform detective to gravestone
        yield return StartCoroutine(TransformToGravestone());
        // System dialogue
        DialogueManager.Instance.StartDialogue("System", new string[] { "Or is it ..." });
        
        
        
        
        
        // Show Game Over title and placeholder image
        yield return StartCoroutine(ShowGameOverTitle());
    }
    
    private void SpawnDetective()
    {
        if (detectivePrefab != null)
        {
            detectiveInstance = Instantiate(detectivePrefab, detectiveSpawnPosition, Quaternion.identity);
            detectiveRenderer = detectiveInstance.GetComponent<SpriteRenderer>();
            if (detectiveRenderer != null)
            {
                originalColor = detectiveRenderer.color;
            }
        }
        else
        {
            Debug.LogError("Detective prefab not assigned!");
        }
    }
    
    private void SpawnPartner()
    {
        if (partnerPrefab != null)
        {
            partnerInstance = Instantiate(partnerPrefab, partnerSpawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Partner prefab not assigned!");
        }
    }
    
    private IEnumerator MovePartnerToDetective()
    {
        if (partnerInstance == null || detectiveInstance == null) yield break;
        
        Vector3 startPosition = partnerInstance.transform.position;
        Vector3 targetPosition = detectiveTargetPosition;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float duration = distance / walkSpeed;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            partnerInstance.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        partnerInstance.transform.position = targetPosition;
    }
    
    private IEnumerator FlashDetectiveDamage()
    {
        if (detectiveRenderer == null) yield break;
        
        for (int i = 0; i < flashCount; i++)
        {
            // Flash to red
            detectiveRenderer.color = Color.red;
            yield return new WaitForSeconds(flashInterval);
            
            // Return to original color
            detectiveRenderer.color = originalColor;
            yield return new WaitForSeconds(flashInterval);
        }
    }
    
    private IEnumerator TransformToGravestone()
    {
        if (detectiveInstance == null) yield break;
        
        // Wait a moment before starting the transformation
        yield return new WaitForSeconds(gravestoneTransformDelay);
        
        // Fade out the detective
        if (detectiveRenderer != null)
        {
            float fadeElapsed = 0f;
            Color fadeColor = detectiveRenderer.color;
            
            while (fadeElapsed < gravestoneFadeDuration)
            {
                fadeElapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, fadeElapsed / gravestoneFadeDuration);
                fadeColor.a = alpha;
                detectiveRenderer.color = fadeColor;
                yield return null;
            }
        }
        
        // Spawn gravestone at detective's position
        // if (gravestonePrefab != null)
        // {
        //     Vector3 gravestonePosition = detectiveInstance.transform.position;
        //     gravestonePosition.y -= 10f; // Offset 10 units down
        //     GameObject gravestoneInstance = Instantiate(gravestonePrefab, gravestonePosition, Quaternion.identity);
        //     gravestoneInstance.transform.localScale = new Vector3(40f, 40f, 1f);
        //     
        //     // Optional: Add a subtle fade-in effect for the gravestone
        //     SpriteRenderer gravestoneRenderer = gravestoneInstance.GetComponent<SpriteRenderer>();
        //     if (gravestoneRenderer != null)
        //     {
        //         Color gravestoneColor = gravestoneRenderer.color;
        //         gravestoneColor.a = 0f;
        //         gravestoneRenderer.color = gravestoneColor;
        //         
        //         float fadeInElapsed = 0f;
        //         while (fadeInElapsed < 1f)
        //         {
        //             fadeInElapsed += Time.deltaTime;
        //             float alpha = Mathf.Lerp(0f, 1f, fadeInElapsed / 1f);
        //             gravestoneColor.a = alpha;
        //             gravestoneRenderer.color = gravestoneColor;
        //             yield return null;
        //         }
        //     }
        // }
        // else
        // {
        //     Debug.LogError("Gravestone prefab not assigned!");
        // }
        
        // Destroy the detective instance after transformation
        Destroy(detectiveInstance);
        detectiveInstance = null;
        detectiveRenderer = null;
    }
    
    private IEnumerator FadePartner()
    {
        if (partnerInstance == null) yield break;
        
        SpriteRenderer partnerRenderer = partnerInstance.GetComponent<SpriteRenderer>();
        if (partnerRenderer == null) yield break;
        
        // Fade out the partner
        float fadeElapsed = 0f;
        Color fadeColor = partnerRenderer.color;
        
        while (fadeElapsed < partnerFadeDuration)
        {
            fadeElapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeElapsed / partnerFadeDuration);
            fadeColor.a = alpha;
            partnerRenderer.color = fadeColor;
            yield return null;
        }
        
        // Destroy the partner instance after fading
        Destroy(partnerInstance);
        partnerInstance = null;
    }
    
    private IEnumerator ShowGameOverTitle()
    {
        // Wait a moment before showing the title
        yield return new WaitForSeconds(gameOverTitleDelay);
        
        // Spawn placeholder image
        if (placeholderImagePrefab != null)
        {
            GameObject placeholderInstance = Instantiate(placeholderImagePrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
            placeholderInstance.transform.localScale = new Vector3(10f, 10f, 1f);
            
            // Fade in placeholder image
            SpriteRenderer placeholderRenderer = placeholderInstance.GetComponent<SpriteRenderer>();
            if (placeholderRenderer != null)
            {
                Color placeholderColor = placeholderRenderer.color;
                placeholderColor.a = 0f;
                placeholderRenderer.color = placeholderColor;
                
                float fadeInElapsed = 0f;
                while (fadeInElapsed < gameOverTitleFadeIn)
                {
                    fadeInElapsed += Time.deltaTime;
                    float alpha = Mathf.Lerp(0f, 1f, fadeInElapsed / gameOverTitleFadeIn);
                    placeholderColor.a = alpha;
                    placeholderRenderer.color = placeholderColor;
                    yield return null;
                }
            }
        }
        else
        {
            Debug.LogError("Placeholder image prefab not assigned!");
        }
        
        // Create and show Game Over title using image prefab
        if (uiCanvas != null && titleImagePrefab != null)
        {
            // Instantiate the title image prefab
            GameObject titleGO = Instantiate(titleImagePrefab, uiCanvas.transform, false);
            titleGO.name = "GameOverTitle";
            
            // Set up RectTransform with custom position and size
            RectTransform titleRect = titleGO.GetComponent<RectTransform>();
            if (titleRect != null)
            {
                titleRect.anchorMin = new Vector2(0.5f, 0.5f);
                titleRect.anchorMax = new Vector2(0.5f, 0.5f);
                titleRect.sizeDelta = titleSize;
                titleRect.anchoredPosition = titlePosition;
            }
            
                        
            
            // Fade in title image
            SpriteRenderer titleRenderer = titleGO.GetComponent<SpriteRenderer>();
            if (titleRenderer != null)
            {
                Color fadeColor = titleRenderer.color;
                fadeColor.a = 0f;
                titleRenderer.color = fadeColor;
                titleRenderer.sortingOrder = titleSortingOrder;
                float fadeInElapsed = 0f;
                while (fadeInElapsed < gameOverTitleFadeIn)
                {
                    fadeInElapsed += Time.deltaTime;
                    float alpha = Mathf.Lerp(0f, 1f, fadeInElapsed / gameOverTitleFadeIn);
                    fadeColor.a = alpha;
                    titleRenderer.color = fadeColor;
                    yield return null;
                }
            }
            else
            {
                // Try Image component for UI
                UnityEngine.UI.Image titleImage = titleGO.GetComponent<UnityEngine.UI.Image>();
                if (titleImage != null)
                {
                    // Add Canvas for sorting order control
                    Canvas titleCanvas = titleGO.GetComponent<Canvas>();
                    if (titleCanvas == null)
                    {
                        titleCanvas = titleGO.AddComponent<Canvas>();
                        titleCanvas.overrideSorting = true;
                        titleCanvas.sortingOrder = titleSortingOrder;
                    }
                    
                    Color fadeColor = titleImage.color;
                    fadeColor.a = 0f;
                    titleImage.color = fadeColor;
                    
                    float fadeInElapsed = 0f;
                    while (fadeInElapsed < gameOverTitleFadeIn)
                    {
                        fadeInElapsed += Time.deltaTime;
                        float alpha = Mathf.Lerp(0f, 1f, fadeInElapsed / gameOverTitleFadeIn);
                        fadeColor.a = alpha;
                        titleImage.color = fadeColor;
                        yield return null;
                    }
                }
            }
        }
        else
        {
            if (uiCanvas == null)
                Debug.LogError("UI Canvas not assigned!");
            if (titleImagePrefab == null)
                Debug.LogError("Title image prefab not assigned!");
        }
    }
}
