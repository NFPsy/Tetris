using UnityEngine;
using TMPro;

// 점수와 레벨을 계산하고 화면에 표시하는 스크립트입니다.
public class ScoreManager : MonoBehaviour
{
    private const int LinesPerLevel = 10; // 이 줄 수만큼 지울 때마다 레벨이 1 오름
    private const string HighScoreKey = "TetrisHighScore"; // PlayerPrefs에 최고점수를 저장할 때 쓰는 이름표

    // NES(고전) 테트리스 점수 테이블입니다. 인덱스가 한 번에 지운 줄 수를 의미합니다.
    // 예: 한 번에 4줄(테트리스)을 지우면 1줄씩 4번 지우는 것보다 훨씬 많은 점수를 줍니다.
    // [0줄, 1줄=싱글, 2줄=더블, 3줄=트리플, 4줄=테트리스]
    private static readonly int[] ClassicLineScores = { 0, 40, 100, 300, 1200 };

    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelText;

    public int Score { get; private set; }
    public int Level { get; private set; } = 1; // 1레벨부터 시작
    public int HighScore { get; private set; } // 지금까지 기록된 최고 점수

    private int totalLinesCleared; // 지금까지 지운 줄의 총합(레벨 계산용)

    private void Awake()
    {
        // PlayerPrefs는 게임을 껐다 켜도, 씬을 재시작해도 값이 그대로 남아있는 저장소입니다.
        // 저장된 최고점수가 없으면 0을 기본값으로 사용합니다.
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        UpdateDisplay();
    }

    // 한 번에 lineCount줄을 지웠을 때 호출합니다. 점수를 올리고 레벨을 갱신합니다.
    public void AddLines(int lineCount)
    {
        // 혹시 4줄보다 많이 지워지는 경우를 대비해 테이블 범위를 벗어나지 않게 clamp
        int tableIndex = Mathf.Clamp(lineCount, 0, ClassicLineScores.Length - 1);

        // 기본 점수에 현재 레벨을 곱해서, 레벨이 높을수록 더 큰 점수를 받게 합니다.
        Score += ClassicLineScores[tableIndex] * Level;

        totalLinesCleared += lineCount;
        Level = 1 + totalLinesCleared / LinesPerLevel; // 10줄마다 레벨 +1

        // 현재 점수가 최고점수를 넘었으면 즉시 갱신하고 저장합니다.
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
        }

        UpdateDisplay();
    }

    // 최고점수/점수/레벨 텍스트를 화면에 갱신합니다.
    private void UpdateDisplay()
    {
        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + HighScore;
        }
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
