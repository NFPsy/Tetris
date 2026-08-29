using UnityEngine;

// 일시정지 화면을 담당하는 스크립트입니다.
// GameManager의 OnPauseChanged 이벤트를 구독하고 있다가, 일시정지 상태에 맞춰
// 화면 중앙의 "PAUSE" 문구를 보여주거나 숨깁니다.
public class PauseUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject pauseText; // 화면 중앙 "PAUSE" 텍스트(평소엔 꺼져있음)

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
        if (pauseText != null)
        {
            pauseText.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (gameManager != null)
        {
            gameManager.OnPauseChanged += HandlePauseChanged;
        }
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.OnPauseChanged -= HandlePauseChanged;
        }
    }

    private void HandlePauseChanged(bool paused)
    {
        if (pauseText != null)
        {
            pauseText.SetActive(paused);
        }
    }
}
