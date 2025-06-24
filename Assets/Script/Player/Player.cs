using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    private Rigidbody2D rb;
    private bool isGrounded;

    [Header("Ladder Settings")]
    private bool isOnLadder = false;
    private bool isClimbing = false;
    public float climbSpeed = 3f;

    [Header("Shoot Settings")]
    public Transform respawnPoint;
    [SerializeField] private float bulletSpeed = 10f;
    private int count = 0;

    [Header("Laser & Light Pool")]
    public GameObject lightPrefab;
    private List<GameObject> activeLights = new List<GameObject>();
    private Queue<GameObject> lightPool = new Queue<GameObject>();
    private List<GameObject> activeBranches = new List<GameObject>();
    [SerializeField] private int maxLightCount = 100;

    [Header("Laser Settings")]
    [SerializeField] private LayerMask interactionLayers;
    [SerializeField] private int maxReflections = 5;
    [SerializeField] private float laserMaxDistance = 20f;

    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        InitLightPool();
    }

    void Update()
    {
        if (GameManager.Instance.stageNum == 5 &&
            LightFlicker.Instance != null && LightFlicker.Instance.IsLightOn)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        Move(); // 항상 호출

        if (isOnLadder)
            Climb();

        Jump();

        if (Input.GetMouseButtonDown(0)) Shoot();
        if (Input.GetMouseButton(1)) FireLaser();
        if (Input.GetMouseButtonUp(1)) DisableLaser();
    }

    void Move()
    {
        if (isClimbing) return; // 사다리 타는 중이면 좌우 이동 X

        float xInput = Input.GetAxisRaw("Horizontal");
        Vector2 velocity = rb.velocity;
        velocity.x = xInput * moveSpeed;
        rb.velocity = velocity;
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isClimbing)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
            animator?.SetTrigger("Jump");
        }
    }

    void Climb()
    {
        float vInput = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(vInput) > 0.1f)
        {
            isClimbing = true;
            rb.gravityScale = 0f;
            rb.velocity = new Vector2(rb.velocity.x, vInput * climbSpeed);
        }
        else
        {
            isClimbing = false;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ladder"))
        {
            isOnLadder = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("ladder"))
        {
            isOnLadder = false;
            isClimbing = false;
            rb.gravityScale = 1f;
            
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;

        if (collision.gameObject.CompareTag("Respawn2"))
        {
            Transform spawn = GameObject.Find("SpawnPoint")?.transform;
            if (spawn != null) transform.position = spawn.position;
        }
    }

    void InitLightPool()
    {
        for (int i = 0; i < maxLightCount; i++)
        {
            GameObject obj = Instantiate(lightPrefab);
            DontDestroyOnLoad(obj);
            obj.SetActive(false);
            lightPool.Enqueue(obj);
        }
    }

    void Shoot()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        if (GameManager.Instance.TryGetBullet(out GameObject bullet))
        {
            bullet.transform.position = transform.position;
            bullet.SetActive(true);
            bullet.GetComponent<Rigidbody2D>().velocity = direction * bulletSpeed;
            count = (count + 1) % GameManager.Instance.bullets.Count;
        }
    }

    void FireLaser()
    {
        DisableLaser();

        Vector3 origin = transform.position;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2 direction = (mouseWorldPos - origin).normalized;

        for (int i = 0; i < 3; i++)
        {
            LineSpawner.Instance.FireLaserBeam(origin, direction, i, ref activeLights, ref lightPool, activeBranches);
        }
    }

    void DisableLaser()
    {
        foreach (GameObject l in activeLights)
        {
            l.SetActive(false);
            lightPool.Enqueue(l);
        }
        activeLights.Clear();

        foreach (GameObject branch in activeBranches)
        {
            branch.SetActive(false);
            LineSpawner.Instance.RecycleLaser(branch);
        }
        activeBranches.Clear();
    }
}
