using System.Collections;
using System.Linq;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public static Piece selectedPiece;
    public enum ColorState {NONE, YELLOW, RED};
    public enum PieceState {NONE, TILED, KICKED, OUT};
    [SerializeField] Material transparentMaterial;
    [SerializeField] Material highlightMaterial;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Material[] materialList;
    private Coroutine moveCoroutine;
    private GameObject boardParent;
    [HideInInspector] public Tile currentTile;
    [HideInInspector] public ColorState colorState;
    [HideInInspector] public PieceState pieceState;
    [HideInInspector] public bool canSelect = false;
    private int nbTiles = 24;
    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        materialList = meshRenderer.materials;
        materialList[1] = transparentMaterial;
        meshRenderer.materials = materialList;
        meshCollider = GetComponent<MeshCollider>();
        canSelect = false;
        colorState = ColorState.NONE;
        pieceState = PieceState.TILED;
        boardParent = FindObjectOfType<BoardLayout>().gameObject;
    }

    private void OnMouseDown()
    {
        if (canSelect == false) return;
        var pieceList = FindObjectsOfType<Piece>().ToList();
        foreach (Piece piece in pieceList) {
            if (piece == this) {
                continue;
            }
            piece.Deselect();
        }
        
        if (materialList[1] == highlightMaterial) {
            Deselect();
            ClearTileMovePossible();
        } else {
            ClearTileMovePossible();
            for (int i = 1 ; i <= GameController.gameController.GetCurrentMoveDone() ; i++) {
                GetTileMovePossible(GameController.gameController.GetDiceValue(i), GetCurrentTileIndex());
            }
            Select();
        }
    }

    private int GetCurrentTileIndex()
    {
        if (pieceState == PieceState.TILED) {
            return currentTile.index;
        }
        if (pieceState == PieceState.KICKED) {
            return colorState == ColorState.YELLOW ? 0 : 25;
        }
        return colorState == ColorState.YELLOW ? 25 : 0;
    }

    public void ClearTileMovePossible()
    {
        var tileList = FindObjectsOfType<Tile>().ToList();
        foreach (Tile tile in tileList) {
            tile.HighlightTile(false);
        }
        GameController.gameController.yellowOutZone.GetComponent<OutZone>().HighlightZone(false);
        GameController.gameController.redOutZone.GetComponent<OutZone>().HighlightZone(false);
    }

    public void GetTileMovePossible(int moveValue, int currentTileIndex)
    {
        var tileList = FindObjectsOfType<Tile>().ToList();

        if (colorState == ColorState.YELLOW) {
            var yellowOutZone = GameController.gameController.yellowOutZone.GetComponent<OutZone>();
            if (GameController.gameController.CanMoveOut() && (currentTileIndex + moveValue == yellowOutZone.index)) {
                yellowOutZone.CanSelect(true);
                return;
            }
            if (GameController.gameController.CanMoveOut() && currentTileIndex + moveValue > nbTiles && GameController.gameController.IsPieceOnHigherTile(tileList, this)) {
                yellowOutZone.CanSelect(true);
                return;
            }
            if (currentTileIndex + moveValue > nbTiles) return;
            Tile tile = tileList.Find(tile => tile.index == currentTileIndex + moveValue);
            HighlightTileMovePossible(tile, ColorState.YELLOW);
        } else {
            var redOutZone = GameController.gameController.redOutZone.GetComponent<OutZone>();
            if (GameController.gameController.CanMoveOut() && (currentTileIndex - moveValue == redOutZone.index)) {
                redOutZone.CanSelect(true);
                return;
            }
            if (GameController.gameController.CanMoveOut() && currentTileIndex - moveValue < 1 && GameController.gameController.IsPieceOnHigherTile(tileList, this)) {
                redOutZone.CanSelect(true);
                return;
            }
            if (currentTileIndex - moveValue < 1) return;
            Tile tile = tileList.Find(tile => tile.index == currentTileIndex - moveValue);
            HighlightTileMovePossible(tile, ColorState.RED);
        }
    }

    private void HighlightTileMovePossible(Tile tile, ColorState colorStateValue)
    {
        var pieces = tile.tileCenter.GetComponentsInChildren<Piece>().ToList();
        if (pieces != null && pieces.Count >= 1) {
            if (pieces.First().colorState != colorStateValue&& pieces.Count >= 2) {
                return;
            }
        }
        tile.CanSelect(true);
        tile.EnableLocalPiecesMeshCollider(false);
    }

    private void Select()
    {
        selectedPiece = this;
        HighlightTile(true);
        meshRenderer.materials = materialList;
    }

    private void Deselect()
    {
        selectedPiece = null;
        HighlightTile(false);
        meshRenderer.materials = materialList;
        EnableAllPiecesMeshCollider(true);
    }

    private void Deselect(bool isSelected)
    {
        if (isSelected) selectedPiece = null;
        HighlightTile(false);
        meshRenderer.materials = materialList;
        EnableAllPiecesMeshCollider(true);
    }

    public void CanSelect(bool state)
    {
        canSelect = state;
        HighlightTile(state);
    }

    public void HighlightTile(bool state)
    {
        if (state) {
            materialList[1] = highlightMaterial;
        } else {
            materialList[1] = transparentMaterial;
        }
    }

    public void EnableTileMeshCollider(bool state)
    {
        meshCollider.enabled = state;
    }

    public void EnableAllPiecesMeshCollider(bool state) {
        var pieceList = FindObjectsOfType<Piece>().ToList();
        pieceList.ForEach(outZone => outZone.EnableTileMeshCollider(state));
    }

    public void SetColorState(ColorState newColorState)
    {
        colorState = newColorState;
    }

    public void Kick()
    {
        transform.parent = boardParent.transform;
        currentTile = null;
        pieceState = PieceState.KICKED;
        MoveToReset(colorState == ColorState.YELLOW ? GameController.gameController.yellowKickSpawn.transform.localPosition : GameController.gameController.redKickSpawn.transform.localPosition);
    }

    public void MoveToReset(Vector3 newPosition)
    {
        if (moveCoroutine != null) {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(SmoothMove(newPosition));
        Deselect(false);
    }

    public void MoveTo(Vector3 newPosition)
    {
        if (moveCoroutine != null) {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(SmoothMove(newPosition));
        currentTile = pieceState == PieceState.TILED ? GetComponentInParent<Tile>() : null;
        Deselect();
    }

    private IEnumerator SmoothMove(Vector3 targetPosition)
    {
        float duration = 1f;
        float elapsed = 0.0f;

        Vector3 startPosition = transform.localPosition;

        while (elapsed < duration) {
            transform.localPosition = Vector3.MoveTowards(startPosition, targetPosition, duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPosition;
    }
}
