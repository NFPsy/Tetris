using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private float fallInterval = 1f;

    private Piece activePiece;
    private float fallTimer;

    private void Awake()
    {
        if (board == null)
        {
            board = FindFirstObjectByType<Board>();
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
        if (activePiece != null)
        {
            activePiece.Move(Vector2Int.down);
        }
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
