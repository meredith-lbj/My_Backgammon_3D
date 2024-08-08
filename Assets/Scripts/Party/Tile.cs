using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Material defaultTileMaterial;
    public Material highlightTileMaterial;
    public int index = -1;
    private int maxPieces = 5;
    private int currentPieces = 0;
    private MeshRenderer meshRenderer;
    public GameObject tileCenter;
    private float pieceSize = 0.05f;
    public bool canSelect = false;
    public List<Vector3> allLocalPositions = new List<Vector3>() {};

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        tileCenter = transform.GetChild(0).gameObject;

        float x = pieceSize / 2;
        for (int i = 0 ; i < maxPieces ; i++) {
            allLocalPositions.Add(new Vector3(x, 0, 0));
            x += pieceSize;
        }

        canSelect = false;
    }

    private void OnMouseDown()
    {
        if (canSelect == false) return;

        if (Piece.selectedPiece != null) {
            if (Piece.selectedPiece.pieceState == Piece.PieceState.TILED) {
                Tile currentTile = Piece.selectedPiece.currentTile;
                GameController.gameController.UpdateAvailableMove(Math.Abs(currentTile.index - index));
                Piece.selectedPiece.transform.parent = null;
                currentTile.RemovePiece();
            } else {
                GameController.gameController.UpdateAvailableMove(Math.Abs(Piece.selectedPiece.colorState == Piece.ColorState.YELLOW ? 0 - index : 25 - index));
                Piece.selectedPiece.pieceState = Piece.PieceState.TILED;
                Piece.selectedPiece.transform.parent = null;
                if (GameController.gameController.IsPieceKickedLeft()) Piece.selectedPiece.canSelect = false;
            }

            var tileList = FindObjectsOfType<Tile>().ToList();
            tileList.ForEach(tile => tile.CanSelect(false));
            var outZoneList = FindObjectsOfType<OutZone>().ToList();
            outZoneList.ForEach(outZone => outZone.CanSelect(false));

            var pieces = tileCenter.GetComponentsInChildren<Piece>().ToList();
            if (pieces != null && pieces.Count == 1 && pieces.First().colorState != Piece.selectedPiece.colorState) {
                pieces.First().Kick();
                RemovePiece();
                GameController.gameController.UpdateKickedPieceCounter();
            }

            Piece.selectedPiece.transform.parent = tileCenter.transform;
            Piece.selectedPiece.transform.localPosition = Vector3.zero;
            Piece.selectedPiece.MoveTo(AddPiece());
            Piece.selectedPiece = null;

            EnableAllPiecesMeshCollider(true);

            GameController.gameController.CheckMoveIsPossible();
        }
    }

    public void HighlightTile(bool state)
    {
        if (state) {
            meshRenderer.material = highlightTileMaterial;
        } else {
            meshRenderer.material = defaultTileMaterial;
        }
    }

    public void EnableLocalPiecesMeshCollider(bool state)
    {
        var pieceList = tileCenter.GetComponentsInChildren<Piece>().ToList();
        pieceList.ForEach(piece => piece.EnableTileMeshCollider(state));
    }

    public void EnableAllPiecesMeshCollider(bool state)
    {
        var pieceList = FindObjectsOfType<Piece>().ToList();
        pieceList.ForEach(piece => piece.EnableTileMeshCollider(state));
    }

    public Vector3 AddPiece()
    {
        if (currentPieces + 1 > maxPieces) {
            float x = pieceSize + (pieceSize * (maxPieces % 5));
            maxPieces++;
            allLocalPositions.Add(new Vector3(x, maxPieces / 5 * 0.01f, 0));
        }
        return allLocalPositions[currentPieces++];
    }

    public List<Vector3> AddPiece(int value)
    {
        currentPieces += value;
        Refresh();
        return allLocalPositions.GetRange(currentPieces - value, currentPieces);
    }

    public Vector3 RemovePiece()
    {
        currentPieces--;
        Refresh();
        return allLocalPositions[currentPieces];
    }

    public void Refresh()
    {
        for (int i = 0 ; i < tileCenter.transform.childCount ; i++) {
            tileCenter.transform.GetChild(i).GetComponent<TrailRenderer>().enabled = false;
            tileCenter.transform.GetChild(i).transform.localPosition = allLocalPositions[i];
            tileCenter.transform.GetChild(i).GetComponent<TrailRenderer>().enabled = true;
        }
    }

    public Piece.ColorState GetCurrentPieceColor()
    {
        var pieceList = GetComponentsInChildren<Piece>().ToList();
        if (pieceList == null) return Piece.ColorState.NONE;
        return pieceList.First().colorState;
    }

    public int GetCurrentPieceCount()
    {
        var pieceList = GetComponentsInChildren<Piece>().ToList();
        if (pieceList == null) return 0;
        return pieceList.Count;
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
        HighlightTile(state);
    }

    public void SpawnPiece(GameObject prefab)
    {
        var pos = tileCenter.transform.localPosition;
        GameObject piece = Instantiate(prefab, pos, Quaternion.identity);
        piece.transform.parent = tileCenter.transform;
        piece.transform.localPosition = Vector3.zero;
        piece.transform.localPosition = allLocalPositions[currentPieces];
    }

    public void SpawnPiece(GameObject prefab, int value)
    {
        var pos = tileCenter.transform.localPosition;

        for (int i = 0 ; i < value ; i++) {
            GameObject piece = Instantiate(prefab, pos, Quaternion.identity);
            piece.transform.parent = tileCenter.transform;
            piece.transform.localPosition = Vector3.zero;
            piece.transform.localPosition = allLocalPositions[i];
        }
    }

    public void SpawnPiece(GameObject prefab, Piece.ColorState colorState, int value)
    {
        var pos = tileCenter.transform.localPosition;

        for (int i = 0 ; i < value ; i++) {
            GameObject piece = Instantiate(prefab, pos, Quaternion.identity);
            piece.GetComponent<Piece>().SetColorState(colorState);
            piece.transform.parent = tileCenter.transform;
            piece.transform.localPosition = Vector3.zero;
            piece.transform.localPosition = allLocalPositions[i];
        }
    }

    public void SpawnPiece(Tile tile, GameObject prefab, Piece.ColorState colorState, int value)
    {
        var pos = tileCenter.transform.localPosition;

        for (int i = 0 ; i < value ; i++) {
            GameObject piece = Instantiate(prefab, pos, Quaternion.identity);
            piece.GetComponent<Piece>().SetColorState(colorState);
            piece.GetComponent<Piece>().currentTile = tile;
            piece.transform.parent = tileCenter.transform;
            piece.transform.localPosition = Vector3.zero;
            piece.transform.localPosition = allLocalPositions[i];
        }
    }
}
