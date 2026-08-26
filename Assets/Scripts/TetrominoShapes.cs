using UnityEngine;

// 테트리스에서 사용하는 7가지 블록(테트로미노) 종류입니다.
public enum TetrominoType
{
    I, O, T, S, Z, J, L
}

// 각 블록 종류의 "모양"과 "색깔"을 정의하는 스크립트입니다.
// 실제 게임 로직(이동, 회전 등)은 여기 없고, 순수하게 데이터만 담당합니다.
public static class TetrominoShapes
{
    // 블록을 이루는 4칸의 좌표를 돌려줍니다.
    // 좌표는 회전 기준점 (0,0)을 기준으로 한 상대 좌표입니다.
    // 예) O(정사각형)는 (0,0),(1,0),(0,1),(1,1) 네 칸으로 이루어집니다.
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

    // 블록 종류별로 화면에 표시할 색깔을 돌려줍니다.
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
