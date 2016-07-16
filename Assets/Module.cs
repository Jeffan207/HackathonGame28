using UnityEngine;
using System;

public class Module : MonoBehaviour {

    //straight: in bottom, out top
    //  corner: in bottom, out right

    internal event Action playersEnter;

    bool usedup;

    public void OnTriggerEnter2D(Collider2D col)
    {
        if (!usedup)
        {
            if (col.gameObject.layer == 10)
            {
                if (playersEnter != null)
                {
                    Debug.Log("Invoking playerEnter");
                    playersEnter.Invoke();
                    usedup = true;
                }
            }
        }
    }

    public bool corner;

    private Vector3 outvector = Vector3.up;

    public Vector3 outVector
    {
        get
        {
            return outvector;
        }
    }

    public void RotateCounterClockwise()
    {
        transform.rotation = transform.rotation * Quaternion.AngleAxis(90, Vector3.forward);
    }
    public void RotateClockwise()
    {
        transform.rotation = transform.rotation * Quaternion.AngleAxis(-90, Vector3.forward);
    }

    public void PointOutUp(bool fromRight)
    {
        if (corner)
        {
            RotateCounterClockwise();
            if(fromRight)
            {
                transform.localScale = new Vector3(1, -1, 1);
            }
        }
        outvector = Vector3.up;
    }
    public void PointOutRight()
    {
        if (!corner)
        {
            RotateClockwise();
        }
        outvector = Vector3.right;
    }
    public void PointOutLeft()
    {
        if (corner)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            RotateCounterClockwise();
        }
        outvector = Vector3.left;
    }
}
