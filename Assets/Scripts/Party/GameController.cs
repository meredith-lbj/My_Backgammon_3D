using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController gameController;

    [Header("Roll Dices Data")]
    [SerializeField] float maxForce;
    [SerializeField] float maxRollForce;
    [SerializeField] GameObject rollDicesButton;
    [SerializeField] List<Transform> defaultDicesPosition;
    [SerializeField] List<Transform> startDicesPosition;
    public GameObject dicePrefab;
    private GameObject dice1;
    private GameObject dice2;
    private bool isDiceTurn = false;

    [Header("Canvas Data")]
    [SerializeField] GameObject playerOrderResult;
    [SerializeField] GameObject noMovePossible;
    [SerializeField] GameObject drawGame;
    [SerializeField] GameObject victoryView;
    public GameObject yellowOutZone;
    public GameObject redOutZone;
    public GameObject yellowKickSpawn;
    public GameObject redKickSpawn;
    [SerializeField] TMP_Text yellowKickCounterText;
    [SerializeField] TMP_Text redKickCounterText;
    public GameObject yellowOutSpawn;
    public GameObject redOutSpawn;
    [SerializeField] TMP_Text yellowOutCounterText;
    [SerializeField] TMP_Text redOutCounterText;
    [SerializeField] Image player1stat;
    [SerializeField] Image player2stat;
    [SerializeField] Sprite highlightSprite;
    private List<Player> players;
    private List<int> yellowJanRange = new List<int>() {19, 20, 21, 22, 23, 24};
    private List<int> redJanRange = new List<int>() {1, 2, 3, 4, 5, 6};
    private int dice1Value = 0;
    private int dice2Value = 0;
    private int nbPieceMove = 0;
    private int currentMoveDone = -1;
    private int playerTurn = -1;
    private bool isNextTurn = true;
    private bool canStartGame = false;
    private bool isGameOver = false;
    private int nbTiles = 24;
    private float timeToReroll = 3f;
    private float timer = 0f;

    void Awake()
    {
        gameController = this;
        isNextTurn = true;
        canStartGame = false;
        isGameOver = false;
        players = FindObjectsOfType<Player>().ToList();
    }

    void Update()
    {
        if (playerTurn != -1 && isGameOver == true) {
            StartCoroutine(DisplayPlayerVictoryResult(players[0].GetIsWinner() ? 1 : 2));
            return;
        }
        // if (playerTurn != -1 && players[0].canMove == false && players[1].canMove == false) { // TO DO : Remove ?
        //     if (!drawGame.activeSelf) drawGame.SetActive(true);
        //     return;
        // }
        if (isDiceTurn == true && playerTurn == -1) {
            HandleDiceRoll();
            return;
        }
        if (isDiceTurn == true && playerTurn != -1) {
            HandleDiceRoll();
            return;
        }
        if (canStartGame == true && isDiceTurn == false && playerTurn != -1 && isNextTurn == true) {
            HandleTurn();
        }

    }

    public int GetCurrentMoveDone()
    {
        return currentMoveDone;
    }

    public void ThrowDicesLogic()
    {
        isDiceTurn = true;
        isNextTurn = true;
        if (playerTurn == -1) {
            rollDicesButton.SetActive(false);
            RollDices(new List<Vector3>() { startDicesPosition[0].position, startDicesPosition[1].position}, true);
        } else {
            rollDicesButton.SetActive(false);
            RollDices(new List<Vector3>() { defaultDicesPosition[0].position, defaultDicesPosition[1].position}, false);
        }
    }

    public void StartGame()
    {
        playerOrderResult.SetActive(false);
        rollDicesButton.SetActive(true);
    }

    public void UpdatePlayerUi(int turn)
    {
        if (turn == 1) {
            player1stat.sprite = highlightSprite;
            player2stat.sprite = null;
        } else {
            player1stat.sprite = null;
            player2stat.sprite = highlightSprite;
        }
    }

    public void SkipTurn()
    {
        playerTurn = playerTurn == 1 ? 2 : 1;
        ResetMove();
        noMovePossible.SetActive(false);
        yellowKickCounterText.enabled = true;
        redKickCounterText.enabled = true;
        rollDicesButton.SetActive(true);
        UpdatePlayerUi(playerTurn);
    }

    public void CheckMoveIsPossible()
    {
        if (currentMoveDone == 0 || isGameOver) return;
        Debug.Log("ALOOOOOOOOOOOO : " + currentMoveDone);

        CanMoveOut();

        var pieceList = FindObjectsOfType<Piece>().ToList();

        if (!CanMovePlayer()) {
            Debug.Log("POURQUOI  ");
            noMovePossible.SetActive(true);
            yellowKickCounterText.enabled = false;
            redKickCounterText.enabled = false;
            pieceList.ForEach(piece => piece.canSelect = false);
            return;
        }
    }

    public void CheckPlayerIsWinner(Piece.ColorState colorState, int currentPieces, int maxPieces)
    {
        if (colorState == Piece.ColorState.YELLOW && currentPieces == maxPieces) {
            isGameOver = true;
            players[0].SetIsWinner(true);
        } else if (colorState == Piece.ColorState.RED && currentPieces == maxPieces) {
            isGameOver = true;
            players[1].SetIsWinner(true);
        }
    }
    public bool CanMoveOut()
    {
        var pieceList = FindObjectsOfType<Piece>().Where(piece => playerTurn == 1 ? piece.colorState == Piece.ColorState.YELLOW : piece.colorState == Piece.ColorState.RED).ToList();
        pieceList.RemoveAll(piece => piece.pieceState == Piece.PieceState.OUT);
        Debug.Log("EEEEEEEEEEEEEEH OH");

        if (pieceList.Any(piece => piece.pieceState != Piece.PieceState.TILED)) return false;
        Debug.Log("EEEEEEEEEEEEEEH OH 2");

        foreach (var piece in pieceList) {
            if (piece.colorState == Piece.ColorState.YELLOW && !yellowJanRange.Contains(piece.currentTile.index)) {
                Debug.Log("EEEEEEEEEEEEEEH OH 3"+ piece.currentTile.index);
                players[playerTurn - 1].canMoveOut = false;
                return false;
            } else if (piece.colorState == Piece.ColorState.RED && !redJanRange.Contains(piece.currentTile.index)) {
                Debug.Log("EEEEEEEEEEEEEEH OH 4 " + piece.currentTile.index);
                players[playerTurn - 1].canMoveOut = false;
                return false;
            }
        }
        players[playerTurn - 1].canMoveOut = true;
        Debug.Log("EEEEEEEEEEEEEEH OH 5");
        return true;
    }
    public bool CanMovePlayer()
    {
        var tileList = FindObjectsOfType<Tile>().ToList();
        var pieceList = FindObjectsOfType<Piece>().Where(piece => playerTurn == 1 ? piece.colorState == Piece.ColorState.YELLOW : piece.colorState == Piece.ColorState.RED).ToList();
        pieceList.RemoveAll(piece => piece.pieceState == Piece.PieceState.OUT);
        
        if (pieceList.Any(piece => piece.pieceState == Piece.PieceState.KICKED)) {
            pieceList.ForEach(piece => {if (piece.pieceState != Piece.PieceState.KICKED) {piece.canSelect = false;} else piece.canSelect = true; });
            var kickedPieceList = pieceList.Where(piece => piece.pieceState == Piece.PieceState.KICKED).ToList();
            if (CanMovePiece(tileList, kickedPieceList.First(), dice1Value, kickedPieceList.First().colorState == Piece.ColorState.YELLOW ? 0 : 25) ||
                CanMovePiece(tileList, kickedPieceList.First(), dice2Value, kickedPieceList.First().colorState == Piece.ColorState.YELLOW ? 0 : 25)) {
                return true;
            }
            return false;
        }

        if (CanMoveInTile(tileList, pieceList)) return true;

        if (CanMoveOutPlayer(tileList)) return true;

        return false;
    }

    bool CanMoveInTile(List<Tile> tileList, List<Piece> pieceList)
    {
        foreach (var piece in pieceList) {
            var position = piece.currentTile.GetComponent<Tile>().index;
            if (CanMovePiece(tileList, piece, dice1Value, position) ||
                CanMovePiece(tileList, piece, nbPieceMove == 4 ? dice1Value * 2 : dice2Value, position) ||
                CanMovePiece(tileList, piece, nbPieceMove == 4 ? dice1Value * 3 : dice1Value + dice2Value, position)) {
                Debug.Log("TROP BIZARRRRRRE ");
                return true;
            }
            if (nbPieceMove == 4 && CanMovePiece(tileList, piece, dice1Value * 4, position)) {
                Debug.Log("TROP BIZARRRRRRE 2");
                return true;
            }
        }
        return false;
    }

    bool CanMoveOutPlayer(List<Tile> tileList)
    {
        if (players[playerTurn - 1].canMoveOut) {
            var pieceList = FindObjectsOfType<Piece>().Where(piece => playerTurn == 1 ? piece.colorState == Piece.ColorState.YELLOW : piece.colorState == Piece.ColorState.RED).ToList();
            pieceList.RemoveAll(piece => piece.pieceState == Piece.PieceState.OUT);
            Debug.Log("TROP BIZARRRRRRE 5");
            if (CanMoveInTile(tileList, pieceList)) return false;

            var playerColor = players[playerTurn - 1].colorState;
            var fanTileList = tileList.FindAll(tile => playerColor == Piece.ColorState.YELLOW ? yellowJanRange.Contains(tile.index) : redJanRange.Contains(tile.index));
            if (playerColor == Piece.ColorState.YELLOW) fanTileList.OrderBy(tile => tile.index);
            if (playerColor == Piece.ColorState.RED) fanTileList.OrderByDescending(tile => tile.index);

            Tile tile = fanTileList.Find(tile => tile.GetCurrentPieceCount() > 0);
            Piece piece = tile.GetComponentsInChildren<Piece>().First();
            int position = tile.index;

            if (CanMoveOutPiece(piece, dice1Value, position) ||
                CanMoveOutPiece(piece, nbPieceMove == 4 ? dice1Value * 2 : dice2Value, position) ||
                CanMoveOutPiece(piece, nbPieceMove == 4 ? dice1Value * 3 : dice1Value + dice2Value, position)) {
                Debug.Log("TROP BIZARRRRRRE 3");
                return true;
            }
            if (nbPieceMove == 4 && CanMoveOutPiece(piece, dice1Value * 4, position)) {
                Debug.Log("TROP BIZARRRRRRE 4");
                return true;
            }
        }
        return false;
    }

    public bool IsPieceOnHigherTile(List<Tile> tileList, Piece piece)
    {
        if (players[playerTurn - 1].canMoveOut && CanMoveOutPlayer(tileList)) {
            var playerColor = players[playerTurn - 1].colorState;
            var fanTileList = tileList.FindAll(tile => playerColor == Piece.ColorState.YELLOW ? yellowJanRange.Contains(tile.index) : redJanRange.Contains(tile.index));
            if (playerColor == Piece.ColorState.YELLOW) {
                fanTileList = fanTileList.OrderBy(tile => tile.index).ToList();
                fanTileList.ForEach(tile => Debug.Log("ICI LAAAAAAAAA YELLOW : " + tile.index));
            } else {
                fanTileList = fanTileList.OrderByDescending(tile => tile.index).ToList();
                fanTileList.ForEach(tile => Debug.Log("ICI LAAAAAAAAA RED : " + tile.index));
            }

            Tile tile = fanTileList.Find(tile => tile.GetCurrentPieceCount() > 0);
            Debug.Log("TROP BIZARRRRRRE 4 : " + tile.index);
            if (piece.currentTile.index == tile.index) return true;
        }
        Debug.Log("TROP BIZARRRRRRE 5");
        return false;
    }

    public bool CanMoveOutPiece(Piece piece, int diceValue, int currentPosition)
    {
        int targetPosition = piece.colorState == Piece.ColorState.YELLOW ? currentPosition + diceValue : currentPosition - diceValue;

        if (targetPosition <= 0 || targetPosition >= nbTiles + 1) return true;

        return false;
    }

    public bool CanMovePiece(List<Tile> tileList, Piece piece, int diceValue, int currentPosition)
    {
        int targetPosition = piece.colorState == Piece.ColorState.YELLOW ? currentPosition + diceValue : currentPosition - diceValue;
        if (players[playerTurn - 1].canMoveOut) {
            Debug.Log("MOVE OU PART LA ");
            if (targetPosition < 0 || targetPosition > nbTiles + 1 || diceValue == 0) return false;
            Debug.Log("MOVE OU PART LA 1");
            if (targetPosition == 0 || targetPosition == nbTiles + 1) return true;
        } else {
            Debug.Log("MOVE OU PART LA 2");
            if (targetPosition < 1 || targetPosition > nbTiles) return false;
        }

        Tile targetTile = tileList.Find(tile => tile.index == targetPosition);
        Debug.Log("MOVE OU PART LA 4");

        if (targetTile.CanAddPiece(piece)) return true;
        Debug.Log("MOVE OU PART LA 5");
        return false;
    }

    public void UpdateOutPieceCounter()
    {
        var pieceList = FindObjectsOfType<Piece>().ToList().Where(piece => piece.colorState == players[playerTurn - 1].colorState && piece.pieceState == Piece.PieceState.OUT).ToList();
        int count = pieceList.Count;

        if (playerTurn == 1) {
            yellowOutCounterText.text = count.ToString();
        } else {
            redOutCounterText.text = count.ToString();
        }
    }

    public void UpdateKickedPieceCounter()
    {
        var pieceList = FindObjectsOfType<Piece>().ToList().Where(piece => playerTurn == 1 ? piece.colorState == Piece.ColorState.RED && piece.pieceState == Piece.PieceState.KICKED : piece.colorState == Piece.ColorState.YELLOW && piece.pieceState == Piece.PieceState.KICKED).ToList();
        int count = pieceList.Count;

        if (playerTurn == 1) {
            redKickCounterText.text = count.ToString();
            players[1].canMoveOut = false;
        } else {
            yellowKickCounterText.text = count.ToString();
            players[0].canMoveOut = false;
        }
    }

    public bool IsPieceKickedLeft()
    {
        var pieceList = FindObjectsOfType<Piece>().ToList().Where(piece => playerTurn == 1 ? piece.colorState == Piece.ColorState.YELLOW : piece.colorState == Piece.ColorState.RED).ToList();
        
        if (pieceList.Any(piece => piece.pieceState == Piece.PieceState.KICKED)) {
            int count = pieceList.FindAll(piece => piece.pieceState == Piece.PieceState.KICKED).ToList().Count;
            if (playerTurn == 1) {
                yellowKickCounterText.text = count.ToString();
            } else {
                redKickCounterText.text = count.ToString();
            }
            return true;
        }

        pieceList.ForEach(piece => piece.canSelect = true);
        if (playerTurn == 1) {
            yellowKickCounterText.text = "";
        } else {
            redKickCounterText.text = "";
        }
        return false;
    }

    private void HandleTurn()
    {
        var pieceList = FindObjectsOfType<Piece>().ToList();

        if (players[playerTurn - 1].canMoveOut == false && CanMoveOut() && CanMovePlayer()) {
            Debug.Log("JE PEUX MOVE OUT ICI ");
            players[playerTurn - 1].canMoveOut = true;
        } else {
            Debug.Log("DEFAULT MOVE FALSE OUT STATUS ");
            players[playerTurn - 1].canMoveOut = false;
        }

        if (players[playerTurn - 1].canMoveOut == false && !CanMovePlayer()) {
            players[playerTurn - 1].canMove = false;
            noMovePossible.SetActive(true);
            yellowKickCounterText.enabled = false;
            redKickCounterText.enabled = false;
            pieceList.ForEach(piece => piece.canSelect = false);
            isNextTurn = false;
            return;
        }

        if (players[playerTurn - 1].canMove == false) players[playerTurn - 1].canMove = true;

        if (currentMoveDone == 0) {
            playerTurn = playerTurn == 1 ? 2 : 1;
            UpdatePlayerUi(playerTurn);
            pieceList.ForEach(piece => piece.canSelect = false);
            rollDicesButton.SetActive(true);
            isNextTurn = false;
            Debug.Log("C' EST AU TOUR DE PLAYER : " + playerTurn);
            return;
        }

        if (playerTurn == 1) {
            if (!pieceList.Any(piece => piece.colorState == Piece.ColorState.YELLOW && piece.pieceState == Piece.PieceState.KICKED))
                pieceList.FindAll(piece => piece.colorState == Piece.ColorState.YELLOW).ForEach(piece => piece.canSelect = true);
            pieceList.FindAll(piece => piece.colorState == Piece.ColorState.RED).ForEach(piece => piece.canSelect = false);
        } else {
            pieceList.FindAll(piece => piece.colorState == Piece.ColorState.YELLOW).ForEach(piece => piece.canSelect = false);
            if (!pieceList.Any(piece => piece.colorState == Piece.ColorState.RED && piece.pieceState == Piece.PieceState.KICKED))
                pieceList.FindAll(piece => piece.colorState == Piece.ColorState.RED).ForEach(piece => piece.canSelect = true);
        }
        isNextTurn = false;
    }

    public int CalculNbPieceMove()
    {
        if (dice1Value == dice2Value) return 4;

        var pieceList = FindObjectsOfType<Piece>().ToList();

        if (playerTurn == 1 && pieceList.Any(piece => piece.colorState == Piece.ColorState.YELLOW && piece.pieceState == Piece.PieceState.KICKED) ||
            pieceList.Any(piece => piece.colorState == Piece.ColorState.RED && piece.pieceState == Piece.PieceState.KICKED)) {
            return 2;
        }
        return 3;
    }

    public int GetDiceValue(int value)
    {
        if (nbPieceMove == 4) {
            return dice1Value * value;
        }
        if (value == 1) {
            return dice1Value != 0 ? dice1Value : dice2Value;
        } else if (value == 2) {
            return dice2Value;
        } else if (value == 3) {
            return dice1Value + dice2Value;
        }
        return dice1Value + dice2Value;
    }

    public void UpdateAvailableMove(int distance)
    {
        if (currentMoveDone == 0) return;

        Debug.Log("JE CAPTE PAS CA : " + nbPieceMove);
        if (nbPieceMove == 4) {
            dice2Value = 0;
            currentMoveDone = distance % dice1Value == 0 ? currentMoveDone - distance / dice1Value : currentMoveDone - 1;
            ResetMove();
            Debug.Log("JE PASSE LA AVEC 4 ET IL RESTE " + currentMoveDone + " DISTANCE DE " + distance);
            return;
        }
        if (distance == dice1Value) {
            dice1Value = 0;
            currentMoveDone = currentMoveDone == 3 ? currentMoveDone - 2 : currentMoveDone - 1;
        } else if (distance == dice2Value) {
            dice2Value = 0;
            currentMoveDone = currentMoveDone == 3 ? currentMoveDone - 2 : currentMoveDone - 1;
        } else if (distance == dice1Value + dice2Value) {
            dice1Value = 0;
            dice2Value = 0;
            currentMoveDone = 0;
        } else {
            if (dice1Value != 0) {
                dice1Value = 0;
            } else {
                dice2Value = 0;
            }
            currentMoveDone--;
            if (dice1Value == 0 && dice2Value == 0) currentMoveDone = 0;
        }
        ResetMove();
        Debug.Log(" CURRENT MOVE LA : " + currentMoveDone + "DISTANCE : " + distance);
    }

    void ResetMove()
    {
        if (currentMoveDone == 0) {
            dice1Value = 0;
            dice2Value = 0;
            nbPieceMove = 0;
            isNextTurn = true;
        }
    }

    void HandleDiceRoll()
    {
        var diceComponent = dice1.GetComponent<Dice>();
        var diceComponent2 = dice2.GetComponent<Dice>();

        if (canStartGame && (diceComponent.GetIsStuck() || diceComponent2.GetIsStuck()) && timer > timeToReroll) {
            diceComponent.SetIsStuck(false);
            diceComponent2.SetIsStuck(false);
            RollDices(new List<Vector3>() { defaultDicesPosition[0].position, defaultDicesPosition[1].position}, false);
            timer = 0;
            return;
        }
        if (diceComponent.GetIsStuck() || diceComponent2.GetIsStuck()) timer += Time.deltaTime;
        
        if (diceComponent.GetUpValue() != 0 && diceComponent2.GetUpValue() != 0) {
            dice1Value = diceComponent.GetUpValue();
            dice2Value = diceComponent2.GetUpValue();
            Debug.Log("AAAAAAAAAAAAAAAAAAAH DICE VALUE " + diceComponent.GetUpValue());
            Debug.Log("AAAAAAAAAAAAAAAAAAAH DICE 2 VALUE " + diceComponent2.GetUpValue());
            isDiceTurn = false;
            if (playerTurn == -1 && dice1Value == dice2Value) {
                RollDices(new List<Vector3>() { startDicesPosition[0].position, startDicesPosition[1].position}, true);
                return;
            }
            if (playerTurn == -1 && dice1Value != dice2Value) {
                playerTurn = dice1Value > dice2Value ? 1 : 2;
                UpdatePlayerUi(playerTurn);
                StartCoroutine(DisplayPlayerOrderResult(dice1Value, dice2Value));
                return;
            }
            nbPieceMove = CalculNbPieceMove();
            Debug.Log("OH WOOOOW : " + nbPieceMove);
            currentMoveDone = nbPieceMove;
            if (canStartGame == false) canStartGame = true;
        }
    }

    IEnumerator DisplayPlayerOrderResult(int value1, int value2)
    {
        yield return new WaitForSeconds(1);
        var textList = playerOrderResult.GetComponentsInChildren<TMP_Text>();
        textList[1].text = "Joueur 1 : " + value1;
        textList[2].text = "Joueur 2 : " + value2;
        textList[3].text = "Le joueur " + playerTurn + " débute !";
        playerOrderResult.SetActive(true);
    }

    IEnumerator DisplayPlayerVictoryResult(int value)
    {
        var pieceList = FindObjectsOfType<Piece>().ToList();
        pieceList.ForEach(piece => piece.CanSelect(false));
        var tileList = FindObjectsOfType<Tile>().ToList();
        tileList.ForEach(tile => tile.CanSelect(false));
        var outZoneList = FindObjectsOfType<OutZone>().ToList();
        outZoneList.ForEach(outZone => outZone.CanSelect(false));

        yield return new WaitForSeconds(1);
        var textList = victoryView.GetComponentsInChildren<TMP_Text>();
        textList[0].text = "Partie terminée.\nLe joueur " + value + " a gagné !";
        victoryView.SetActive(true);
    }

    public void RollDices(List<Vector3> positions, bool init)
    {
        isDiceTurn = true;

        var dices = FindObjectsOfType<Dice>();
        foreach (var dice in dices) {
            Destroy(dice.gameObject);
        }

        dice1 = Instantiate(dicePrefab, positions[0], Random.rotation);
        dice2 = Instantiate(dicePrefab, positions[1], Random.rotation);

        Rigidbody rb1 = dice1.GetComponent<Rigidbody>();
        Rigidbody rb2 = dice2.GetComponent<Rigidbody>();

        rb1.velocity = Vector3.zero;
        rb2.velocity = Vector3.zero;

        float forceX = Random.Range(0, maxRollForce);
        float forceY = Random.Range(0, maxRollForce);
        float forceZ = Random.Range(0, maxRollForce);

        float forceX2 = Random.Range(0, maxRollForce);
        float forceY2 = Random.Range(0, maxRollForce);
        float forceZ2 = Random.Range(0, maxRollForce);

        rb1.AddForce(Vector3.forward * maxForce);
        if (!init) {
            rb2.AddForce(Vector3.forward * maxForce);
        } else {
            rb2.AddForce(Vector3.back * maxForce);
        }
        
        rb1.AddTorque(forceX, forceY, forceZ);
        rb2.AddTorque(forceX2, forceY2, forceZ2);
        
        AudioManager.instance.playSFXByName("dice_roll_2");
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
