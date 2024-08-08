using UnityEngine;
public class Player : MonoBehaviour
{
    public int turn = -1;
    public Piece.ColorState colorState;
    [HideInInspector] public bool canMove = true;
    [HideInInspector] public bool canMoveOut = true;
    private bool isWinner = false;
    public bool GetIsWinner()
    {
        return isWinner;
    }

    public void SetIsWinner(bool state)
    {
        isWinner = state;
    }

}
