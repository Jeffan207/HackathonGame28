using UnityEngine;
using System.Collections;

public class DeathIndicator : MonoBehaviour {
    void Update()
    {
        if (Player.players != null && Player.players.Count > 0)
        {
            transform.position = new Vector3(Player.localPlayer.transform.position.x, Player.penultimatePlayer.transform.position.y - Camera.main.orthographicSize, 0);
        }
    }
}
