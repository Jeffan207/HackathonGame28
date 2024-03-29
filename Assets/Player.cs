﻿using UnityEngine;
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

    public Rigidbody2D rb;

    // bounce properties
    [Header("Bounce Properties")]
    public float bounceMinRadius = 1;
    public float bounceForce = 10;

    // grapple properties
    [Header("Grapple Properties")]
    public Grapple grapplePrefab;
    private Grapple grappleInstance;

    public LayerMask wallCheckMask;

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

    internal float deathDistance
    {
        get
        {
            return 2f * Camera.main.orthographicSize;
        }
    }

    //sprite properties
    public Sprite stillSprite;
	public Sprite moveSprite;

	//sound effects
	public AudioClip grappleHitSound;
	public AudioClip collisionSound;
	public AudioClip deathSound;
    public AudioSource astronautSoundSource;

    [Header("")]
    public GameObject deathPrefab;
    public SpriteController[] myRenderers;

    // rope management
    public ChainMaster chainPrefab;
    private ChainMaster chainInstance;

    void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        if (astronautSoundSource == null)
        {
            astronautSoundSource = GetComponent<AudioSource>();
        }
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
                Camera.main.GetComponent<CameraFollow>().Follow(rb.transform);
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

    void FixedUpdate()
    {
        foreach (SpriteController myRenderer in myRenderers)
        {
            myRenderer.SetDirection(rb.velocity);
        }
    }

    void Update () {
        // lame way of networking ragdoll primary rigidbody
        if(this.hasAuthority)
        {
            //this.transform.position = rb.transform.position;
            //this.transform.rotation = rb.transform.rotation;
        }
        else
        {
            Vector3 delta = this.transform.position - rb.transform.position;
            //rb.transform.position = this.transform.position;
            //rb.transform.rotation = this.transform.rotation;

            foreach(SpriteController bodyPart in myRenderers)
            {
                //bodyPart.transform.position += delta;
            }
        }

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
                                    //CmdSpawnGrapple(Camera.main.ScreenToWorldPoint(Input.mousePosition) - rb.position);

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
                        // tap to shoot grapple
                        CmdSpawnGrapple(Camera.main.ScreenToWorldPoint(Input.mousePosition) - rb.transform.position);

                        // tap to move
                        currentDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - rb.transform.position;
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
                    if(Vector3.Distance(rb.transform.position, pivot) > visualChainLength * 1.5f)
                    {
                        try
                        {
                            chainInstance.RebuildChain();
                            visualChainLength = visualChainLength * 1.5f;
                        }
                        catch (InvalidOperationException) { }
                    }

                    //rb.velocity += rb.transform.position - pivot;
                    if (Time.time - grappleTime < pullTime || Vector3.Distance(rb.transform.position, pivot) > grappleLength)
                    {
                        rb.AddForce(-pullForce * Time.deltaTime * (rb.transform.position - pivot).normalized);
                    }

                    // TODO check to update pivot
                    RaycastHit2D hit = Physics2D.Raycast(rb.transform.position, pivot - rb.transform.position, Vector3.Distance(pivot, rb.transform.position), wallCheckMask);

                    // transition to pulling = false, pivoting = true after 1 second
                    //if (rb.velocity.magnitude > 1.5f && Mathf.Abs(Vector2.Dot(rb.velocity.normalized, (rb.transform.position - pivot).normalized)) < 0.2)//  
                    if (false && Time.time - grappleTime > pullTime)
                    {
                        pulling = false;
                        pivoting = true;
                    }
                }
                // in this phase, we are constrained by a rope of a certain length
                else if(pivoting)
                {
                    /*
                    if((rb.transform.position - pivot).magnitude > wireLength)
                    {
                        Vector3 offset = (rb.transform.position - pivot);
                        Vector3.ClampMagnitude(offset, wireLength);
                        rb.transform.position = pivot + offset;
                    }
                    */
                }
                // in this phase, we are free to move with WASD and be propelled by tap-to-move
                else
                {
                    //rb.AddForce(currentDirection * currentAcceleration);
                    currentAcceleration = Mathf.Lerp(currentAcceleration, 0, Time.deltaTime / 0.1f);

                    rb.AddForce(Input.GetAxis("Horizontal") * Vector3.right * WASDAcceleration * Time.deltaTime +
                                Input.GetAxis("Vertical") * Vector3.up * WASDAcceleration * Time.deltaTime);
                    rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);

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
                    if (rb.transform.position.y < penultimatePlayer.transform.position.y - deathDistance)
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
            chainInstance.CreateChain(grappleInstance.transform, rb.transform, grapplePosition);
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
        grappleInstance.transform.position = rb.transform.position;
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
            grappleLength = Vector3.Distance(rb.transform.position, position);
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

            // player-player grappling
            rb.velocity += player.rb.velocity;
            CmdHaltPlayer(identity);
            CmdGrappleDisconnect();
        }
    }
    [Command]
    private void CmdHaltPlayer(NetworkIdentity identity)
    {
        Player player = identity.GetComponent<Player>();
        player.RpcHalt();
    }
    [ClientRpc]
    private void RpcHalt()
    {
        rb.velocity = Vector2.zero;
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
            rb.transform.position = Vector3.zero + UnityEngine.Random.Range(-1f, 1f) * Vector3.up + UnityEngine.Random.Range(-1f, 1f) * Vector3.right;
            this.transform.position = rb.transform.position;
            pulling = false;
            pivoting = false;
        }

        alive = true;

        foreach (SpriteController myRenderer in myRenderers)
        {
            myRenderer.Enable();
        }

        rb.isKinematic = false;
        rb.velocity = Vector2.zero;

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
            deathEffect.transform.position = rb.transform.position;

            astronautSoundSource.clip = deathSound;
            astronautSoundSource.Play();

            foreach (SpriteController myRenderer in myRenderers)
            {
                myRenderer.Disable();
            }

            alive = false;
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;

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
            Vector3 delta = (rb.transform.position - player.transform.position);
            float amount = Mathf.Max(bounceMinRadius, delta.magnitude);
            player.rb.AddForce(-delta.normalized * 1f/amount * bounceForce);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 8)
        {
            astronautSoundSource.clip = collisionSound;
            astronautSoundSource.Play();
        }
    }
}
