using UnityEngine;

// 테트리스 보드(10칸 x 20칸)의 상태를 저장하고 그리는 스크립트입니다.
// - cells 배열: 각 칸이 비어있는지(false) 채워져있는지(true) 저장하는 "데이터"
// - blockVisuals 배열: 채워진 칸을 화면에 실제로 보여주는 "네모 스프라이트"
// 데이터와 화면 표시를 같이 관리해서, 줄이 삭제될 때 둘 다 함께 갱신됩니다.
public class Board : MonoBehaviour
{
    // 보드의 가로/세로 칸 수 (표준 테트리스 크기)
    public const int Width = 10;
    public const int Height = 20;

    // [x, y] 위치의 칸이 채워져 있는지 여부만 저장하는 2차원 배열
    private bool[,] cells = new bool[Width, Height];

    // [x, y] 위치에 실제로 보이는 블록 스프라이트(없으면 null)
    private SpriteRenderer[,] blockVisuals = new SpriteRenderer[Width, Height];

    // [x, y] 위치의 칸이 폭탄인지 여부. 그 줄이 삭제될 때 점수를 2배로 주는 데 사용됩니다.
    private bool[,] bombs = new bool[Width, Height];


    private void Awake()
    {
        // 게임 시작 시 보드 범위를 눈으로 알 수 있게 테두리 선을 그립니다.
        DrawBorder();
    }

    // LineRenderer로 보드 바깥쪽에 사각형 테두리를 그립니다.
    // 블록 스프라이트는 칸 중심(pivot 0.5, 0.5)을 기준으로 그려지기 때문에,
    // 칸 (0,0)은 실제로 (-0.5,-0.5)~(0.5,0.5) 범위를 차지합니다.
    // 그래서 테두리도 -0.5만큼 당겨서 그려야 블록들과 정확히 맞습니다.
    private void DrawBorder()
    {
        GameObject borderObject = new GameObject("Border");
        borderObject.transform.SetParent(transform, false);

        LineRenderer line = borderObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true; // 마지막 점과 첫 점을 이어서 사각형을 닫음
        line.positionCount = 4;
        line.SetPosition(0, new Vector3(-0.5f, -0.5f, 0));
        line.SetPosition(1, new Vector3(Width - 0.5f, -0.5f, 0));
        line.SetPosition(2, new Vector3(Width - 0.5f, Height - 0.5f, 0));
        line.SetPosition(3, new Vector3(-0.5f, Height - 0.5f, 0));
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.white;
        line.endColor = Color.white;
        line.sortingOrder = 1;
    }

    // (x, y)가 보드 범위 안에 있는지 확인
    public bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    // (x, y) 칸이 보드 안에 있으면서 이미 채워져 있는지 확인
    public bool IsCellFilled(int x, int y)
    {
        return IsInsideBoard(x, y) && cells[x, y];
    }

    // (x, y) 칸이 보드 안에 있으면서 비어있는지 확인
    // 블록 이동/회전 가능 여부를 판단할 때 이 함수를 사용합니다.
    public bool IsCellEmpty(int x, int y)
    {
        return IsInsideBoard(x, y) && !cells[x, y];
    }

    // (x, y) 칸을 채우거나 비웁니다. 채울 때는 color로 화면에 보일 블록을 새로 만들고,
    // 비울 때는 화면에 있던 블록 오브젝트를 지웁니다.
    // isBomb이 true면 이 칸을 폭탄으로 기억해두었다가, 이 칸이 속한 줄이 삭제될 때 점수를 2배로 줍니다.
    public void SetCell(int x, int y, bool filled, Color color = default, bool isBomb = false)
    {
        if (!IsInsideBoard(x, y))
        {
            return;
        }

        cells[x, y] = filled;
        bombs[x, y] = filled && isBomb;

        if (filled)
        {
            // 아직 이 칸에 블록 스프라이트가 없으면 새로 만듭니다.
            if (blockVisuals[x, y] == null)
            {
                GameObject block = new GameObject("Block");
                block.transform.SetParent(transform, false);
                block.transform.localPosition = new Vector3(x, y, 0);
                blockVisuals[x, y] = block.AddComponent<SpriteRenderer>();
            }
            blockVisuals[x, y].sprite = isBomb ? BombSprite.Get() : SquareSprite.Get();
            blockVisuals[x, y].color = isBomb ? Color.white : color;
        }
        else if (blockVisuals[x, y] != null)
        {
            // 비우는 경우 화면에 있던 블록을 삭제합니다.
            Destroy(blockVisuals[x, y].gameObject);
            blockVisuals[x, y] = null;
        }
    }

    // 보드 전체를 초기 상태(빈 보드)로 되돌립니다.
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

    // 가로 한 줄이 전부 채워진 줄들을 찾아서 지우고, 지운 줄 수를 반환합니다.
    // 아래에서 위로 검사하다가 꽉 찬 줄을 지우면, 그 위의 줄들이 한 칸씩 내려오므로
    // 같은 y 위치를 다시 검사해야 합니다(y--).
    // hadBomb에는 지워진 줄들 중 폭탄 칸이 하나라도 있었는지가 담깁니다(점수 2배 판정용).
    public int ClearFullLines(out bool hadBomb)
    {
        int linesCleared = 0;
        hadBomb = false;
        for (int y = 0; y < Height; y++)
        {
            if (IsRowFull(y))
            {
                if (RowHasBomb(y))
                {
                    hadBomb = true;
                }
                RemoveRow(y);
                linesCleared++;
                y--; // 위 줄이 내려와서 같은 y를 다시 확인
            }
        }
        return linesCleared;
    }

    // y번째 줄에 폭탄 칸이 하나라도 있는지 확인
    private bool RowHasBomb(int y)
    {
        for (int x = 0; x < Width; x++)
        {
            if (bombs[x, y])
            {
                return true;
            }
        }
        return false;
    }

    // y번째 줄의 모든 칸이 채워져 있는지 확인
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

    // y번째 줄을 지우고, 그 위에 있던 모든 줄을 한 칸씩 아래로 내립니다.
    private void RemoveRow(int y)
    {
        // 1) 지울 줄(y)의 블록 스프라이트를 화면에서 삭제
        for (int x = 0; x < Width; x++)
        {
            if (blockVisuals[x, y] != null)
            {
                Destroy(blockVisuals[x, y].gameObject);
                blockVisuals[x, y] = null;
            }
        }

        // 2) y 줄부터 맨 위 줄 바로 아래까지, 한 칸 위(row+1)의 데이터를
        //    지금 줄(row)로 복사해서 전체적으로 한 칸씩 내립니다.
        for (int row = y; row < Height - 1; row++)
        {
            for (int x = 0; x < Width; x++)
            {
                cells[x, row] = cells[x, row + 1];
                bombs[x, row] = bombs[x, row + 1];
                blockVisuals[x, row] = blockVisuals[x, row + 1];
                if (blockVisuals[x, row] != null)
                {
                    // 스프라이트도 새 위치(한 칸 아래)로 옮겨줍니다.
                    blockVisuals[x, row].transform.localPosition = new Vector3(x, row, 0);
                }
            }
        }

        // 3) 맨 위 줄은 내용이 아래로 복사되고 남은 자리이므로 비워줍니다.
        for (int x = 0; x < Width; x++)
        {
            cells[x, Height - 1] = false;
            bombs[x, Height - 1] = false;
            blockVisuals[x, Height - 1] = null;
        }
    }
}
