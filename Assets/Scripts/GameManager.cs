using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private float fallInterval = 1f;

    private Piece activePiece;
    private float fallTimer;

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
        SpawnPiece();
    }

    private void Update()
    {
        fallTimer += Time.deltaTime;
        if (fallTimer >= fallInterval)
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
        int typeCount = System.Enum.GetValues(typeof(TetrominoType)).Length;
        TetrominoType type = (TetrominoType)Random.Range(0, typeCount);
        Vector2Int spawnPosition = new Vector2Int(Board.Width / 2 - 1, Board.Height - 2);

        GameObject pieceObject = new GameObject("Piece");
        activePiece = pieceObject.AddComponent<Piece>();
        activePiece.Initialize(board, type, spawnPosition);
    }
}
