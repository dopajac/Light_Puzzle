using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private LayerMask refractLayer;
    [SerializeField] private int maxReflections = 5;
    [SerializeField] private float laserMaxDistance = 100f;

    private HashSet<Vector2> prismHitPoints = new HashSet<Vector2>();
    private float prismHitThreshold = 0.05f;

    private GameObject[] prismBranches = new GameObject[3];
    private bool prismActive = false;
    private float prismAngleOffset = 15f;

    private List<GameObject>[] branchLights = new List<GameObject>[3];

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        for (int i = 0; i < 3; i++)
            branchLights[i] = new List<GameObject>();

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

    List<Vector3> points = new List<Vector3>();
    points.Add(origin);

    Vector2 currentPos = origin;
    Vector2 currentDir = direction;

    bool prismHit = false; // ✅ 추가: 프리즘 닿았는지 추적

    for (int i = 0; i < maxReflections; i++)
    {
        RaycastHit2D hit = Physics2D.Raycast(currentPos, currentDir, laserMaxDistance, reflectLayer | refractLayer);
        Debug.DrawRay(currentPos, currentDir * 2f, Color.cyan, 2f);
        if (hit.collider != null)
        {
            points.Add(hit.point);
            int layer = hit.collider.gameObject.layer;

            if (((1 << layer) & reflectLayer) != 0)
            {
                currentDir = Vector2.Reflect(currentDir, hit.normal);
                currentPos = hit.point + currentDir * 0.01f;
                continue;
            }
            else if (((1 << layer) & refractLayer) != 0)
            {
                Vector2 baseDir = Refract(currentDir, hit.normal, 1f, 1.5f);
                currentPos = hit.point + baseDir * 0.01f;

                // ✅ 프리즘에 닿은 경우만 분기 생성
                if (hit.collider.CompareTag("Prism"))
                {
                    prismHit = true;
                    UpdatePrismBranches(currentPos, baseDir);
                }

                break;
            }
        }
        else
        {
            float drawLength = 20f;
            points.Add(currentPos + currentDir * Mathf.Min(laserMaxDistance, drawLength));
            break;
        }
    }

    lineRenderer.positionCount = points.Count;
    lineRenderer.SetPositions(points.ToArray());
    SpawnLightsAlongLaser(points);

    // ✅ 프리즘에 닿지 않았으면 이전 분기 라인, 라이트 제거
    if (!prismHit && prismActive)
    {
        for (int i = 0; i < 3; i++)
        {
            if (prismBranches[i] != null)
                prismBranches[i].GetComponent<LineRenderer>().positionCount = 0;

            foreach (var l in branchLights[i])
            {
                if (l != null)
                {
                    l.SetActive(false);
                    lightPool.Enqueue(l);
                }
            }
            branchLights[i].Clear();
        }

        prismActive = false; // 상태도 꺼줘야 이후 프레임에도 안 그려짐
    }
}


    void UpdatePrismBranches(Vector2 origin, Vector2 baseDir)
    {
        if (!prismActive)
        {
            for (int i = 0; i < 3; i++)
            {
                if (prismBranches[i] == null)
                    prismBranches[i] = Instantiate(LazerPrefab);
                else
                    prismBranches[i].SetActive(true); // 재사용 시 활성화
            }
            prismActive = true;
        }

        Vector2[] dirs = new Vector2[]
        {
            Quaternion.Euler(0, 0, -prismAngleOffset) * baseDir,
            baseDir,
            Quaternion.Euler(0, 0, prismAngleOffset) * baseDir
        };

        for (int i = 0; i < 3; i++)
            UpdateLaserPath(prismBranches[i], origin, dirs[i], i);
    }

    void UpdateLaserPath(GameObject laser, Vector2 origin, Vector2 direction, int branchIndex)
    {
        foreach (var l in branchLights[branchIndex])
        {
            if (l != null)
            {
                l.SetActive(false);
                lightPool.Enqueue(l);
            }
        }
        branchLights[branchIndex].Clear();

        List<Vector3> points = new List<Vector3> { origin };
        Vector2 currentPos = origin;
        Vector2 currentDir = direction;
        float maxDrawDistance = 30f;
        float totalDrawnDistance = 0f;
        bool drewLine = false;

        for (int i = 0; i < maxReflections && totalDrawnDistance < maxDrawDistance; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPos, currentDir, laserMaxDistance, reflectLayer | refractLayer);
            if (hit.collider != null)
            {
                float segmentDist = Vector2.Distance(currentPos, hit.point);
                if (totalDrawnDistance + segmentDist > maxDrawDistance)
                {
                    float remain = maxDrawDistance - totalDrawnDistance;
                    points.Add(currentPos + currentDir * remain);
                    drewLine = true;
                    break;
                }

                points.Add(hit.point);
                drewLine = true;
                totalDrawnDistance += segmentDist;

                int layer = hit.collider.gameObject.layer;
                if (((1 << layer) & reflectLayer) != 0)
                {
                    currentDir = Vector2.Reflect(currentDir, hit.normal);
                    currentPos = hit.point + currentDir * 0.01f;
                    continue;
                }
                else if (((1 << layer) & refractLayer) != 0)
                {
                    Vector2 refracted = Refract(currentDir, hit.normal, 1f, 1.5f);
                    float remain = maxDrawDistance - totalDrawnDistance;
                    points.Add(hit.point + refracted * Mathf.Min(remain, laserMaxDistance));
                    drewLine = true;
                    break;
                }
            }
            else
            {
                float remain = Mathf.Min(maxDrawDistance - totalDrawnDistance, laserMaxDistance);
                points.Add(currentPos + currentDir * remain);
                drewLine = true;
                break;
            }
        }

        LineRenderer lr = laser.GetComponent<LineRenderer>();
        if (!drewLine || points.Count < 2)
        {
            lr.positionCount = 0;
            return;
        }

        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());

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
                branchLights[branchIndex].Add(lightObj);
            }
        }
    }

    Vector2 Refract(Vector2 incident, Vector2 normal, float n1, float n2)
    {
        incident = incident.normalized;
        normal = normal.normalized;

        float cosI = Vector2.Dot(normal, -incident);
        if (cosI < 0)
        {
            normal = -normal;
            cosI = Vector2.Dot(normal, -incident);
        }

        float r = n1 / n2;
        float sinT2 = r * r * (1f - cosI * cosI);

        if (sinT2 > 1f) return Vector2.Reflect(incident, normal);

        float cosT = Mathf.Sqrt(1f - sinT2);
        return r * incident + (r * cosI - cosT) * normal;
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

        if (prismActive)
        {
            for (int i = 0; i < 3; i++)
            {
                if (prismBranches[i] != null)
                    prismBranches[i].GetComponent<LineRenderer>().positionCount = 0;

                foreach (var l in branchLights[i])
                {
                    if (l != null)
                    {
                        l.SetActive(false);
                        lightPool.Enqueue(l);
                    }
                }
                branchLights[i].Clear();
            }
        }

        prismActive = false;
    }
}
