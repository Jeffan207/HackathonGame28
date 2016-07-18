using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

    private Transform target;

    public float smoothingRate;

    public void Follow(Transform newTarget)
    {
        target = newTarget;
    }
	
	void LateUpdate () {
	
        if(target != null)
        {
            //Vector3 delta = transform.position - target.position;
            transform.position = Vector3.Lerp(transform.position, new Vector3(target.position.x, target.position.y, this.transform.position.z) + Vector3.up * 2, Time.deltaTime/smoothingRate);//;
        }

	}
}
