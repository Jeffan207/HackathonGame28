using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;

public class Player : NetworkBehaviour {

    public Grapple grapplePrefab;
    private Grapple grappleInstance;

    public static List<Player> players;
    public static Player localPlayer;

    public static Player penultimatePlayer
    {
        get
        {
            if(players.Count == 0)
            {
                return null;
            }
            if(players.Count == 1)
            {
                return players[0];
            }
            players.Sort(delegate (Player a, Player b) {
                return a.transform.position.y.CompareTo(b.transform.position.y);
            });
            return players[players.Count - 1];
        }
    }

    public float speed = 2;
    public float acceleration = 2;
    private float currentAcceleration;
    private Vector3 currentDirection;

    public float maxSpeed = 20;

    public float minRadius = 1;
    public float force = 5;

    public MeshRenderer myRenderer;

    public GameObject deathPrefab;

    private bool pivoting;
    private bool pulling;
    private Vector3 pivot;
    private float angularMomentum;
    private float grappleTime;
    public float pullTime = 2;
    private float wireLength;
    public float pullForce = 2000;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //target = Camera.main.ScreenToWorldPoint(playerClick);
    }

    public override void OnStartServer()
    {
    }

    [SyncVar]
    public bool alive;

    void Start()
    {
        if(players == null)
        {
            players = new List<Player>();
        }
        players.Add(this);
        Debug.LogFormat("Number of players: {0}", players.Count);

        alive = true;

        if(this.isLocalPlayer)
        {
            localPlayer = this;

            if (Camera.main.GetComponent<CameraFollow>() != null)
            {
                Camera.main.GetComponent<CameraFollow>().Follow(this.transform);
            }
            else
            {
                Debug.LogWarning("The main camera doesn't have a CameraFollow!");
            }
        }

    }

	void Update () {
        if (alive)
        {
            if (this.isLocalPlayer)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if(grappleInstance != null)
                    {
                        GrappleDisconnect();
                    }
                    SpawnGrapple(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position);
                    //currentDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                    //currentAcceleration = acceleration;
                }

                if (Input.GetButton("Jump"))
                {
                    if (grappleInstance != null)
                    {
                        GrappleDisconnect();
                    }
                }

                if (pulling)
                {
                    //GetComponent<Rigidbody2D>().velocity += transform.position - pivot;
                    GetComponent<Rigidbody2D>().AddForce(-pullForce * Time.deltaTime * (transform.position - pivot).normalized);



                    if (Time.time - grappleTime > pullTime)
                    {
                        pulling = false;
                        pivoting = true;
                        wireLength = (transform.position - pivot).magnitude;
                    }
                }
                else if(pivoting)
                {
                    if((transform.position - pivot).magnitude > wireLength)
                    {
                        Vector3 offset = (transform.position - pivot);
                        Vector3.ClampMagnitude(offset, wireLength);
                        transform.position = pivot + offset;
                    }

                }
                else
                {


                    //currentAcceleration = Mathf.Max(currentAcceleration - acceleration * Time.deltaTime, 0);

                    rb.AddForce(currentDirection * currentAcceleration);
                    currentAcceleration = Mathf.Lerp(currentAcceleration, 0, Time.deltaTime / 0.1f);

                    GetComponent<Rigidbody2D>().AddForce(Input.GetAxis("Horizontal") * Vector3.right * speed * Time.deltaTime +
                                                         Input.GetAxis("Vertical") * Vector3.up * speed * Time.deltaTime);
                    GetComponent<Rigidbody2D>().velocity = Vector3.ClampMagnitude(GetComponent<Rigidbody2D>().velocity, maxSpeed);

                    if (Input.GetButton("Jump"))
                    {
                        CmdBounce();
                        //CmdDisconnectGrapple();
                    }
                }
            }

            if (this.isServer)
            {
                if (Time.time - MyNetworkManager.instance.restartTime > 3)
                {
                    if (transform.position.y < penultimatePlayer.transform.position.y - Camera.main.orthographicSize)
                    {
                        CmdLose();
                        alive = false;
                    }
                }
            }
        }
    }
    
    private void SpawnGrapple(Vector3 direction)
    {
        grappleInstance = Instantiate(grapplePrefab);
        grappleInstance.transform.position = this.transform.position;
        grappleInstance.myPlayer = this;
        grappleInstance.Fire(direction);
    }

    internal void GrappleConnect(Vector3 position)
    {
        grappleTime = Time.time;
        pivot = position;
        pivoting = true;
        pulling = true;
        angularMomentum = (transform.position - position).magnitude * rb.velocity.magnitude;
    }

    private void GrappleDisconnect()
    {
        Debug.Log("disconnect");
        pulling = false;
        pivoting = false;
        GetComponent<HingeJoint2D>().enabled = false;

        Destroy(grappleInstance.gameObject);
    }

    public void OnGUI()
    {
        GUI.Label(new Rect(100, 100, 100, 20), penultimatePlayer.transform.position.y.ToString());
    }

    [Server]
    public void Respawn()
    {
        RpcRespawn();
    }

    [Command]
    void CmdLose()
    {
        RpcLose();

        alive = false;

        Debug.Log("Checking if all players are dead");
        int aliveplayers = 0;
        foreach (Player player in Player.players)
        {
            if (player.alive)
            {
                aliveplayers++;
            }
        }
        if (aliveplayers < 2)
        {
            FindObjectOfType<MyNetworkManager>().NewGame();
        }
    }

    [ClientRpc]
    void RpcLose()
    {
        Debug.Log("Player lost");
        GameObject deathEffect = Instantiate(deathPrefab);
        deathEffect.transform.position = transform.position;
        myRenderer.enabled = false;
        alive = false;
        GetComponent<Rigidbody2D>().isKinematic = true;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
    }

    [ClientRpc]
    void RpcRespawn()
    {
        Debug.Log("Player respawned");
        myRenderer.enabled = true;
        transform.position = Vector3.zero;
        alive = true;
        GetComponent<Rigidbody2D>().isKinematic = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
    }

    [Command]
    void CmdBounce()
    {
        Debug.Log("COMMAND Bounce");
        RpcBounce();
    }

    [ClientRpc]
    void RpcBounce()
    {
        Debug.Log("CLIENTRPC Bounce");
        foreach(Player player in FindObjectsOfType<Player>())
        {
            Vector3 delta = (transform.position - player.transform.position);
            float amount = Mathf.Max(minRadius, delta.magnitude);
            player.GetComponent<Rigidbody2D>().AddForce(-delta.normalized * 1f/amount * force);
        }
    }
}
