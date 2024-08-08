using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceFaceDetector : MonoBehaviour
{
    private Dice diceParent;
    private Rigidbody rb;
    private Dictionary<string, int> diceFaces = new Dictionary<string, int>(){
        {"Un", 6},
        {"Deux", 5},
        {"Trois", 4},
        {"Quatre", 3},
        {"Cinq", 2},
        {"Six", 1}
    };
    void Awake()
    {
        diceParent = GetComponentInParent<Dice>();
        rb = GetComponentInParent<Rigidbody>();   
    }

    private void OnTriggerStay(Collider col)
    {
        if (rb.velocity == Vector3.zero && col.tag != "Board") {
            diceParent.SetIsStuck(true);
            return;
        }
        if (rb.velocity != Vector3.zero || col.tag != "Board") return;

        diceParent.SetUpValue(diceFaces[this.gameObject.name]);
        diceParent.SetIsStuck(false);
    }
}
