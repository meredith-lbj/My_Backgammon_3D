using System;
using System.Linq;
using UnityEngine;

public class OutZone : MonoBehaviour
{
    public int index = -1;
    public Piece.ColorState colorState;
    private int maxPieces = 15;
    private int currentPieces = 0;
    private MeshRenderer meshRenderer;
    public GameObject zonePieceSpawn;
    [HideInInspector] public bool canSelect = false;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        zonePieceSpawn = transform.GetChild(0).gameObject;

        canSelect = false;
    }

    private void OnMouseDown()
    {
        if (canSelect == false) return;

        if (Piece.selectedPiece != null && Piece.selectedPiece.pieceState == Piece.PieceState.TILED) {
            Tile currentTile = Piece.selectedPiece.currentTile;
            GameController.gameController.UpdateAvailableMove(Math.Abs(currentTile.index - index));
            Piece.selectedPiece.transform.parent = null;
            currentTile.RemovePiece();

            var tileList = FindObjectsOfType<Tile>().ToList();
            tileList.ForEach(tile => tile.CanSelect(false));
            var outZoneList = FindObjectsOfType<OutZone>().ToList();
            outZoneList.ForEach(outZone => outZone.CanSelect(false));

            Piece.selectedPiece.transform.parent = zonePieceSpawn.transform;
            Piece.selectedPiece.transform.localPosition = Vector3.zero;
            Piece.selectedPiece.pieceState = Piece.PieceState.OUT;
            Piece.selectedPiece.MoveTo(Vector3.zero);
            Piece.selectedPiece = null;

            AddPiece();

            GameController.gameController.UpdateOutPieceCounter();
            GameController.gameController.CheckPlayerIsWinner(colorState, currentPieces, maxPieces);
            GameController.gameController.CheckMoveIsPossible();
        }
    }

    public void HighlightZone(bool state)
    {
        if (state) {
            meshRenderer.enabled = true;
        } else {
            meshRenderer.enabled = false;
        }
    }

    public Vector3 AddPiece()
    {
        currentPieces++;
        return zonePieceSpawn.transform.localPosition;
    }

    public bool CanAddPiece(Piece piece)
    {
        var pieceList = GetComponentsInChildren<Piece>().ToList();
        if (pieceList == null || pieceList.Count <= 1 || pieceList.First().colorState == piece.colorState) return true;
        return false;
    }

    public void CanSelect(bool state)
    {
        canSelect = state;
        HighlightZone(state);
    }
}
