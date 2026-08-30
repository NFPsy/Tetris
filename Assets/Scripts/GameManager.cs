using UnityEngine;
using UnityEngine.InputSystem;

// 게임 전체의 흐름을 관리하는 "총괄" 스크립트입니다.
// - 일정 시간마다 블록을 한 칸씩 떨어뜨리기(타이머)
// - 블록이 더 내려갈 수 없으면 보드에 고정하고, 줄이 찼는지 검사해 점수를 올리기
// - 새 블록을 스폰하고, 스폰할 자리가 막혀있으면 게임 오버 처리하기
public class GameManager : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private NextPiecePreview nextPiecePreview;  // NEXT 패널의 첫 번째 칸(바로 다음 블록)
    [SerializeField] private NextPiecePreview nextPiecePreview2; // NEXT 패널의 두 번째 칸(그다음 블록)
    [SerializeField] private NextPiecePreview holdPreview;

    // 화면에 미리 보여줄 다음 블록 개수
    private const int NextQueueSize = 2;
    [SerializeField] private float fallInterval = 1f;       // 기본 낙하 간격(초). 값이 작을수록 빨리 떨어짐
    [SerializeField] private float softDropInterval = 0.05f; // 아래쪽 화살표를 누르고 있을 때(소프트 드롭) 낙하 간격
    [SerializeField] private float levelSpeedDecay = 0.92f;  // 레벨이 1 오를 때마다 낙하 간격에 곱하는 비율
    [SerializeField] private float minFallInterval = 0.1f;   // 아무리 레벨이 높아져도 이보다 빨라지지는 않음

    // 게임 오버가 되면 이 이벤트가 최종 점수와 함께 발생합니다.
    // GameOverUI 스크립트가 이 이벤트를 구독해서 결과 화면을 띄웁니다.
    public event System.Action<int> OnGameOver;

    // 일시정지 상태가 바뀔 때(true=일시정지, false=재생) 발생합니다.
    // PauseUI 스크립트가 이 이벤트를 구독해서 "PAUSE" 문구를 띄우거나 지웁니다.
    public event System.Action<bool> OnPauseChanged;

    private Piece activePiece;      // 지금 떨어지고 있는 블록
    private readonly System.Collections.Generic.List<TetrominoType> nextQueue = new System.Collections.Generic.List<TetrominoType>(); // 앞으로 나올 블록들(미리 뽑아둠 -> 미리보기에 사용)
    private TetrominoType? heldType; // 보관함(Hold)에 들어있는 블록 종류(비어있으면 null)
    private bool canHold = true;     // 지금 블록에 대해 아직 홀드를 사용하지 않았는지(블록당 1회 제한)
    private float fallTimer;        // 마지막으로 떨어진 뒤 지난 시간
    private bool isGameOver;
    private bool isPaused;

    private void Awake()
    {
        // 인스펙터에서 연결을 안 해둔 경우를 대비해 자동으로 찾아 연결합니다.
        if (board == null)
        {
            board = FindFirstObjectByType<Board>();
        }
        if (scoreManager == null)
        {
            scoreManager = GetComponent<ScoreManager>();
        }
    }

    private void Start()
    {
        // 게임 시작 시 미리보기 개수(NextQueueSize)만큼 "다음 블록"을 미리 뽑아두고, 첫 블록을 스폰합니다.
        for (int i = 0; i < NextQueueSize; i++)
        {
            nextQueue.Add(GetRandomType());
        }
        SpawnPiece();
    }

    // 7가지 블록 종류 중 하나를 무작위로 고릅니다.
    private static TetrominoType GetRandomType()
    {
        int typeCount = System.Enum.GetValues(typeof(TetrominoType)).Length;
        return (TetrominoType)Random.Range(0, typeCount);
    }

    private void Update()
    {
        HandlePauseInput();

        if (isGameOver || isPaused)
        {
            return; // 게임이 끝났거나 일시정지 중이면 더 이상 아무것도 하지 않음
        }

        HandleHardDropInput();
        HandleHoldInput();

        // 레벨이 오를수록 낙하 간격이 점점 짧아지도록 계산합니다.
        // 예: 레벨1은 1초, 레벨2는 1*0.92초, 레벨3은 1*0.92*0.92초... 이런 식으로 점점 빨라지되
        // minFallInterval(0.1초) 아래로는 절대 내려가지 않습니다.
        float currentFallInterval = Mathf.Max(minFallInterval, fallInterval * Mathf.Pow(levelSpeedDecay, scoreManager.Level - 1));

        // 아래쪽 화살표를 누르고 있으면 훨씬 짧은 간격(소프트 드롭)을 사용합니다.
        bool softDrop = Keyboard.current != null && Keyboard.current.downArrowKey.isPressed;
        float interval = softDrop ? softDropInterval : currentFallInterval;

        fallTimer += Time.deltaTime;
        if (fallTimer >= interval)
        {
            fallTimer = 0f;
            StepDown();
        }
    }

    // 스페이스바를 누르는 즉시 블록을 바닥까지 떨어뜨려 고정합니다.
    private void HandleHardDropInput()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            HardDrop();
        }
    }

    // C키를 누르면 현재 블록을 보관함(Hold)의 블록과 교체합니다. 블록당 1회만 가능합니다.
    private void HandleHoldInput()
    {
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            HoldPiece();
        }
    }

    // P키를 누를 때마다 일시정지 상태를 전환합니다. 게임오버 후에는 동작하지 않습니다.
    private void HandlePauseInput()
    {
        if (isGameOver)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            SetPaused(!isPaused);
        }
    }

    // 일시정지 상태를 적용합니다. 낙하 중인 블록의 입력도 함께 멈추거나 재개시킵니다.
    private void SetPaused(bool paused)
    {
        isPaused = paused;
        if (activePiece != null)
        {
            // Piece의 Update()가 멈추도록 컴포넌트 자체를 꺼서 좌우 이동/회전 입력도 함께 막습니다.
            activePiece.enabled = !paused;
        }
        OnPauseChanged?.Invoke(paused);
    }

    // 블록을 한 칸 아래로 내려봅니다. 더 내려갈 수 없으면(바닥/다른 블록에 막히면)
    // 그 자리에 고정하고, 줄 삭제 검사 후 새 블록을 스폰합니다.
    private void StepDown()
    {
        if (activePiece == null)
        {
            return;
        }

        if (!activePiece.Move(Vector2Int.down))
        {
            LockActivePieceAndAdvance();
        }
    }

    // 더 내려갈 수 없을 때까지 한 번에 떨어뜨린 뒤, 그 자리에 즉시 고정합니다.
    private void HardDrop()
    {
        if (activePiece == null)
        {
            return;
        }

        while (activePiece.Move(Vector2Int.down))
        {
        }

        LockActivePieceAndAdvance();
    }

    // 현재 블록을 보드에 고정하고, 줄 삭제 검사와 다음 블록 스폰까지 이어서 처리합니다.
    // 블록이 고정됐으므로 홀드를 다시 사용할 수 있게 됩니다.
    private void LockActivePieceAndAdvance()
    {
        LockPiece(activePiece);
        canHold = true;

        int clearedLines = board.ClearFullLines(out bool hadBomb);
        if (clearedLines > 0)
        {
            scoreManager.AddLines(clearedLines, hadBomb);
        }

        SpawnPiece();
    }

    // 현재 블록을 보관함(Hold)의 블록과 교체합니다.
    // 보관함이 비어있으면 현재 블록을 보관하고 "다음 블록"을 새로 스폰합니다.
    // 보관함에 이미 블록이 있으면 그 블록과 현재 블록을 맞바꿉니다.
    private void HoldPiece()
    {
        if (!canHold || activePiece == null)
        {
            return;
        }

        TetrominoType currentType = activePiece.type;
        Destroy(activePiece.gameObject);
        activePiece = null;

        TetrominoType? previousHeldType = heldType;
        heldType = currentType;
        if (holdPreview != null)
        {
            holdPreview.Show(currentType);
        }

        if (previousHeldType.HasValue)
        {
            SpawnPieceOfType(previousHeldType.Value);
        }
        else
        {
            SpawnPiece();
        }

        canHold = false;
    }

    // 떨어지던 블록(piece)을 보드 데이터에 영구히 새겨넣고, 화면에서 그 블록 오브젝트는 지웁니다.
    // (블록 모양은 이제부터 Board가 관리하는 blockVisuals로 대체됩니다.)
    private void LockPiece(Piece piece)
    {
        Color color = TetrominoShapes.GetColor(piece.type);
        Vector2Int[] absoluteCells = piece.GetAbsoluteCells();
        for (int i = 0; i < absoluteCells.Length; i++)
        {
            // 폭탄 칸은 보드에 폭탄으로 기록해두고, 이 칸이 속한 줄이 삭제될 때 터집니다.
            board.SetCell(absoluteCells[i].x, absoluteCells[i].y, true, color, piece.IsBombCell(i));
        }
        Destroy(piece.gameObject);
    }

    // 새 블록을 보드 위쪽에 스폰합니다.
    // 큐의 맨 앞 블록을 이번에 스폰할 블록으로 꺼내 쓰고, 큐 끝에 새 블록을 채워 넣은 뒤
    // NEXT 패널의 미리보기 화면을 갱신합니다.
    private void SpawnPiece()
    {
        TetrominoType type = nextQueue[0];
        nextQueue.RemoveAt(0);
        nextQueue.Add(GetRandomType());
        UpdateNextPreview();

        SpawnPieceOfType(type);
    }

    // NEXT 패널의 각 칸에 큐에 든 블록들을 순서대로 표시합니다.
    private void UpdateNextPreview()
    {
        if (nextPiecePreview != null)
        {
            nextPiecePreview.Show(nextQueue[0]);
        }
        if (nextPiecePreview2 != null && nextQueue.Count > 1)
        {
            nextPiecePreview2.Show(nextQueue[1]);
        }
    }

    // type 블록을 보드 위쪽에 스폰합니다. "다음 블록" 큐는 건드리지 않으므로,
    // 홀드로 보관해둔 블록을 다시 꺼낼 때도 이 함수를 사용합니다.
    private void SpawnPieceOfType(TetrominoType type)
    {
        Vector2Int spawnPosition = new Vector2Int(Board.Width / 2 - 1, Board.Height - 2);

        // 스폰하려는 자리에 이미 블록이 있다면(=쌓인 블록이 천장까지 닿았다면) 게임 오버입니다.
        foreach (Vector2Int cell in TetrominoShapes.GetCells(type))
        {
            int x = spawnPosition.x + cell.x;
            int y = spawnPosition.y + cell.y;
            if (board.IsCellFilled(x, y))
            {
                GameOver();
                return;
            }
        }

        GameObject pieceObject = new GameObject("Piece");
        activePiece = pieceObject.AddComponent<Piece>();
        activePiece.Initialize(board, type, spawnPosition);
    }

    // 게임을 종료 상태로 만들고, 최종 점수를 담아 OnGameOver 이벤트를 발생시킵니다.
    private void GameOver()
    {
        isGameOver = true;
        activePiece = null;
        OnGameOver?.Invoke(scoreManager.Score);
    }
}
