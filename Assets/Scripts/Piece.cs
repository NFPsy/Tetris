using UnityEngine;
using UnityEngine.InputSystem;

public class Piece : MonoBehaviour
{
    public Board board;
    public TetrominoType type;
    public Vector2Int position;

    private Vector2Int[] cells;
    private SpriteRenderer[] blockRenderers;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.leftArrowKey.wasPressedThisFrame)
        {
            Move(Vector2Int.left);
        }
        else if (keyboard.rightArrowKey.wasPressedThisFrame)
        {
            Move(Vector2Int.right);
        }
        else if (keyboard.upArrowKey.wasPressedThisFrame)
        {
            Rotate();
        }
    }

    public void Initialize(Board targetBoard, TetrominoType tetrominoType, Vector2Int spawnPosition)
    {
        board = targetBoard;
        type = tetrominoType;
        position = spawnPosition;
        cells = TetrominoShapes.GetCells(type);

        blockRenderers = new SpriteRenderer[cells.Length];
        Color color = TetrominoShapes.GetColor(type);
        for (int i = 0; i < cells.Length; i++)
        {
            GameObject block = new GameObject("Block");
            block.transform.SetParent(transform, false);
            block.transform.localPosition = new Vector3(cells[i].x, cells[i].y, 0);
            SpriteRenderer blockRenderer = block.AddComponent<SpriteRenderer>();
            blockRenderer.sprite = SquareSprite.Get();
            blockRenderer.color = color;
            blockRenderers[i] = blockRenderer;
        }

        UpdatePosition();
    }

    public bool IsValidPosition(Vector2Int[] testCells, Vector2Int testPosition)
    {
        foreach (Vector2Int cell in testCells)
        {
            int x = testPosition.x + cell.x;
            int y = testPosition.y + cell.y;
            if (!board.IsCellEmpty(x, y))
            {
                return false;
            }
        }
        return true;
    }

    public bool Move(Vector2Int delta)
    {
        Vector2Int newPosition = position + delta;
        if (!IsValidPosition(cells, newPosition))
        {
            return false;
        }
        position = newPosition;
        UpdatePosition();
        return true;
    }

    public bool Rotate()
    {
        if (type == TetrominoType.O)
        {
            return true;
        }

        Vector2Int[] rotatedCells = new Vector2Int[cells.Length];
        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int cell = cells[i];
            rotatedCells[i] = new Vector2Int(cell.y, -cell.x);
        }

        if (!IsValidPosition(rotatedCells, position))
        {
            return false;
        }

        cells = rotatedCells;
        UpdateBlockShapes();
        return true;
    }

    private void UpdatePosition()
    {
        transform.position = board.transform.position + new Vector3(position.x, position.y, 0);
    }

    private void UpdateBlockShapes()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            blockRenderers[i].transform.localPosition = new Vector3(cells[i].x, cells[i].y, 0);
        }
    }
}
