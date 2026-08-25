using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (gameManager != null)
        {
            gameManager.OnGameOver += HandleGameOver;
        }
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.OnGameOver -= HandleGameOver;
        }
    }

    private void HandleGameOver(int finalScore)
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + finalScore;
        }
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }
}
