using UnityEngine;
using TMPro;

// 게임 오버 결과 화면을 담당하는 스크립트입니다.
// GameManager의 OnGameOver 이벤트를 구독하고 있다가, 게임이 끝나면
// 결과 패널을 화면에 띄우고 최종 점수를 표시합니다.
public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject panel;              // 게임오버 패널(평소엔 꺼져있음)
    [SerializeField] private TextMeshProUGUI finalScoreText; // "Final Score: N" 표시용 텍스트

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
        // 시작할 때는 결과 패널이 보이지 않도록 꺼둡니다.
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    // 이 오브젝트가 활성화될 때 게임오버 이벤트를 구독합니다.
    private void OnEnable()
    {
        if (gameManager != null)
        {
            gameManager.OnGameOver += HandleGameOver;
        }
    }

    // 비활성화될 때는 반드시 구독을 해제해서, 메모리 누수나 중복 호출을 막습니다.
    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.OnGameOver -= HandleGameOver;
        }
    }

    // 게임오버 이벤트가 발생하면 호출됩니다. finalScore는 게임이 끝난 시점의 최종 점수입니다.
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
