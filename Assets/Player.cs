using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {

    // player instance management
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
            return players[1];
        }
    }

    // movement properties
    [Header("Movement Properties")]
    public float WASDAcceleration = 2000;
    public float tapToMoveAcceleration = 80;
    private float currentAcceleration;
    private Vector3 currentDirection;
    public float maxSpeed = 20;

    private Rigidbody2D rb;

    // bounce properties
    [Header("Bounce Properties")]
    public float bounceMinRadius = 1;
    public float bounceForce = 10;

    // grapple properties
    [Header("Grapple Properties")]
    public Grapple grapplePrefab;
    private Grapple grappleInstance;

    private bool pivoting;
    private bool pulling;
    private Vector3 pivot;
    private float grappleTime;
    public float pullTime = 2;
    public float pullForce = 2000;

    // death properties
    [SyncVar]
    internal bool alive;

	//sprite properties
	public Sprite stillSprite;
	public Sprite moveSprite;

    [Header("")]
    public GameObject deathPrefab;
    public MeshRenderer myRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

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
                // TODO swipe to grapple, tap to disconnect grapple
                if (Input.GetMouseButtonDown(0))
                {
					this.gameObject.GetComponentInChildren <SpriteRenderer>().sprite = moveSprite;
                    // tap to shoot grapple
                    CmdSpawnGrapple(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position);

                    // tap to move
                    currentDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                    currentAcceleration = tapToMoveAcceleration;
				
                }

                // press space to drop grapple
                if (Input.GetButton("Jump"))
                {
                    if (grappleInstance != null)
                    {
                        CmdGrappleDisconnect();
                    }
                }

                // pulling = true for 1 second after the grapple lands
                if (pulling)
                {
                    //GetComponent<Rigidbody2D>().velocity += transform.position - pivot;
                    GetComponent<Rigidbody2D>().AddForce(-pullForce * Time.deltaTime * (transform.position - pivot).normalized);

                    // transition to pulling = false, pivoting = true after 1 second
                    if (Time.time - grappleTime > pullTime)
                    {
                        pulling = false;
                        pivoting = true;
                        SpawnRope();
                    }
					this.gameObject.GetComponentInChildren <SpriteRenderer>().sprite = stillSprite;
                }
                // in this phase, we are constrained by a rope of a certain length
                else if(pivoting)
                {
                    /*
                    if((transform.position - pivot).magnitude > wireLength)
                    {
                        Vector3 offset = (transform.position - pivot);
                        Vector3.ClampMagnitude(offset, wireLength);
                        transform.position = pivot + offset;
                    }
                    */
                }
                // in this phase, we are free to move with WASD and be propelled by tap-to-move
                else
                {
                    //rb.AddForce(currentDirection * currentAcceleration);
                    currentAcceleration = Mathf.Lerp(currentAcceleration, 0, Time.deltaTime / 0.1f);

                    GetComponent<Rigidbody2D>().AddForce(Input.GetAxis("Horizontal") * Vector3.right * WASDAcceleration * Time.deltaTime +
                                                         Input.GetAxis("Vertical") * Vector3.up * WASDAcceleration * Time.deltaTime);
                    GetComponent<Rigidbody2D>().velocity = Vector3.ClampMagnitude(GetComponent<Rigidbody2D>().velocity, maxSpeed);

                    // press space to push away other players
                    if (Input.GetButton("Jump"))
                    {
                        CmdBounce();
                    }
                }
            }

            // check death
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
    
    private void SpawnRope()
    {
        // TODO spawn a rope between this.transform.position and this.pivot
    }

    [Command]
    private void CmdSpawnGrapple(Vector3 direction)
    {
        if (grappleInstance != null)
        {
            NetworkServer.Destroy(grappleInstance.gameObject);
        }
        grappleInstance = Instantiate(grapplePrefab);
        grappleInstance.transform.position = this.transform.position;
        grappleInstance.myPlayer = this;

        NetworkServer.Spawn(grappleInstance.gameObject);
        grappleInstance.RpcFire(direction);
    }

    [Command]
    private void CmdGrappleDisconnect()
    {
        RpcGrappleDisconnect();
    }
    [ClientRpc]
    private void RpcGrappleDisconnect()
    {
        GrappleDisconnect();
    }
    private void GrappleDisconnect()
    {
        pulling = false;
        pivoting = false;

        Destroy(grappleInstance.gameObject);
    }

    [ClientRpc]
    internal void RpcGrappleConnect(Vector3 position)
    {
        if (this.hasAuthority)
        {
            Debug.Log("Grapple connected");
            grappleTime = Time.time;
            pivot = position;
            pivoting = true;
            pulling = true;
        }
    }
    [ClientRpc]
    internal void RpcGrapplePlayer(NetworkIdentity identity)
    {
        if (this.hasAuthority)
        {
            Player player = identity.GetComponent<Player>();
            // TODO player-player grappling
        }
    }


    public void OnGUI()
    {
        if (Debug.isDebugBuild)
        {
            GUI.Label(new Rect(100, 100, 100, 20), penultimatePlayer.transform.position.y.ToString());
        }
    }


    [Server]
    public void Respawn()
    {
        RpcRespawn();
    }
    [ClientRpc]
    void RpcRespawn()
    {
        Debug.Log("Player respawned");
        myRenderer.enabled = true;
        transform.position = Vector3.zero + Random.Range(-1f, 1f) * Vector3.up + Random.Range(-1f, 1f) * Vector3.right;
        alive = true;
        pulling = false;
        pivoting = false;
        GetComponent<Rigidbody2D>().isKinematic = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
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


    [Command]
    void CmdBounce()
    {
        RpcBounce();
    }
    [ClientRpc]
    void RpcBounce()
    {
        foreach(Player player in FindObjectsOfType<Player>())
        {
            Vector3 delta = (transform.position - player.transform.position);
            float amount = Mathf.Max(bounceMinRadius, delta.magnitude);
            player.GetComponent<Rigidbody2D>().AddForce(-delta.normalized * 1f/amount * bounceForce);
        }
    }
}
