using UnityEngine;

public enum TetrominoType
{
    I, O, T, S, Z, J, L
}

public static class TetrominoShapes
{
    public static Vector2Int[] GetCells(TetrominoType type)
    {
        switch (type)
        {
            case TetrominoType.I:
                return new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) };
            case TetrominoType.O:
                return new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) };
            case TetrominoType.T:
                return new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) };
            case TetrominoType.S:
                return new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) };
            case TetrominoType.Z:
                return new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(-1, 1) };
            case TetrominoType.J:
                return new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0) };
            case TetrominoType.L:
                return new Vector2Int[] { new Vector2Int(1, 1), new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0) };
            default:
                return new Vector2Int[4];
        }
    }

    public static Color GetColor(TetrominoType type)
    {
        switch (type)
        {
            case TetrominoType.I: return Color.cyan;
            case TetrominoType.O: return Color.yellow;
            case TetrominoType.T: return new Color(0.6f, 0f, 0.8f);
            case TetrominoType.S: return Color.green;
            case TetrominoType.Z: return Color.red;
            case TetrominoType.J: return Color.blue;
            case TetrominoType.L: return new Color(1f, 0.5f, 0f);
            default: return Color.white;
        }
    }
}
