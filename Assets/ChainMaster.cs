using UnityEngine;
using System.Collections;

public class ChainMaster : MonoBehaviour {
    Transform at;
    Transform bt;

    public void CreateChain(Transform grapple, Transform player)
    {
        GetComponent<Chain>().A.transform.position = grapple.position;
        GetComponent<Chain>().B.transform.position = player.position;
        GetComponent<Chain>().rebuildRope();
        at = grapple;
        bt = player;
    }

    void Update()
    {
        if (at != null && bt != null)
        {
            GetComponent<Chain>().A.transform.position = at.position;
            GetComponent<Chain>().B.transform.position = bt.position;
        }
    }
}
