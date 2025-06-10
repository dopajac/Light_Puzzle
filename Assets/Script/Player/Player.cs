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
    private GameObject Lazer;
    private LineRenderer lineRenderer;

    public GameObject lightPrefab; // ✅ Light2D 프리팹
    private List<GameObject> activeLights = new List<GameObject>();
    private Queue<GameObject> lightPool = new Queue<GameObject>();
    private int maxLightCount = 20;
    
    [SerializeField] private LayerMask reflectLayer;
    [SerializeField] private int maxReflections = 5;
    [SerializeField] private float laserMaxDistance = 100f;

    
    
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //Light 오브젝트 풀링
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

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }

        if (Input.GetMouseButton(1))
        {
            FireLaser();
        }

        if (Input.GetMouseButtonUp(1))
        {
            DisableLaser();
        }
    }

    void Move()
    {
        float xInput = Input.GetAxisRaw("Horizontal"); // A,D or ←,→
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

    // 땅 체크 (Ground 태그가 붙은 오브젝트와 충돌할 때만)
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        
        if(collision.gameObject.CompareTag("Respawn2"))
        { 
            Transform respawnPoint = GameObject.Find("SpawnPoint")?.transform;
           if (respawnPoint != null)
           {
               transform.position = respawnPoint.transform.position;
           }
           else
           {
               Debug.LogError("SpawnPoint 오브젝트를 찾을 수 없습니다.");
           }
           
        }
    }
    
    void Shoot()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        GameObject bullet =  GameManager.Instance.bullets[count];
        bullet.transform.position = transform.position;
        bullet.SetActive(true);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = direction * bulletSpeed;

        count = (count + 1) % GameManager.Instance.bullets.Count;
    }

    void FireLaser()
    {
        GameObject laserObj = GameObject.Find("Lazer(Clone)");
        if (laserObj == null)
        {
            Debug.LogWarning("Lazer 오브젝트를 찾을 수 없습니다.");
            return;
        }

        LineRenderer lineRenderer = laserObj.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogWarning("Lazer 오브젝트에 LineRenderer가 없습니다.");
            return;
        }

        
        Vector3 origin = transform.position ;
        
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2 direction = (mouseWorldPos - (Vector3)origin).normalized;

        List<Vector3> points = new List<Vector3>();
        points.Add(origin);

        Vector2 currentPos = origin;
        Vector2 currentDir = direction;

        for (int i = 0; i < maxReflections; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPos, currentDir, laserMaxDistance, reflectLayer);
            if (hit.collider != null)
            {
                points.Add(hit.point);

                // ✅ 반사 방향 계산
                currentDir = Vector2.Reflect(currentDir, hit.normal);
                // ✅ 살짝 앞쪽으로 이동 (거울에 다시 붙는 현상 방지)
                currentPos = hit.point + currentDir * 0.01f;
               // if (hit.collider.CompareTag("Target"))
                //    break;
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

            int lightCount = Mathf.FloorToInt(distance / lightSpacing);
            for (int j = 0; j <= lightCount; j++)
            {
                if (lightPool.Count == 0)
                    return; // ✅ 더 이상 라이트 풀 없음 (30개 한도 초과 시 중단)

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
            lightPool.Enqueue(l); // 풀에 다시 넣음
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