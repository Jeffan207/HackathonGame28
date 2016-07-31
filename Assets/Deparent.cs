using UnityEngine;
using System.Collections;

public class Deparent : MonoBehaviour {

    private bool fired;

    public Transform[] transforms;

	void Update () {
	    if(!fired)
        {
            fired = true;
            foreach(Transform t in transforms)
            {
                t.SetParent(null);
            }
        }
	}
}
