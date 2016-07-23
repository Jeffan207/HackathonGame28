using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class LevelGenerator : NetworkBehaviour {

    // full width, not half-width
    public float moduleWidth;

    public Module startModule;
    public Module[] straightModules;
    public Module[] cornerModules;

    private List<Module> currentModules;

    private int moduleLead = 8;

    public Arrow newArrowPrefab;

    void Awake()
    {
        currentModules = new List<Module>();
        
        ClientScene.RegisterPrefab(startModule.gameObject);
        foreach (Module module in straightModules)
        {
            ClientScene.RegisterPrefab(module.gameObject);
        }
        foreach (Module module in cornerModules)
        {
            ClientScene.RegisterPrefab(module.gameObject);
        }
    }

    public override void OnStartServer()
    {
        StartNewRound();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            AddNewModule();
        }
    }

    public void AddNewModule()
    {
        bool corner = Random.Range(0, 2) == 1;
        bool right = Random.Range(0, 2) == 1;

        ServerAddNewModule(Random.Range(0, cornerModules.Length), Random.Range(0, straightModules.Length), corner, right);
    }
    [Server]
    public void ServerAddNewModule(int cornerIndex, int straightIndex, bool corner, bool right)
    {
        Module latestModule = currentModules[currentModules.Count - 1];

        if (corner)
        {
            Module newModule = Instantiate(cornerModules[cornerIndex]);

            Arrow arrow = Instantiate(newArrowPrefab);
            arrow.transform.SetParent(newModule.transform);
            arrow.SetSprite(Arrow.Direction.RIGHT);

            newModule.transform.position = latestModule.transform.position + latestModule.outVector * moduleWidth;
            currentModules.Add(newModule);
            if (this.isServer)
            {
                newModule.playersEnter += AddNewModule;
                if (currentModules.Count > moduleLead)
                {
                    //currentModules[currentModules.Count - moduleLead - 1].playersEnter -= AddNewModule;
                }
            }

            Debug.LogFormat("Making corner from {0}", latestModule.outVector);

            //up
            if (latestModule.outVector.y > Mathf.Abs(latestModule.outVector.x))
            {
                //arrow.SetSprite(Arrow.Direction.UP);
                if (right)
                {
                    Debug.Log("Pointing right");
                    newModule.PointOutRight();
                }
                else
                {
                    Debug.Log("Pointing left");
                    newModule.PointOutLeft();
                }
            }
            // right
            else if (latestModule.outVector.x > 0)
            {
                //arrow.SetSprite(Arrow.Direction.RIGHT);
                Debug.Log("Pointing up from right");
                newModule.PointOutUp(true);
            }
            //left
            else
            {
                //arrow.SetSprite(Arrow.Direction.LEFT);
                Debug.Log("Pointing up from left");
                newModule.PointOutUp(false);
            }
            NetworkServer.Spawn(newModule.gameObject);

            arrow.transform.position = new Vector3(arrow.transform.position.x, arrow.transform.position.y, 2);
        }
        else
        {
            Module newModule = Instantiate(straightModules[straightIndex]);

            Arrow arrow = Instantiate(newArrowPrefab);
            arrow.transform.SetParent(newModule.transform);
            arrow.SetSprite(Arrow.Direction.UP);

            newModule.transform.position = latestModule.transform.position + latestModule.outVector * moduleWidth;
            currentModules.Add(newModule);
            //latestModule.playersEnter -= AddNewModule;
            newModule.playersEnter += AddNewModule;

            //up
            if (latestModule.outVector.y > Mathf.Abs(latestModule.outVector.x))
            {
                //arrow.SetSprite(Arrow.Direction.UP);
                newModule.PointOutUp(false);
            }
            // right
            else if (latestModule.outVector.x > 0)
            {
                //arrow.SetSprite(Arrow.Direction.RIGHT);
                newModule.PointOutRight();
            }
            //left
            else
            {
                //arrow.SetSprite(Arrow.Direction.LEFT);
                newModule.PointOutLeft();
            }
            NetworkServer.Spawn(newModule.gameObject);

            arrow.transform.position = new Vector3(arrow.transform.position.x, arrow.transform.position.y, 2);
        }
    }
    internal void StartNewRound()
    {
        foreach(Module module in currentModules)
        {
            if(module != null)
            {
                Destroy(module.gameObject);
            }
        }
        currentModules.Clear();

        Module newStart = Instantiate(startModule);
        currentModules.Add(newStart);
        newStart.playersEnter += AddNewModule;
        NetworkServer.Spawn(newStart.gameObject);
        for(int i = 0; i < moduleLead; i++)
        {
            AddNewModule();
        }
        AddNewModule();
    }
}
