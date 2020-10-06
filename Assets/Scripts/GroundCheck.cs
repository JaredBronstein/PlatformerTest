using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private bool isInAir = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Environment")
        {
            isInAir = false;
            //Debug.Log("Player is on the ground");
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Environment")
        {
            isInAir = true;
            //Debug.Log("Player is in the air");
        }
    }

    public bool IsInAir()
    {
        return isInAir;
    }
}
