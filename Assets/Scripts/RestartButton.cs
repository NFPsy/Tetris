using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 결과 화면의 "Restart" 버튼에 붙는 스크립트입니다.
// 버튼을 클릭하면 현재 씬을 처음부터 다시 불러와서(재시작) 게임을 초기화합니다.
public class RestartButton : MonoBehaviour
{
    private void Awake()
    {
        // 같은 오브젝트에 있는 Button 컴포넌트를 찾아서,
        // 클릭 이벤트(onClick)에 Restart() 함수를 연결합니다.
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(Restart);
        }
    }

    // 지금 실행 중인 씬을 다시 불러옵니다.
    // 씬을 새로 불러오면 모든 오브젝트(보드, 점수, 블록 등)가 처음 상태로 초기화됩니다.
    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
