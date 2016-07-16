using UnityEngine;
using System.Collections;

public class LevelGeneration : MonoBehaviour {

	public float maxHeight = 0f;
	public float currentHeight = 0f;

	public GameObject wall;
	public GameObject shortWall;

	// Use this for initialization
	void Start () {
		spawnWalls (10f);
		spawnWalls (4f);
		spawnFloor ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Player.penultimatePlayer != null) {
			if (Player.penultimatePlayer.transform.position.y > maxHeight + 5f) {
				maxHeight = Player.penultimatePlayer.transform.position.y + 5f;
				spawnWalls (0f);
			}
		}
	}

	void spawnWalls(float offset) {
		Instantiate(wall, new Vector2 (10f, maxHeight+15f - offset), Quaternion.identity);
		Instantiate(wall, new Vector2 (-10f, maxHeight+15f - offset), Quaternion.identity);
		spawnShortWall (offset);
		if (Random.Range (0f, 10f) < 5f) {
			spawnShortWall (offset);
		}
	}

	void spawnShortWall(float offset) {
		GameObject sw = Instantiate (shortWall, new Vector2 (Random.Range (-10f, 10f), maxHeight + 20f - offset), Quaternion.identity) as GameObject;
		sw.transform.Rotate(new Vector3(0f,0f,Random.Range(0f, 180f)));
	}

	void spawnFloor() {
		GameObject sw = Instantiate (shortWall, new Vector2 (0f, -5f), Quaternion.identity) as GameObject;
		sw.transform.localScale += new Vector3 (20f, 1f, 1f);
	}


}
