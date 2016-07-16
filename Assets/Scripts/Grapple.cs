using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class Grapple : NetworkBehaviour
{
    public Player myPlayer;

    public float speed;

    public void Start()
    {
        Debug.LogFormat("New grapple (is server = {0})", this.isServer);
    }

    [ClientRpc]
    internal void RpcFire(Vector3 direction)
    {
        GetComponent<Rigidbody2D>().velocity = direction * speed;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        // check for walls layer
        if (collision.gameObject.layer == 8)
        {
            GetComponent<Rigidbody2D>().isKinematic = true;
            if (this.isServer)
            {
                myPlayer.RpcGrappleConnect(transform.position);
            }
        }
        // check for player layer (make sure we aren't hitting our own player)
        if (collision.gameObject.layer == 10 && collision.gameObject.GetComponent<Player>() != null && !collision.gameObject.GetComponent<Player>().Equals(myPlayer))
        {
            GetComponent<Rigidbody2D>().isKinematic = true;
            transform.SetParent(collision.gameObject.transform);
            if (this.isServer)
            {
                myPlayer.RpcGrapplePlayer(collision.gameObject.GetComponent<NetworkIdentity>());
            }
        }
    }
}
