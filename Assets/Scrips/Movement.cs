using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

	public float speed = 20;
	public Vector3 playerClick;
	public  float target;
	public Rigidbody2D rb;

	void Awake() {
		rb = GetComponent<Rigidbody2D> ();
		//target = Camera.main.ScreenToWorldPoint(playerClick);
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Mouse0)) {
			rb.velocity = Vector2.zero;
			playerClick = Input.mousePosition;
			Vector3 target = Camera.main.ScreenToWorldPoint (playerClick);
			Debug.Log (target);
			rb.AddForce (target * speed);

			//rb.AddForce (target );
		}
	}
}
