using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCheck : MonoBehaviour
{
    private bool isClinging = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Wall Jump")
        {
            isClinging = true;
            //Debug.Log("Player is on the ground");
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Wall Jump")
        {
            isClinging = false;
            //Debug.Log("Player is in the air");
        }
    }

    public bool IsClinging()
    {
        return isClinging;
    }
}
