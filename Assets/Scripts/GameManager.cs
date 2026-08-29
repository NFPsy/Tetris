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
    [SerializeField] private NextPiecePreview nextPiecePreview;
    [SerializeField] private float fallInterval = 1f;       // 기본 낙하 간격(초). 값이 작을수록 빨리 떨어짐
    [SerializeField] private float softDropInterval = 0.05f; // 스페이스바를 누르고 있을 때(소프트 드롭) 낙하 간격
    [SerializeField] private float levelSpeedDecay = 0.92f;  // 레벨이 1 오를 때마다 낙하 간격에 곱하는 비율
    [SerializeField] private float minFallInterval = 0.1f;   // 아무리 레벨이 높아져도 이보다 빨라지지는 않음

    // 게임 오버가 되면 이 이벤트가 최종 점수와 함께 발생합니다.
    // GameOverUI 스크립트가 이 이벤트를 구독해서 결과 화면을 띄웁니다.
    public event System.Action<int> OnGameOver;

    private Piece activePiece;      // 지금 떨어지고 있는 블록
    private TetrominoType nextType; // 다음에 나올 블록 종류(미리 뽑아둠 -> 미리보기에 사용)
    private float fallTimer;        // 마지막으로 떨어진 뒤 지난 시간
    private bool isGameOver;

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
        // 게임 시작 시 "다음 블록"을 미리 하나 뽑아두고, 첫 블록을 스폰합니다.
        nextType = GetRandomType();
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
        if (isGameOver)
        {
            return; // 게임이 끝났으면 더 이상 아무것도 하지 않음
        }

        // 레벨이 오를수록 낙하 간격이 점점 짧아지도록 계산합니다.
        // 예: 레벨1은 1초, 레벨2는 1*0.92초, 레벨3은 1*0.92*0.92초... 이런 식으로 점점 빨라지되
        // minFallInterval(0.1초) 아래로는 절대 내려가지 않습니다.
        float currentFallInterval = Mathf.Max(minFallInterval, fallInterval * Mathf.Pow(levelSpeedDecay, scoreManager.Level - 1));

        // 스페이스바를 누르고 있으면 훨씬 짧은 간격(소프트 드롭)을 사용합니다.
        bool softDrop = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        float interval = softDrop ? softDropInterval : currentFallInterval;

        fallTimer += Time.deltaTime;
        if (fallTimer >= interval)
        {
            fallTimer = 0f;
            StepDown();
        }
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
            LockPiece(activePiece);

            int clearedLines = board.ClearFullLines(out bool hadBomb);
            if (clearedLines > 0)
            {
                scoreManager.AddLines(clearedLines, hadBomb);
            }

            SpawnPiece();
        }
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
    private void SpawnPiece()
    {
        // 미리 뽑아뒀던 "다음 블록"을 이번에 스폰할 블록으로 사용하고,
        // 그 다음 블록을 새로 뽑아서 미리보기 화면을 갱신합니다.
        TetrominoType type = nextType;
        nextType = GetRandomType();
        if (nextPiecePreview != null)
        {
            nextPiecePreview.Show(nextType);
        }

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
