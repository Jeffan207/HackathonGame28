using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;

public class MenuHandler : MonoBehaviour {
	public MatchmakingManager matchmakingManager;
	public MyNetworkManager networkManager;

	public GameObject mainMenuPanel;
	public GameObject pauseMenuPanel;
	public GameObject hud;

    public void Start()
    {
        mainMenuPanel.SetActive(true);
		pauseMenuPanel.SetActive (false);
		hud.SetActive (false);
    }

    public void createGame() {
		matchmakingManager.createGame ();
		mainMenuPanel.SetActive(false);
		hud.SetActive (true);
	}
	public void listGames() {
		matchmakingManager.listGames ();
	}

    public void joinGame()
    {
        mainMenuPanel.SetActive(false);
        hud.SetActive(true);
    }

    public void leaveGame() {
		Debug.Log ("Got here");
        mainMenuPanel.SetActive(true);
		pauseMenuPanel.SetActive (false);
		hud.SetActive (false);
        networkManager.LeaveGame();
    }

	public void enablePauseMenu() {
		if (!pauseMenuPanel.activeSelf) {
			pauseMenuPanel.SetActive (true);
		} else {
			pauseMenuPanel.SetActive (false);
		}
	}
}
