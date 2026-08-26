using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private const int LinesPerLevel = 10;
    private static readonly int[] ClassicLineScores = { 0, 40, 100, 300, 1200 };

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelText;

    public int Score { get; private set; }
    public int Level { get; private set; } = 1;

    private int totalLinesCleared;

    private void Awake()
    {
        UpdateDisplay();
    }

    public void AddLines(int lineCount)
    {
        int tableIndex = Mathf.Clamp(lineCount, 0, ClassicLineScores.Length - 1);
        Score += ClassicLineScores[tableIndex] * Level;
        totalLinesCleared += lineCount;
        Level = 1 + totalLinesCleared / LinesPerLevel;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + Score;
        }
        if (levelText != null)
        {
            levelText.text = "Level: " + Level;
        }
    }
}
