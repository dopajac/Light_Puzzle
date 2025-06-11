using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    private Rigidbody2D rb;
    private bool isGrounded;

    public Transform respawnPoint;
    [SerializeField] private float bulletSpeed = 10f;
    private int count = 0;

    public GameObject LazerPrefab;
    private LineRenderer lineRenderer;

    public GameObject lightPrefab;
    private List<GameObject> activeLights = new List<GameObject>();
    private Queue<GameObject> lightPool = new Queue<GameObject>();
    private int maxLightCount = 100;

    [SerializeField] private LayerMask reflectLayer;
    [SerializeField] private int maxReflections = 5;
    [SerializeField] private float laserMaxDistance = 100f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        for (int i = 0; i < maxLightCount; i++)
        {
            GameObject obj = Instantiate(lightPrefab);
            obj.SetActive(false);
            lightPool.Enqueue(obj);
        }
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
            Transform respawnPoint = GameObject.Find("SpawnPoint")?.transform;
            if (respawnPoint != null) transform.position = respawnPoint.position;
            else Debug.LogError("SpawnPoint 오브젝트를 찾을 수 없습니다.");
        }
    }

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

    void FireLaser()
{
    GameObject laserObj = GameObject.Find("Lazer(Clone)");
    if (laserObj == null || laserObj.GetComponent<LineRenderer>() == null)
    {
        Debug.LogWarning("Lazer 오브젝트를 찾을 수 없거나 LineRenderer 없음");
        return;
    }

    lineRenderer = laserObj.GetComponent<LineRenderer>();

    Vector3 origin = transform.position;
    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    mouseWorldPos.z = 0f;
    Vector2 direction = (mouseWorldPos - origin).normalized;

    List<Vector3> points = new List<Vector3> { origin };
    Vector2 currentPos = origin;
    Vector2 currentDir = direction;

    Collider2D lastCollider = null;

    HashSet<Collider2D> recentHits = new HashSet<Collider2D>();

    for (int i = 0; i < maxReflections; i++)
    {
        RaycastHit2D hit = Physics2D.Raycast(currentPos, currentDir, laserMaxDistance, reflectLayer);

        if (hit.collider != null && !recentHits.Contains(hit.collider) && hit.distance > 0.01f)
        {
            // ✅ 여기서 히트 위치가 너무 가까운지 확인
            if (Vector2.Distance(currentPos, hit.point) < 0.05f)
            {
                Debug.Log("무한 루프 방지: Hit 위치 너무 가까움");
                break;
            }

            points.Add(hit.point);
            string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
            Debug.Log($"Hit {hit.collider.name} | Layer: {layerName} | Dir: {currentDir}");

            if (layerName == "Refract")
            {
                currentDir = Refract(currentDir, hit.normal, 1f, 1.5f);
            }
            else if (layerName == "Reflect")
            {
                currentDir = Vector2.Reflect(currentDir, hit.normal);
            }

            currentPos = hit.point + currentDir.normalized * 0.1f;
        }
        else
        {
            points.Add(currentPos + currentDir * laserMaxDistance);
            break;
        }
    }

    lineRenderer.positionCount = points.Count;
    lineRenderer.SetPositions(points.ToArray());
    SpawnLightsAlongLaser(points);
}
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
            return Vector2.Reflect(incident, normal); // 반사 fallback
        }

        float cosT = Mathf.Sqrt(1f - sinT2);
        Vector2 refracted = r * incident + (r * cosI - cosT) * normal;
        if (refracted.magnitude < 0.001f)
        {
            Debug.Log("굴절 방향 너무 작음 - 반사로 대체");
            return Vector2.Reflect(incident, normal);
        }

        return refracted.normalized;
    }
    void SpawnLightsAlongLaser(List<Vector3> points)
    {
        ClearOldLights();
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

    void DisableLaser()
    {
        if (lineRenderer != null)
            lineRenderer.positionCount = 0;

        ClearOldLights();
    }
}
