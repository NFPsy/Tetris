using UnityEngine;
using UnityEngine.UI;

// 화면 좌측 상단 "NEXT" 패널에 다음에 나올 블록의 모양을 미리 보여주는 스크립트입니다.
// 최대 4칸짜리 UI Image를 미리 만들어두고, Show()가 호출될 때마다
// 필요한 개수만큼 켜서(enabled=true) 블록 모양대로 배치합니다.
public class NextPiecePreview : MonoBehaviour
{
    [SerializeField] private RectTransform blockContainer; // 미리보기 블록들이 들어갈 부모
    [SerializeField] private float cellSize = 24f;          // 미리보기에서 한 칸의 픽셀 크기

    private Image[] blockImages; // 재사용할 4개의 칸 이미지(모든 블록이 4칸이므로 4개면 충분)

    private void Awake()
    {
        // 4개의 빈 사각형 Image를 미리 만들어 둡니다. (Show()에서 필요한 만큼만 사용)
        blockImages = new Image[4];
        for (int i = 0; i < blockImages.Length; i++)
        {
            GameObject block = new GameObject("PreviewBlock");
            block.transform.SetParent(blockContainer, false);
            RectTransform rect = block.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(cellSize, cellSize);
            blockImages[i] = block.AddComponent<Image>();
            blockImages[i].sprite = SquareSprite.Get(); // 실제 블록과 같은 테두리 스프라이트 사용
        }
    }

    // type 블록의 모양과 색깔로 미리보기를 갱신합니다.
    public void Show(TetrominoType type)
    {
        Vector2Int[] cells = TetrominoShapes.GetCells(type);
        Color color = TetrominoShapes.GetColor(type);

        // 블록 모양의 가로/세로 범위(바운딩 박스)를 구해서, 미리보기 칸 중앙에 오도록 계산합니다.
        int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
        foreach (Vector2Int cell in cells)
        {
            minX = Mathf.Min(minX, cell.x);
            maxX = Mathf.Max(maxX, cell.x);
            minY = Mathf.Min(minY, cell.y);
            maxY = Mathf.Max(maxY, cell.y);
        }

        float width = (maxX - minX + 1) * cellSize;
        float height = (maxY - minY + 1) * cellSize;

        for (int i = 0; i < blockImages.Length; i++)
        {
            if (i < cells.Length)
            {
                blockImages[i].enabled = true;
                blockImages[i].color = color;

                // 블록의 상대 좌표를 바운딩 박스 중앙 기준 픽셀 좌표로 변환
                float x = (cells[i].x - minX) * cellSize - width / 2f + cellSize / 2f;
                float y = (cells[i].y - minY) * cellSize - height / 2f + cellSize / 2f;
                blockImages[i].rectTransform.anchoredPosition = new Vector2(x, y);
            }
            else
            {
                // 이번 블록에서 사용하지 않는 칸은 꺼둡니다.
                blockImages[i].enabled = false;
            }
        }
    }
}
