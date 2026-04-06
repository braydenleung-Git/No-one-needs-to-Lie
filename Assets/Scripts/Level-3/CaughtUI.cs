using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CaughtUI : MonoBehaviour
{
    public static CaughtUI Instance { get; private set; }

    [SerializeField] private GameObject caughtPanel;
    [SerializeField] private Button retryButton;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        caughtPanel.SetActive(false);
        retryButton?.onClick.AddListener(OnRetry);
    }

    public void Show()
    {
        caughtPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void OnRetry()
    {
        GameState.Instance.ResetLevel3();
        PlayerSpawnManager.ReturnFromLevel = 3;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Town");
    }
}