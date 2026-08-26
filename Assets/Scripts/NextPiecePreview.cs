using UnityEngine;
using UnityEngine.UI;

public class NextPiecePreview : MonoBehaviour
{
    [SerializeField] private RectTransform blockContainer;
    [SerializeField] private float cellSize = 24f;

    private Image[] blockImages;

    private void Awake()
    {
        blockImages = new Image[4];
        for (int i = 0; i < blockImages.Length; i++)
        {
            GameObject block = new GameObject("PreviewBlock");
            block.transform.SetParent(blockContainer, false);
            RectTransform rect = block.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(cellSize, cellSize);
            blockImages[i] = block.AddComponent<Image>();
            blockImages[i].sprite = SquareSprite.Get();
        }
    }

    public void Show(TetrominoType type)
    {
        Vector2Int[] cells = TetrominoShapes.GetCells(type);
        Color color = TetrominoShapes.GetColor(type);

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
                float x = (cells[i].x - minX) * cellSize - width / 2f + cellSize / 2f;
                float y = (cells[i].y - minY) * cellSize - height / 2f + cellSize / 2f;
                blockImages[i].rectTransform.anchoredPosition = new Vector2(x, y);
            }
            else
            {
                blockImages[i].enabled = false;
            }
        }
    }
}
