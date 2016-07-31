using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteController : MonoBehaviour {

    private SpriteRenderer sr;

	void Start ()
    {
        sr = GetComponent<SpriteRenderer>();
	}

    public void SetDirection(Vector2 face)
    {
        if (Mathf.Abs(transform.right.x) < 0.2 || Mathf.Abs(face.x) < 0.2)
        {
            return;
        }

        bool upsideDown = false;

        if(transform.right.x < 0)
        {
            upsideDown = true;
        }

        if(face.x > 0)
        {
            //unflip
            sr.flipX = upsideDown;
        }
        else
        {
            //flip
            sr.flipX = !upsideDown;
        }
    }

    public void Enable()
    {
        sr.enabled = true;
    }

    public void Disable()
    {
        sr.enabled = false;
    }
}
