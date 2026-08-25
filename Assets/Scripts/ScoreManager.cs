using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    public int Score { get; private set; }

    private void Awake()
    {
        UpdateDisplay();
    }

    public void AddLines(int lineCount)
    {
        Score += lineCount * 100;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + Score;
        }
    }
}
