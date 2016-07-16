using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;

public class MenuHandler : MonoBehaviour {
	public MatchmakingManager matchmakingManager;

	public GameObject mainMenuPanel;
	public GameObject pauseMenuPanel;
	public GameObject hud;

	public void createGame() {
		matchmakingManager.createGame ();
		mainMenuPanel.SetActive(false);
		hud.SetActive (true);
	}
	public void joinGame() {
		matchmakingManager.joinGame ();
		mainMenuPanel.SetActive(false);
		hud.SetActive (true);
	}

	public void leaveGame() {
		Debug.Log ("Got here");
		matchmakingManager.leaveGame ();
	}

	public void enablePauseMenu() {
		if (!pauseMenuPanel.activeSelf) {
			pauseMenuPanel.SetActive (true);
		} else {
			pauseMenuPanel.SetActive (false);
		}
	}
}
