using UnityEngine;

public class Board : MonoBehaviour
{
    public const int Width = 10;
    public const int Height = 20;

    private bool[,] cells = new bool[Width, Height];

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

    public void SetCell(int x, int y, bool filled)
    {
        if (IsInsideBoard(x, y))
        {
            cells[x, y] = filled;
        }
    }

    public void ClearBoard()
    {
        cells = new bool[Width, Height];
    }
}
