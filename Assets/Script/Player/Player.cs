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
    public GameObject LazerPrefab;
    public GameObject lightPrefab;
    private LineRenderer lineRenderer;
    private List<GameObject> activeLights = new List<GameObject>();
    private Queue<GameObject> lightPool = new Queue<GameObject>();
    private Queue<GameObject> laserPool = new Queue<GameObject>();
    private List<GameObject> activeBranches = new List<GameObject>();
    [SerializeField] private int maxLightCount = 100;
    [SerializeField] private int laserPoolSize = 10;

    [Header("Laser Settings")]
    [SerializeField] private LayerMask interactionLayers;
    [SerializeField] private int maxReflections = 5;
    [SerializeField] private float laserMaxDistance = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        InitPools();
    }

    void Update()
    {
        Move();
        Jump();

        if (Input.GetMouseButtonDown(0)) Shoot();
        if (Input.GetMouseButton(1)) FireLaser();
        if (Input.GetMouseButtonUp(1)) DisableLaser();
    }

    // -------------------- Movement --------------------
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

    // -------------------- Object Pool Initialization --------------------
    void InitPools()
    {
        for (int i = 0; i < maxLightCount; i++)
        {
            GameObject obj = Instantiate(lightPrefab);
            obj.SetActive(false);
            lightPool.Enqueue(obj);
        }

        for (int i = 0; i < laserPoolSize; i++)
        {
            GameObject obj = Instantiate(LazerPrefab);
            obj.SetActive(false);
            laserPool.Enqueue(obj);
        }
    }

    // -------------------- Bullet Shooting --------------------
    void Shoot()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        GameObject bullet = GameManager.Instance.bullets[count];
        bullet.transform.position = transform.position;
        bullet.SetActive(true);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = direction * bulletSpeed;

        count = (count + 1) % GameManager.Instance.bullets.Count;
    }

    // -------------------- Main Laser Logic --------------------
    void FireLaser()
    {
        DisableLaser(); // 이전 레이저 + 라이트 제거

        Vector3 origin = transform.position;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2 direction = (mouseWorldPos - origin).normalized;

        for (int i = 0; i < 3; i++)
        {
            FireLaserBeam(origin, direction, i);
            // 이 안에서 각각 SpawnLightsAlongLaser 호출되니까 OK
        }
    }

     void FireLaserBeam(Vector2 origin, Vector2 direction, int type)
    {
        List<Vector3> points = new List<Vector3> { origin };
        Vector2 currentPos = origin;
        Vector2 currentDir = direction;
        Collider2D lastCollider = null;

        for (int i = 0; i < maxReflections; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPos, currentDir, laserMaxDistance, interactionLayers);
            if (hit.collider != null && hit.distance > 0.01f && hit.collider != lastCollider)
            {
                points.Add(hit.point);
                lastCollider = hit.collider;

                string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);

                if (layerName == "Reflect")
                {
                    currentDir = Vector2.Reflect(currentDir, hit.normal);
                }
                else if (layerName == "Refract")
                {
                    currentDir = Refract(currentDir, hit.normal, 1f, 1.5f);
                }
                else if (layerName == "Prism")
                {
                    if (type == 1)
                        currentDir = Quaternion.Euler(0, 0, +15) * currentDir;
                    else if (type == 2)
                        currentDir = Quaternion.Euler(0, 0, -15) * currentDir;

                    currentPos = hit.point + currentDir.normalized *1.5f;
                    points.Add(currentPos);
                    continue;
                }

                currentPos = hit.point + currentDir.normalized * 1.5f;
            }
            else
            {
                points.Add(currentPos + currentDir * laserMaxDistance);
                break;
            }
        }

        GameObject laserObj = laserPool.Count > 0 ? laserPool.Dequeue() : Instantiate(LazerPrefab);
        laserObj.SetActive(true);
        activeBranches.Add(laserObj);

        LineRenderer lr = laserObj.GetComponent<LineRenderer>();
        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());

        SpawnLightsAlongLaser(points);
    }

    // -------------------- Light Handling --------------------
    void SpawnLightsAlongLaser(List<Vector3> points)
    {
        float lightSpacing = 1.5f;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 start = points[i];
            Vector3 end = points[i + 1];
            Vector3 dir = (end - start).normalized;
            float distance = Vector3.Distance(start, end);
            int lightCount = Mathf.Max(1, Mathf.FloorToInt(distance / lightSpacing));

            for (int j = 0; j <= lightCount; j++)
            {
                if (lightPool.Count == 0) continue;

                Vector3 pos = start + dir * (j * lightSpacing);
                GameObject lightObj = lightPool.Dequeue();
                lightObj.transform.position = pos;
                lightObj.SetActive(true);
                activeLights.Add(lightObj);
            }
        }
    }

    void ClearOldLights()
    {
        foreach (GameObject l in activeLights)
        {
            l.SetActive(false);
            lightPool.Enqueue(l);
        }
        activeLights.Clear();
    }

    // -------------------- Cleanup --------------------
    void DisableLaser()
    {
        if (lineRenderer != null)
            lineRenderer.positionCount = 0;

        ClearOldLights();

        foreach (GameObject branch in activeBranches)
        {
            branch.SetActive(false);
            laserPool.Enqueue(branch);
        }
        activeBranches.Clear();
    }

    // -------------------- Physics Helpers --------------------
    Vector2 Refract(Vector2 incident, Vector2 normal, float n1, float n2)
    {
        incident = incident.normalized;
        normal = normal.normalized;

        float r = n1 / n2;
        float cosI = -Vector2.Dot(normal, incident);
        float sinT2 = r * r * (1f - cosI * cosI);

        if (sinT2 > 1f)
        {
            Debug.Log("전반사 발생 - 반사로 처리");
            return Vector2.Reflect(incident, normal);
        }

        float cosT = Mathf.Sqrt(1f - sinT2);
        Vector2 refracted = r * incident + (r * cosI - cosT) * normal;

        return refracted.magnitude < 0.001f ? Vector2.Reflect(incident, normal) : refracted;
    }
}
