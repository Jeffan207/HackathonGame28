﻿using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System.Collections;

public class MyNetworkManager : NetworkManager {

    public new void OnMatchJoined(JoinMatchResponse matchInfo)
    {
        Debug.Log("OnMatchJoined");
        if (matchInfo.success)
        {
            StartClient(new MatchInfo(matchInfo));
        }
    }
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.LogFormat("OnClientConnect {0}", conn.address);
    }
    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        Debug.Log("OnServerConnect");
    }
    public override void OnStartClient(NetworkClient client)
    {
        base.OnStartClient(client);
        Debug.Log("OnStartClient");
    }
    public override void OnStartHost()
    {
        base.OnStartHost();
        Debug.Log("OnStartHost");
    }
    public new void OnMatchCreate(CreateMatchResponse matchInfo)
    {
        Debug.Log("OnMatchCreate");
        if (matchInfo.success)
        {
            StartHost(new MatchInfo(matchInfo));
        }
    }
}