using System.Collections;
using UnityEngine;
using TMPro;

public class XPNotification : MonoBehaviour
{
    public static XPNotification Instance;

    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float displayTime = 2f;

    private Coroutine current;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        notificationText.alpha = 0;
    }

    public void Show(string message)
    {
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(DisplayMessage(message));
    }

    private IEnumerator DisplayMessage(string message)
    {
        notificationText.text = message;
        notificationText.alpha = 1;
        yield return new WaitForSeconds(displayTime);
        
        float t = 0.5f;
        while (t > 0)
        {
            t -= Time.deltaTime;
            notificationText.alpha = t / 0.5f;
            yield return null;
        }

        notificationText.alpha = 0;
    }
}