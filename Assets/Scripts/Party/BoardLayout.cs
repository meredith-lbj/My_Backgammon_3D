using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardLayout : MonoBehaviour
{
    public int nbTilesPerSide = 6; // Number of triangles per side
    public Material transparentTileMaterial;
    public Material blueTileMaterial;
    public GameObject yellowPiecePrefab;
    public GameObject redPiecePrefab;
    private float xSpacing = 0.303f; // x space from board
    private float ySpacing = 0.303f; // y space from board
    private float tileSize = 0.046f; // tile size
    private float tileHeight = 0.16f; // tile height
    private List<Tile> tileList;
    void Awake()
    {
        GenerateTriangleTiles();
        InitPiecePosition();
    }

    void GenerateTriangleTiles()
    {
        // Upper left row
        GenerateTriangle(0, 12, xSpacing, ySpacing, 90);

        xSpacing = 0.303f;
        ySpacing = -0.0265f;

        // Upper right row
        GenerateTriangle(6, 6, xSpacing, ySpacing, 90);

        xSpacing = 0.303f;
        ySpacing = 0.303f;

        // Lower right row
        GenerateTriangle(12, 24, xSpacing, ySpacing, -90);

        xSpacing = 0.303f;
        ySpacing = -0.0265f;

        // Lower left row
        GenerateTriangle(18, 18, xSpacing, ySpacing, -90);

    }

    void GenerateTriangle(int indexRef, int tileIndex, float posXSpacing, float posYSpacing, int yAngle)
    {
        float posY = 0;
        float posX = 0;

        for (int i = 0 ; i < nbTilesPerSide ; i++) {
            GameObject tileObject = new GameObject($"Triangle_{i + indexRef}");
            tileObject.transform.parent = transform;

            Mesh mesh = new Mesh();
            tileObject.AddComponent<MeshFilter>().mesh = mesh;
            tileObject.AddComponent<MeshRenderer>().material = transparentTileMaterial;
            
            Vector3[] vertices = new Vector3[3]; // 3 vertices per triangle
            int[] triangles = new int[3]; // 3 indices per triangle

            vertices[0] = new Vector3(posX * tileSize + -posXSpacing, 0, posY * tileSize + -posYSpacing); // Left base
            vertices[1] = new Vector3(posX * tileSize + -posXSpacing, 0, (posY + 1) * tileSize + -posYSpacing); // Right base
            vertices[2] = new Vector3((posX + 1) * tileSize + tileHeight + -posXSpacing, 0, posY * tileSize + tileSize / 2 + -posYSpacing); // Tip

            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            GameObject tileCenterObject = new GameObject($"TileCenter");
            tileCenterObject.transform.parent = tileObject.transform;
            tileCenterObject.transform.localPosition = new Vector3((posX + 1) * tileSize + -posXSpacing - tileSize, 0, posY * tileSize + tileSize / 2 + -posYSpacing);

            tileObject.layer = LayerMask.NameToLayer("Tile");
            tileObject.AddComponent<BoxCollider>();
            tileObject.AddComponent<Tile>();

            var tile = tileObject.GetComponent<Tile>();
            tile.defaultTileMaterial = transparentTileMaterial;
            tile.highlightTileMaterial = blueTileMaterial;
            tile.index = tileIndex - i;

            tileObject.transform.localRotation = Quaternion.Euler(0, yAngle, 0);
            tileObject.transform.localPosition = new Vector3(0f, -0.015f, 0f);
            posY++;
        }
    }

    void InitPiecePosition()
    {
        tileList = FindObjectsOfType<Tile>().ToList();
        tileList.Reverse();

        tileList[0].SpawnPiece(tileList[0], yellowPiecePrefab, Piece.ColorState.YELLOW, 5);
        tileList[0].AddPiece(5);
        tileList[4].SpawnPiece(tileList[4], redPiecePrefab, Piece.ColorState.RED, 3);
        tileList[4].AddPiece(3);
        tileList[6].SpawnPiece(tileList[6], redPiecePrefab, Piece.ColorState.RED, 5);
        tileList[6].AddPiece(5);
        tileList[11].SpawnPiece(tileList[11], yellowPiecePrefab, Piece.ColorState.YELLOW, 2);
        tileList[11].AddPiece(2);

        tileList[12].SpawnPiece(tileList[12], redPiecePrefab, Piece.ColorState.RED, 2);
        tileList[12].AddPiece(2);
        tileList[17].SpawnPiece(tileList[17], yellowPiecePrefab, Piece.ColorState.YELLOW, 5);
        tileList[17].AddPiece(5);
        tileList[19].SpawnPiece(tileList[19], yellowPiecePrefab, Piece.ColorState.YELLOW, 3);
        tileList[19].AddPiece(3);
        tileList[23].SpawnPiece(tileList[23], redPiecePrefab, Piece.ColorState.RED, 5);
        tileList[23].AddPiece(5);
    }
}
