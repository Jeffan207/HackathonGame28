using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {

    public static List<Player> players;
    public static Player localPlayer;

    public float speed = 2;
    public float acceleration = 2;
    private float currentAcceleration;
    private Vector3 currentDirection;

    public float maxSpeed = 20;

    public float minRadius = 1;
    public float force = 5;

    public MeshRenderer myRenderer;

    public GameObject deathPrefab;

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
                    currentDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                    currentAcceleration = acceleration;
                }

                //currentAcceleration = Mathf.Max(currentAcceleration - acceleration * Time.deltaTime, 0);

                rb.AddForce(currentDirection * currentAcceleration);
                currentAcceleration = Mathf.Lerp(currentAcceleration, 0, Time.deltaTime / 0.1f);

                GetComponent<Rigidbody2D>().AddForce(Input.GetAxis("Horizontal") * Vector3.right * speed * Time.deltaTime +
                                                     Input.GetAxis("Vertical") * Vector3.up * speed * Time.deltaTime);
                GetComponent<Rigidbody2D>().velocity = Vector3.ClampMagnitude(GetComponent<Rigidbody2D>().velocity, maxSpeed);

                if (Input.GetButton("Jump"))
                {
                    CmdBounce();
                }


            }

            if (this.hasAuthority)
            {
                if (transform.position.y < -5)
                {
                    CmdLose();
                    alive = false;
                }
            }
        }
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
        bool alldead = true;
        foreach (Player player in Player.players)
        {
            if (player.alive)
            {
                alldead = false;
            }
        }
        if (alldead)
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
