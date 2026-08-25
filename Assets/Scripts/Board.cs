using UnityEngine;

public class Board : MonoBehaviour
{
    public const int Width = 10;
    public const int Height = 20;

    private bool[,] cells = new bool[Width, Height];
    private SpriteRenderer[,] blockVisuals = new SpriteRenderer[Width, Height];

    public bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public bool IsCellFilled(int x, int y)
    {
        return IsInsideBoard(x, y) && cells[x, y];
    }

    public bool IsCellEmpty(int x, int y)
    {
        return IsInsideBoard(x, y) && !cells[x, y];
    }

    public void SetCell(int x, int y, bool filled, Color color = default)
    {
        if (!IsInsideBoard(x, y))
        {
            return;
        }

        cells[x, y] = filled;

        if (filled)
        {
            if (blockVisuals[x, y] == null)
            {
                GameObject block = new GameObject("Block");
                block.transform.SetParent(transform, false);
                block.transform.localPosition = new Vector3(x, y, 0);
                blockVisuals[x, y] = block.AddComponent<SpriteRenderer>();
                blockVisuals[x, y].sprite = SquareSprite.Get();
            }
            blockVisuals[x, y].color = color;
        }
        else if (blockVisuals[x, y] != null)
        {
            Destroy(blockVisuals[x, y].gameObject);
            blockVisuals[x, y] = null;
        }
    }

    public void ClearBoard()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                SetCell(x, y, false);
            }
        }
    }

    public int ClearFullLines()
    {
        int linesCleared = 0;
        for (int y = 0; y < Height; y++)
        {
            if (IsRowFull(y))
            {
                RemoveRow(y);
                linesCleared++;
                y--;
            }
        }
        return linesCleared;
    }

    private bool IsRowFull(int y)
    {
        for (int x = 0; x < Width; x++)
        {
            if (!cells[x, y])
            {
                return false;
            }
        }
        return true;
    }

    private void RemoveRow(int y)
    {
        for (int x = 0; x < Width; x++)
        {
            if (blockVisuals[x, y] != null)
            {
                Destroy(blockVisuals[x, y].gameObject);
                blockVisuals[x, y] = null;
            }
        }

        for (int row = y; row < Height - 1; row++)
        {
            for (int x = 0; x < Width; x++)
            {
                cells[x, row] = cells[x, row + 1];
                blockVisuals[x, row] = blockVisuals[x, row + 1];
                if (blockVisuals[x, row] != null)
                {
                    blockVisuals[x, row].transform.localPosition = new Vector3(x, row, 0);
                }
            }
        }

        for (int x = 0; x < Width; x++)
        {
            cells[x, Height - 1] = false;
            blockVisuals[x, Height - 1] = null;
        }
    }
}
