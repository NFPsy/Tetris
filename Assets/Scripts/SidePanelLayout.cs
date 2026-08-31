using UnityEngine;

// HOLD 패널(왼쪽)과 NEXT 패널(오른쪽)을 보드의 실제 좌우 가장자리에 딱 붙여주는 스크립트입니다.
// CameraFit이 화면 비율에 맞춰 카메라를 조정하면, 보드가 화면에서 차지하는 위치/크기도 바뀌기 때문에
// 이 스크립트가 매 프레임 보드의 화면상 좌우 위치를 다시 계산해서 두 패널을 그 위치로 옮겨줍니다.
public class SidePanelLayout : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Board board;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private RectTransform holdPanel; // 왼쪽에 붙일 패널
    [SerializeField] private RectTransform nextPanel;  // 오른쪽에 붙일 패널
    [SerializeField] private float gap = 20f;          // 보드와 패널 사이 간격

    private void LateUpdate()
    {
        if (targetCamera == null || board == null || canvasRect == null)
        {
            return;
        }

        // 보드 좌/우 가장자리의 월드 좌표 (칸 중심 기준이라 0.5칸씩 밖으로 확장)
        float boardLeftWorldX = board.transform.position.x - 0.5f;
        float boardRightWorldX = board.transform.position.x + Board.Width - 0.5f;
        float midWorldY = board.transform.position.y + Board.Height / 2f;

        float boardLeftCanvasX = WorldXToCanvasX(boardLeftWorldX, midWorldY);
        float boardRightCanvasX = WorldXToCanvasX(boardRightWorldX, midWorldY);

        if (holdPanel != null)
        {
            Vector2 pos = holdPanel.anchoredPosition;
            pos.x = boardLeftCanvasX - gap - holdPanel.sizeDelta.x;
            holdPanel.anchoredPosition = pos;
        }

        if (nextPanel != null)
        {
            Vector2 pos = nextPanel.anchoredPosition;
            pos.x = boardRightCanvasX + gap;
            nextPanel.anchoredPosition = pos;
        }
    }

    // 특정 월드 좌표(x, y)가 화면상 어디에 보이는지를, 캔버스의 좌상단을 기준(0)으로 한 x값으로 변환합니다.
    // (HighScoreText 등 다른 UI들이 anchorMin/Max=(0,1), pivot=(0,1)로 좌상단 기준 좌표를 쓰기 때문에 맞춰줍니다.)
    private float WorldXToCanvasX(float worldX, float worldY)
    {
        Vector3 worldPoint = new Vector3(worldX, worldY, 0f);
        Vector2 screenPoint = targetCamera.WorldToScreenPoint(worldPoint);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 localPoint);

        // localPoint는 캔버스 중심 기준이므로, 캔버스 폭의 절반을 더해 좌상단 기준으로 바꿔줍니다.
        return localPoint.x + canvasRect.rect.width / 2f;
    }
}
