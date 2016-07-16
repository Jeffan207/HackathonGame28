using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour {

    // full width, not half-width
    public float moduleWidth;

    public Module startModule;
    public Module[] straightModules;
    public Module[] cornerModules;

    private List<Module> currentModules;

    private int moduleLead = 4;

    void Awake()
    {
        currentModules = new List<Module>();
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
        Module latestModule = currentModules[currentModules.Count - 1];

        bool corner = Random.Range(0, 2) == 1;

        if (corner)
        {
            Module newModule = Instantiate(cornerModules[Random.Range(0, cornerModules.Length)]);
            newModule.transform.position = latestModule.transform.position + latestModule.outVector * moduleWidth;
            currentModules.Add(newModule);
            newModule.playersEnter += AddNewModule;
            if (currentModules.Count > moduleLead)
            {
                //currentModules[currentModules.Count - moduleLead - 1].playersEnter -= AddNewModule;
            }

            Debug.LogFormat("Making corner from {0}", latestModule.outVector);

            //up
            if (latestModule.outVector.y > Mathf.Abs(latestModule.outVector.x))
            {
                bool right = Random.Range(0, 2) == 1;
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
                Debug.Log("Pointing up from right");
                newModule.PointOutUp(true);
            }
            //left
            else
            {
                Debug.Log("Pointing up from left");
                newModule.PointOutUp(false);
            }
        }
        else
        {
            Module newModule = Instantiate(straightModules[Random.Range(0, straightModules.Length)]);
            newModule.transform.position = latestModule.transform.position + latestModule.outVector * moduleWidth;
            currentModules.Add(newModule);
            latestModule.playersEnter -= AddNewModule;
            newModule.playersEnter += AddNewModule;

            //up
            if (latestModule.outVector.y > Mathf.Abs(latestModule.outVector.x))
            {
                newModule.PointOutUp(false);
            }
            // right
            else if (latestModule.outVector.x > 0)
            {
                newModule.PointOutRight();
            }
            //left
            else
            {
                newModule.PointOutLeft();
            }
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
        for(int i = 0; i < moduleLead; i++)
        {
            AddNewModule();
        }
        AddNewModule();
    }
}
