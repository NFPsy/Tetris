using UnityEngine;
using UnityEngine.InputSystem;

// 지금 화면에서 떨어지고 있는 블록(테트로미노) 하나를 담당하는 스크립트입니다.
// 이 블록의 위치, 모양, 좌우 이동/회전 입력 처리를 모두 여기서 합니다.
// 블록이 바닥에 닿아 "고정"되는 처리는 GameManager가 담당합니다.
public class Piece : MonoBehaviour
{
    public Board board;           // 이 블록이 놓여있는 보드
    public TetrominoType type;    // 블록 종류(I, O, T, S, Z, J, L)
    public Vector2Int position;   // 보드 기준 이 블록의 회전축(피벗) 위치

    // 블록 4칸 중 하나가 폭탄일 확률
    private const float BombChance = 0.15f;
    // 고스트 블록(착지 예상 위치 미리보기)의 투명도
    private const float GhostAlpha = 0.25f;

    // 현재 회전 상태에서, 피벗을 기준으로 한 4칸의 상대 좌표
    private Vector2Int[] cells;
    // 화면에 보이는 4개의 블록 사각형(스프라이트)
    private SpriteRenderer[] blockRenderers;
    // 바닥에 그대로 떨어졌을 때 도착할 위치를 보여주는 반투명 블록들
    private SpriteRenderer[] ghostRenderers;
    // 4칸 중 폭탄인 칸의 인덱스(-1이면 폭탄 없음)
    private int bombCellIndex = -1;

    public bool HasBomb => bombCellIndex >= 0;

    // 매 프레임 키 입력을 확인해서 좌우 이동/회전을 처리합니다.
    // (아래로 빨리 떨어뜨리는 소프트 드롭은 GameManager에서 스페이스바로 처리합니다.)
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

    // GameManager가 새 블록을 만들 때 호출합니다.
    // 블록 종류/시작 위치를 정하고, 화면에 보일 4개의 사각형을 만듭니다.
    public void Initialize(Board targetBoard, TetrominoType tetrominoType, Vector2Int spawnPosition)
    {
        board = targetBoard;
        type = tetrominoType;
        position = spawnPosition;
        cells = TetrominoShapes.GetCells(type);

        // 일정 확률로 4칸 중 하나를 폭탄 칸으로 지정합니다.
        bombCellIndex = Random.value < BombChance ? Random.Range(0, cells.Length) : -1;

        blockRenderers = new SpriteRenderer[cells.Length];
        ghostRenderers = new SpriteRenderer[cells.Length];
        Color color = TetrominoShapes.GetColor(type);
        Color ghostColor = color;
        ghostColor.a = GhostAlpha;
        for (int i = 0; i < cells.Length; i++)
        {
            GameObject block = new GameObject("Block");
            block.transform.SetParent(transform, false);
            // 피벗 기준 상대 좌표를 그대로 자식 오브젝트의 로컬 위치로 사용
            block.transform.localPosition = new Vector3(cells[i].x, cells[i].y, 0);
            SpriteRenderer blockRenderer = block.AddComponent<SpriteRenderer>();
            bool isBomb = i == bombCellIndex;
            blockRenderer.sprite = isBomb ? BombSprite.Get() : SquareSprite.Get();
            blockRenderer.color = isBomb ? Color.white : color;
            blockRenderers[i] = blockRenderer;

            // 고스트 블록은 항상 일반 사각형 모양으로, 반투명하게 표시합니다.
            GameObject ghost = new GameObject("Ghost");
            ghost.transform.SetParent(transform, false);
            SpriteRenderer ghostRenderer = ghost.AddComponent<SpriteRenderer>();
            ghostRenderer.sprite = SquareSprite.Get();
            ghostRenderer.color = ghostColor;
            ghostRenderer.sortingOrder = -1; // 실제 블록보다 뒤에 그려지도록
            ghostRenderers[i] = ghostRenderer;
        }

        UpdatePosition();
        UpdateGhost();
    }

