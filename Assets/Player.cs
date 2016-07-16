using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
    public float speed = 2;

	void Update () {
        if (this.isLocalPlayer)
        {
            transform.position += Input.GetAxis("Horizontal") * Vector3.right * speed * Time.deltaTime;
            transform.position += Input.GetAxis("Vertical") * Vector3.up * speed * Time.deltaTime;
        }
    }
}
