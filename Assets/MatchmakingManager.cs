using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MatchmakingManager : MonoBehaviour {

    public GameObject playerPrefab;

    List<MatchDesc> matchList = new List<MatchDesc>();
    bool matchCreated;
    MyNetworkMatch networkMatch;

    void Awake()
    {
        networkMatch = gameObject.GetComponent<MyNetworkMatch>();

        ClientScene.RegisterPrefab(playerPrefab);
    }

	public void createGame() {
		CreateMatchRequest create = new CreateMatchRequest();
		create.name = "NewRoom";
		create.size = 4;
		create.advertise = true;
		create.password = "";
		networkMatch.CreateMatch(create, OnMatchCreate);
	}

	public void joinGame() {
		networkMatch.ListMatches(0, 20, "", OnMatchList);
	}

	public void leaveGame() {
		//This really doesn't work but its basically a placeholder until someone figures out how to do it right.
		SceneManager.LoadScene ("menu");
	}


    void OnGUI()
    {
        // You would normally not join a match you created yourself but this is possible here for demonstration purposes.

        if (GUILayout.Button("Local Game Start"))
        {
            GetComponent<MyNetworkManager>().StartHost();
            FindObjectOfType<Canvas>().enabled = false;
        }
        if (GUILayout.Button("Local Game Join"))
        {
            GetComponent<MyNetworkManager>().StartClient();
            FindObjectOfType<Canvas>().enabled = false;
        }

        if (matchList.Count > 0)
        {
            GUILayout.Label("Current rooms");
        }
        foreach (var match in matchList)
        {
            if (GUILayout.Button(match.name))
            {
                networkMatch.JoinMatch(match.networkId, "", OnMatchJoined);
            }
        }
    }

    public void OnMatchCreate(CreateMatchResponse matchResponse)
    {
        if (matchResponse.success)
        {
            Debug.Log("Create match succeeded");
            matchCreated = true;
            Utility.SetAccessTokenForNetwork(matchResponse.networkId, new NetworkAccessToken(matchResponse.accessTokenString));
            //NetworkServer.Listen(new MatchInfo(matchResponse), 9000);
            GetComponent<MyNetworkManager>().StartHost(new MatchInfo(matchResponse));
        }
        else
        {
            Debug.LogError("Create match failed");
        }
    }

    public void OnMatchList(ListMatchResponse matchListResponse)
    {
        if (matchListResponse.success && matchListResponse.matches != null)
        {
            if (matchListResponse.matches.Count > 0)
            {
                networkMatch.JoinMatch(matchListResponse.matches[0].networkId, "", OnMatchJoined);
            }
            else
            {
                Debug.LogWarning("No matchmaking rooms");
            }
        }
    }

    public void OnMatchJoined(JoinMatchResponse matchJoin)
    {
        if (matchJoin.success)
        {
            Debug.Log("Join match succeeded");
            if (matchCreated)
            {
                Debug.LogWarning("Match already set up, aborting...");
                return;
            }
            Utility.SetAccessTokenForNetwork(matchJoin.networkId, new NetworkAccessToken(matchJoin.accessTokenString));
            //NetworkClient myClient = new NetworkClient();
            //myClient.RegisterHandler(MsgType.Connect, OnConnected);
            //myClient.Connect(new MatchInfo(matchJoin));
            GetComponent<MyNetworkManager>().StartClient(new MatchInfo(matchJoin));
        }
        else
        {
            Debug.LogError("Join match failed");
        }
    }
    
    public void OnConnected(NetworkMessage msg)
    {
        Debug.Log("Connected!");
    }/*
        GameObject localPlayer = Instantiate(playerPrefab);

        NetworkServer.SpawnWithClientAuthority(localPlayer, msg.conn);
    }
    */
}