    // testPosition에 testCells 모양대로 블록을 놓았을 때, 4칸 전부 보드 범위 안에 있고
    // 비어있는지(다른 블록과 겹치지 않는지) 확인합니다.
    // 이동, 회전, 낙하 모두 이 함수를 통해 "이렇게 움직여도 되는지"를 판단합니다.
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

    // delta(예: 왼쪽/오른쪽/아래쪽)만큼 블록을 옮겨봅니다.
    // 이동한 위치가 유효하지 않으면(범위 밖이거나 다른 블록과 겹치면) 이동을 취소하고 false를 반환합니다.
    public bool Move(Vector2Int delta)
    {
        Vector2Int newPosition = position + delta;
        if (!IsValidPosition(cells, newPosition))
        {
            return false;
        }
        position = newPosition;
        UpdatePosition();
        UpdateGhost();
        return true;
    }

    // 블록을 시계방향으로 90도 회전시킵니다.
    public bool Rotate()
    {
        // O(정사각형)는 어떻게 돌려도 모양이 같으므로 회전할 필요가 없습니다.
        if (type == TetrominoType.O)
        {
            return true;
        }

        // 회전 공식 (x, y) -> (y, -x): 피벗 (0,0)을 중심으로 시계방향 90도 회전
        Vector2Int[] rotatedCells = new Vector2Int[cells.Length];
        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int cell = cells[i];
            rotatedCells[i] = new Vector2Int(cell.y, -cell.x);
        }

        // 회전한 모양이 보드 범위를 벗어나거나 다른 블록과 겹치면 회전을 취소합니다.
        // (이 프로젝트는 "월킥"은 구현하지 않아서, 회전이 막히면 그냥 회전하지 않습니다.)
        if (!IsValidPosition(rotatedCells, position))
        {
            return false;
        }

        cells = rotatedCells;
        UpdateBlockShapes();
        UpdateGhost();
        return true;
    }

    // 현재 블록이 차지하는 4칸의 "보드 절대 좌표"를 계산합니다.
    // 블록이 바닥에 고정될 때 GameManager가 이 좌표들을 보드 데이터에 반영합니다.
    public Vector2Int[] GetAbsoluteCells()
    {
        Vector2Int[] absolute = new Vector2Int[cells.Length];
        for (int i = 0; i < cells.Length; i++)
        {
            absolute[i] = position + cells[i];
        }
        return absolute;
    }

    // GetAbsoluteCells()가 반환한 배열에서 index번째 칸이 폭탄 칸인지 확인합니다.
    public bool IsBombCell(int index)
    {
        return index == bombCellIndex;
    }

    // position 값이 바뀔 때마다 실제 화면 위치(transform.position)를 갱신합니다.
    private void UpdatePosition()
    {
        transform.position = board.transform.position + new Vector3(position.x, position.y, 0);
    }

    // 회전으로 모양(cells)이 바뀔 때, 화면에 보이는 4개 사각형의 위치도 새 모양에 맞게 갱신합니다.
    private void UpdateBlockShapes()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            blockRenderers[i].transform.localPosition = new Vector3(cells[i].x, cells[i].y, 0);
        }
    }

    // 지금 모양 그대로 곧장 떨어뜨렸을 때 도착할 위치(가장 아래로 내려간 위치)를 계산해서
    // 고스트 블록들을 그 자리로 옮깁니다. 월드 좌표를 직접 지정하므로 piece 자신의 위치와 무관하게 정확합니다.
    private void UpdateGhost()
    {
        Vector2Int ghostPosition = position;
        while (IsValidPosition(cells, ghostPosition + Vector2Int.down))
        {
            ghostPosition += Vector2Int.down;
        }

        for (int i = 0; i < cells.Length; i++)
        {
            ghostRenderers[i].transform.position = board.transform.position
                + new Vector3(ghostPosition.x + cells[i].x, ghostPosition.y + cells[i].y, 0);
        }
    }
}
