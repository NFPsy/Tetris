using UnityEngine;

// 화면 크기/비율이 바뀌어도 보드(Board.Width x Board.Height) 전체가 항상 화면 안에 들어오도록
// 카메라의 orthographicSize를 매 프레임 자동으로 계산해주는 스크립트입니다.
// Main Camera에 붙여서 사용합니다.
[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    [SerializeField] private float padding = 1f; // 보드 테두리 바깥쪽 여유 공간(단위: 칸 수)

    private Camera targetCamera;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
    }

    // 화면 크기가 바뀔 수 있으므로(창 크기 조절, 다른 해상도로 빌드 등) 매 프레임 다시 계산합니다.
    private void LateUpdate()
    {
        float targetHeight = Board.Height + padding * 2f;
        float targetWidth = Board.Width + padding * 2f;

        // 세로 기준으로 필요한 크기와 가로 기준으로 필요한 크기 중 더 큰 쪽을 사용해야
        // 화면 비율과 관계없이 보드가 절대 잘리지 않습니다.
        float verticalSize = targetHeight / 2f;
        float horizontalSize = (targetWidth / 2f) / targetCamera.aspect;

        targetCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }
}
