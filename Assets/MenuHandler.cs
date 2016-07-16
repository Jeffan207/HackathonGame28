using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;

public class MenuHandler : MonoBehaviour {
	public MatchmakingManager matchmakingManager;

	public void createGame() {
		matchmakingManager.createGame ();
		GetComponent<Canvas> ().enabled = false;
	}
	public void joinGame() {
		matchmakingManager.joinGame ();
		GetComponent<Canvas> ().enabled = false;
	}
}
