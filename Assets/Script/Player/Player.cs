// Player.cs
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
    [SerializeField] private float laserMaxDistance = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        InitLightPool();
    }

    void Update()
    {
        Move();
        Jump();

        if (Input.GetMouseButtonDown(0)) Shoot();
        if (Input.GetMouseButton(1)) FireLaser();
        if (Input.GetMouseButtonUp(1)) DisableLaser();
    }

    void Move()
    {
        float xInput = Input.GetAxisRaw("Horizontal");
        Vector2 velocity = rb.velocity;
        velocity.x = xInput * moveSpeed;
        rb.velocity = velocity;
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) isGrounded = true;

        if (collision.gameObject.CompareTag("Respawn2"))
        {
            Transform spawn = GameObject.Find("SpawnPoint")?.transform;
            if (spawn != null) transform.position = spawn.position;
            else Debug.LogError("SpawnPoint 오브젝트를 찾을 수 없습니다.");
        }
    }

    void InitLightPool()
    {
        for (int i = 0; i < maxLightCount; i++)
        {
            GameObject obj = Instantiate(lightPrefab);
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
        else
        {
            Debug.Log("총알 없음");
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
