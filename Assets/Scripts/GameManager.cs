using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private NextPiecePreview nextPiecePreview;
    [SerializeField] private float fallInterval = 1f;
    [SerializeField] private float softDropInterval = 0.05f;

    public event System.Action<int> OnGameOver;

    private Piece activePiece;
    private TetrominoType nextType;
    private float fallTimer;
    private bool isGameOver;

    private void Awake()
    {
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
        nextType = GetRandomType();
        SpawnPiece();
    }

    private static TetrominoType GetRandomType()
    {
        int typeCount = System.Enum.GetValues(typeof(TetrominoType)).Length;
        return (TetrominoType)Random.Range(0, typeCount);
    }

    private void Update()
    {
        if (isGameOver)
        {
            return;
        }

        bool softDrop = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        float interval = softDrop ? softDropInterval : fallInterval;

        fallTimer += Time.deltaTime;
        if (fallTimer >= interval)
        {
            fallTimer = 0f;
            StepDown();
        }
    }

    private void StepDown()
    {
        if (activePiece == null)
        {
            return;
        }

        if (!activePiece.Move(Vector2Int.down))
        {
            LockPiece(activePiece);
            int clearedLines = board.ClearFullLines();
            if (clearedLines > 0)
            {
                scoreManager.AddLines(clearedLines);
            }
            SpawnPiece();
        }
    }

    private void LockPiece(Piece piece)
    {
        Color color = TetrominoShapes.GetColor(piece.type);
        foreach (Vector2Int cell in piece.GetAbsoluteCells())
        {
            board.SetCell(cell.x, cell.y, true, color);
        }
        Destroy(piece.gameObject);
    }

    private void SpawnPiece()
    {
        TetrominoType type = nextType;
        nextType = GetRandomType();
        if (nextPiecePreview != null)
        {
            nextPiecePreview.Show(nextType);
        }

        Vector2Int spawnPosition = new Vector2Int(Board.Width / 2 - 1, Board.Height - 2);

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

    private void GameOver()
    {
        isGameOver = true;
        activePiece = null;
        OnGameOver?.Invoke(scoreManager.Score);
    }
}
