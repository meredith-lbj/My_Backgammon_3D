using UnityEngine;

public class Dice : MonoBehaviour
{
    private Rigidbody rb;
    private int upValue = 0;
    private bool isStuck = false;
    void Start()
    {
        rb = GetComponent<Rigidbody>();  
        isStuck = false; 
    }

    public bool GetIsStuck()
    {
        return isStuck;
    }

    public void SetIsStuck(bool value)
    {
        isStuck = value;
    }

    public void SetUpValue(int value)
    {
        upValue = value;
    }

    public int GetUpValue()
    {
        return upValue;
    }

}
