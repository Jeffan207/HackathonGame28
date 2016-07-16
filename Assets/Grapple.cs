using UnityEngine;
using System.Collections;
using System;

public class Grapple : MonoBehaviour
{
    public Player myPlayer;

    public float speed;

    internal void Fire(Vector3 direction)
    {
        GetComponent<Rigidbody2D>().velocity = direction * speed;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        // check for walls layer
        if(collision.gameObject.layer == 8)
        {
            GetComponent<Rigidbody2D>().isKinematic = true;
            myPlayer.GrappleConnect(transform.position);
        }
    }
}
