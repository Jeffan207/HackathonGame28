using UnityEngine;
using UnityEngine.Networking;
using System;

public class Module : NetworkBehaviour {

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
        outvector = Vector3.up;
        RpcPointOutUp(fromRight);
    }
    //[ClientRpc]
    public void RpcPointOutUp(bool fromRight)
    {
        if (corner)
        {
            RotateCounterClockwise();
            if(fromRight)
            {
                transform.rotation = Quaternion.AngleAxis(180, Vector3.up) * transform.rotation;
                //transform.localScale = new Vector3(1, -1, 1);
            }
        }
    }
    public void PointOutRight()
    {
        outvector = Vector3.right;
        RpcPointOutRight();
    }
    //[ClientRpc]
    public void RpcPointOutRight()
    {
        if (!corner)
        {
            RotateClockwise();
        }
    }
    public void PointOutLeft()
    {
        outvector = Vector3.left;
        RpcPointOutLeft();
    }
    //[ClientRpc]
    public void RpcPointOutLeft()
    {
        if (corner)
        {
            transform.rotation = Quaternion.AngleAxis(180, Vector3.up) * transform.rotation;
            //transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            RotateCounterClockwise();
        }
    }
}
