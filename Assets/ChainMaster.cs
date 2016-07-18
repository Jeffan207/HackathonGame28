using UnityEngine;
using System;

public class ChainMaster : MonoBehaviour {
    Transform at;
    Transform bt;

    private bool chainBuilt;

    public void CreateChain(Transform grapple, Transform player, Vector3 grapplePosition)
    {
        if (grapple == null)
        {
            Debug.LogWarning("in CreateChain(), grapple reference is null");
        }
        if (player == null)
        {
            Debug.LogWarning("in CreateChain(), player reference is null");
        }
        
        GetComponent<Chain>().A.transform.position = grapplePosition;
        GetComponent<Chain>().B.transform.position = player.position;

        RebuildChain();

        at = grapple;
        bt = player;
    }

    void Update()
    {
        if (at != null && bt != null)
        {
            GetComponent<Chain>().A.transform.position = at.position;
            GetComponent<Chain>().B.transform.position = bt.position;

            if(!chainBuilt)
            {
                RebuildChain();
            }
        }
    }

    internal void RebuildChain()
    {
        try
        {
            GetComponent<Chain>().rebuildRope();
            chainBuilt = true;
        }
        catch (ArgumentOutOfRangeException) { }
    }
}
