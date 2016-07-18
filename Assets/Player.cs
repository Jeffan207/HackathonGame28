using UnityEngine;
using System.Collections.Generic;
using System;
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

    public float grappleSpeed = 5;

    private float grappleLength;
    private float visualChainLength;

    private float lastTapTime;
    public float maxTimeForDoubleTap = 0.3f;
    private bool hasReleasedSinceLastTap;
    private bool hasDoubleTapped;
    // death properties
    [SyncVar]
    internal bool alive;
    private float lastRespawn;

    //sprite properties
    public Sprite stillSprite;
	public Sprite moveSprite;

	//sound effects
	public AudioClip grappleHitSound;
	public AudioClip collisionSound;
    private AudioSource astronautSoundSource;

    [Header("")]
    public GameObject deathPrefab;
    public SpriteRenderer myRenderer;

    // rope management
    public ChainMaster chainPrefab;
    private ChainMaster chainInstance;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
		astronautSoundSource = GetComponentInChildren<AudioSource> ();
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

    private bool swiping = false;
    private bool eventSent = false;
    private Vector2 lastPosition;

    void Update () {
        if (alive)
        {
            if (this.isLocalPlayer)
            {
                if (Input.touchCount > 0)
                {
                    if (Input.GetTouch(0).deltaPosition.sqrMagnitude != 0)
                    {
                        if (swiping == false)
                        {
                            swiping = true;
                            lastPosition = Input.GetTouch(0).position;
                            return;
                        }
                        else
                        {
                            if (!eventSent)
                            {
                                Vector2 direction = Input.GetTouch(0).position - lastPosition;
                                if (direction.magnitude > 75)
                                {
                                    Debug.Log("Swipe detected");
                                    eventSent = true;
                                    //CmdSpawnGrapple(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position);

                                    astronautSoundSource.clip = grappleHitSound;
                                    astronautSoundSource.pitch = UnityEngine.Random.Range(.8f, 1.2f);
                                    astronautSoundSource.Play();
                                    CmdSpawnGrapple(grappleSpeed * (Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position) - Camera.main.ScreenToWorldPoint(lastPosition)));
                                }
                            }
                        }
                    }
                    else {
                        if (!eventSent && grappleInstance != null)
                        {
                            lastTapTime = Time.time;
                            hasReleasedSinceLastTap = false;
                            CmdGrappleDisconnect();
                        }
                        else if (Time.time - lastTapTime < maxTimeForDoubleTap &&  
                                    hasReleasedSinceLastTap && 
                                    !hasDoubleTapped)
                        {
                            CmdAntiWall();
                            hasDoubleTapped = true;
                        }
                    }
                }
                else
                {
                    if (swiping && !eventSent)
                    {
                        if (grappleInstance != null)
                        {
                            lastTapTime = Time.time;
                            CmdGrappleDisconnect();
                        }
                    }
                    swiping = false;
                    eventSent = false;
                    hasReleasedSinceLastTap = true;

                    if (Time.time - lastTapTime > maxTimeForDoubleTap)
                    {
                        hasDoubleTapped = false;
                    }
                }
                // TODO swipe to grapple, tap to disconnect grapple
                if ((Debug.isDebugBuild || Application.platform != RuntimePlatform.Android) && Input.GetMouseButtonDown(0))
                {
                    if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    {
                        this.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = moveSprite;

                        // tap to shoot grapple
                        CmdSpawnGrapple(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position);

                        // tap to move
                        currentDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                        currentAcceleration = tapToMoveAcceleration;
                    }
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
                    if(Vector3.Distance(transform.position, pivot) > visualChainLength * 1.5f)
                    {
                        try
                        {
                            chainInstance.RebuildChain();
                            visualChainLength = visualChainLength * 1.5f;
                        }
                        catch (InvalidOperationException) { }
                    }

                    //GetComponent<Rigidbody2D>().velocity += transform.position - pivot;
                    if (Time.time - grappleTime < pullTime || Vector3.Distance(transform.position, pivot) > grappleLength)
                    {
                        GetComponent<Rigidbody2D>().AddForce(-pullForce * Time.deltaTime * (transform.position - pivot).normalized);
                    }

                    // TODO check to update pivot

                    // transition to pulling = false, pivoting = true after 1 second
                    //if (rb.velocity.magnitude > 1.5f && Mathf.Abs(Vector2.Dot(rb.velocity.normalized, (transform.position - pivot).normalized)) < 0.2)//  
                    if(false && Time.time - grappleTime > pullTime)
                    {
                        pulling = false;
                        pivoting = true;
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
    
    private void SpawnRope(Vector3 grapplePosition)
    {
        Debug.Log("Trying to spawn rope");
        if (grappleInstance != null)
        {
            chainInstance = Instantiate(chainPrefab);
            chainInstance.CreateChain(grappleInstance.transform, this.transform, grapplePosition);
        }
    }

    [Command]
    private void CmdSpawnGrapple(Vector3 direction)
    {
        if (grappleInstance != null)
        {
            Destroy(grappleInstance.gameObject);
        }

        grappleInstance = Instantiate(grapplePrefab);
        grappleInstance.transform.position = this.transform.position;
        grappleInstance.myPlayer = this;

        NetworkServer.Spawn(grappleInstance.gameObject);
        grappleInstance.RpcFire(direction);

        RpcSpawnGrapple(grappleInstance.GetComponent<NetworkIdentity>());
    }
    [ClientRpc]
    private void RpcSpawnGrapple(NetworkIdentity grapple)
    {
        grappleInstance = grapple.GetComponent<Grapple>();
        grappleInstance.myPlayer = this;
        if(chainInstance != null)
        {
            Destroy(chainInstance.gameObject);
        }
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

        if (chainInstance != null)
        {
            Debug.Log("Destroying chain");
            Destroy(chainInstance.gameObject);
        }
        if (grappleInstance != null)
        {
            Destroy(grappleInstance.gameObject);
        }
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
            grappleLength = Vector3.Distance(transform.position, position);
            visualChainLength = grappleLength;
        }

        if (chainInstance != null)
        {
            Destroy(chainInstance.gameObject);
        }
        SpawnRope(position);
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
        lastRespawn = Time.time;

        Debug.Log("Player respawned");
        if (this.hasAuthority)
        {
            transform.position = Vector3.zero + UnityEngine.Random.Range(-1f, 1f) * Vector3.up + UnityEngine.Random.Range(-1f, 1f) * Vector3.right;
            pulling = false;
            pivoting = false;
        }

        alive = true;
        myRenderer.enabled = true;

        GetComponent<Rigidbody2D>().isKinematic = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        if (grappleInstance != null)
        {
            Destroy(grappleInstance.gameObject);
        }
        if (chainInstance != null)
        {
            Destroy(chainInstance.gameObject);
        }
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
        if (Time.time - lastRespawn > 1f)
        {
            Debug.Log("Player lost");
            GameObject deathEffect = Instantiate(deathPrefab);
            deathEffect.transform.position = transform.position;
            myRenderer.enabled = false;
            alive = false;
            GetComponent<Rigidbody2D>().isKinematic = true;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;

            if (grappleInstance != null)
            {
                Destroy(grappleInstance.gameObject);
            }
            if (chainInstance != null)
            {
                Destroy(chainInstance.gameObject);
            }
            pulling = false;
            pivoting = false;
        }
    }

    [Command]
    void CmdAntiWall()
    {
        // TODO calculate nearest wall and repulsive force
        Vector2 force = Vector2.zero;
        RpcAntiWall(force);
    }
    [ClientRpc]
    void RpcAntiWall(Vector2 force)
    {
        rb.AddForce(force, ForceMode2D.Impulse);
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        astronautSoundSource.clip = collisionSound;
        astronautSoundSource.Play();
    }
}
