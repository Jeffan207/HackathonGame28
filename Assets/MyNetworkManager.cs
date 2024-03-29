﻿using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System.Collections.Generic;

public class MyNetworkManager : NetworkManager {

    public static MyNetworkManager instance;

    public LevelGenerator levelGeneratorPrefab;
    private LevelGenerator levelGenerator;

    internal float restartTime;

    public void Start()
    {
        instance = this;
    }


    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("OnStartServer");
    }

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
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log("OnServerDisconnect");
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

    public void NewGame()
    {
        //Network.Disconnect();

        restartTime = Time.time;

        Debug.Log("New game!");
        foreach(Player player in Player.players)
        {
            player.Respawn();
        }

        levelGenerator.StartNewRound();

    }

    public void LeaveGame()
    {
        try
        {
            this.StopClient();
        }
        catch (System.Exception e)
        {
            Debug.Log(e.StackTrace);
        }
        try
        {
            this.StopHost();
        }
        catch (System.Exception e)
        {
            Debug.Log(e.StackTrace);
        }

        if (Network.isClient)
        {
        }
        if (Network.isServer)
        {
        }

        Network.Disconnect();
    }

    public void Update()
    {
        if (NetworkServer.active)
        {
            if (levelGenerator == null)
            {
                levelGenerator = Instantiate(levelGeneratorPrefab);
                NetworkServer.Spawn(levelGenerator.gameObject);
                levelGenerator.StartNewRound();
            }
        }
    }
}
