using UnityEngine;
using System.Collections;

public class DeathIndicator : MonoBehaviour {

    void Start()
    {
        transform.position = 20 * Vector3.down;
    }

    void Update()
    {
        if (Player.players != null && Player.players.Count > 0 && Player.localPlayer != null)
        {
            transform.position = new Vector3(Player.localPlayer.transform.position.x, Player.penultimatePlayer.transform.position.y - Player.penultimatePlayer.deathDistance, 0);
        }
    }
}
