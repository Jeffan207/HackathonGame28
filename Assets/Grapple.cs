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

    public void OnTriggerEnter2D(Collider2D collision)
    {
        // check for walls layer
        if (collision.gameObject.layer == 8)
        {
            GetComponent<Rigidbody2D>().isKinematic = true;
            myPlayer.GrappleConnect(transform.position);
        }
        // check for player layer (make sure we aren't hitting our own player)
        if (collision.gameObject.layer == 10 && collision.gameObject.GetComponent<Player>() != null && !collision.gameObject.GetComponent<Player>().Equals(myPlayer))
        {
            GetComponent<Rigidbody2D>().isKinematic = true;
            transform.SetParent(collision.gameObject.transform);
            myPlayer.GrapplePlayer(collision.gameObject.GetComponent<Player>());
        }
    }
}
