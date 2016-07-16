using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
    public float speed = 2;
    
    public float maxSpeed = 20;

    public float minRadius = 1;
    public float force = 5;

	void Update () {
        if (this.isLocalPlayer)
        {
            GetComponent<Rigidbody2D>().AddForce(Input.GetAxis("Horizontal") * Vector3.right * speed * Time.deltaTime + 
                                                 Input.GetAxis("Vertical")   * Vector3.up    * speed * Time.deltaTime);
            GetComponent<Rigidbody2D>().velocity = Vector3.ClampMagnitude(GetComponent<Rigidbody2D>().velocity, maxSpeed);

            if (Input.GetButton("Jump"))
            {
                CmdBounce();
            }
        }
    }

    [Command]
    void CmdBounce()
    {
        Debug.Log("COMMAND Bounce");
        RpcBounce();
    }

    [ClientRpc]
    void RpcBounce()
    {
        Debug.Log("CLIENTRPC Bounce");
        foreach(Player player in FindObjectsOfType<Player>())
        {
            Vector3 delta = (transform.position - player.transform.position);
            float amount = Mathf.Max(minRadius, delta.magnitude);
            player.GetComponent<Rigidbody2D>().AddForce(-delta.normalized * 1f/amount * force);
        }
    }
}
