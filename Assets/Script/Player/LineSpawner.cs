using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LineSpawner : MonoBehaviour
{
    [Header("레이저 세팅")]
    public GameObject LazerPrefab;
    [SerializeField] private int laserPoolSize = 10;
    [SerializeField] private int portalLaserPoolSize = 10;
    [SerializeField] private int coreLaserPoolSize = 10;

    private Queue<GameObject> laserPool = new Queue<GameObject>();
    private Queue<GameObject> portalLaserPool = new Queue<GameObject>();
    private Queue<GameObject> coreLaserPool = new Queue<GameObject>();

    [Header("Core Light Settings")]
    [SerializeField] private GameObject lightPrefab;
    [SerializeField] private int coreLightPoolSize = 100;
    private Queue<GameObject> coreLightPool = new Queue<GameObject>();
    private List<GameObject> coreActiveLights = new List<GameObject>();
    private List<GameObject> coreActiveBranches = new List<GameObject>();

    public static LineSpawner Instance { get; private set; }

    [Header("플레이어 참조")]
    public GameObject Player;

    [SerializeField] private LayerMask interactionLayers;
    [SerializeField] private float laserMaxDistance = 10f;
    [SerializeField] private int maxReflections = 5;

    [Header("거울 관련")]
    private HashSet<RotateMirror> rotatingMirrors = new HashSet<RotateMirror>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        InitializeLaserPools();
        InitCoreLightPool();
    }

    void InitCoreLightPool()
    {
        for (int i = 0; i < coreLightPoolSize; i++)
        {
            GameObject obj = Instantiate(lightPrefab);
            obj.SetActive(false);
            coreLightPool.Enqueue(obj);
        }
    }

    void InitializeLaserPools()
    {
        for (int i = 0; i < laserPoolSize; i++)
        {
            GameObject obj = Instantiate(LazerPrefab);
            obj.SetActive(false);
            laserPool.Enqueue(obj);
        }
        for (int i = 0; i < portalLaserPoolSize; i++)
        {
            GameObject obj = Instantiate(LazerPrefab);
            obj.SetActive(false);
            portalLaserPool.Enqueue(obj);
        }
        for (int i = 0; i < coreLaserPoolSize; i++)
        {
            GameObject obj = Instantiate(LazerPrefab);
            obj.SetActive(false);
            coreLaserPool.Enqueue(obj);
        }
    }

    public GameObject GetCoreLaser()
    {
        if (coreLaserPool.Count > 0)
            return coreLaserPool.Dequeue();
        return Instantiate(LazerPrefab);
    }

    public void RecycleCoreLaser(GameObject obj)
    {
        obj.SetActive(false);
        coreLaserPool.Enqueue(obj);
    }

    public void FireLaserBeam(Vector2 origin, Vector2 direction, int type, ref List<GameObject> activeLights, ref Queue<GameObject> lightPool, List<GameObject> activeBranches)
    {
        List<Vector3> points = new List<Vector3> { origin };
        Vector2 currentPos = origin;
        Vector2 currentDir = direction;
        Collider2D lastCollider = null;
        HashSet<RotateMirror> currentHitMirrors = new HashSet<RotateMirror>();

        for (int i = 0; i < maxReflections; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPos, currentDir, laserMaxDistance, interactionLayers);
            if (hit.collider != null && hit.distance > 0.01f && hit.collider != lastCollider)
            {
                points.Add(hit.point);
                lastCollider = hit.collider;
                string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);

                if (layerName == "Reflect")
                    currentDir = Vector2.Reflect(currentDir, hit.normal);
                else if (layerName == "Refract")
                    currentDir = Refract(currentDir, hit.normal, 1f, 1.5f);
                else if (layerName == "Prism")
                {
                    if (type == 1) currentDir = Quaternion.Euler(0, 0, +15) * currentDir;
                    else if (type == 2) currentDir = Quaternion.Euler(0, 0, -15) * currentDir;
                    currentPos = hit.point + currentDir.normalized * 1.5f;
                    points.Add(currentPos);
                    continue;
                }
                else if (layerName == "Core")
                {
                    CoreController core = hit.collider.GetComponent<CoreController>();
                    if (core != null)
                        core.IncreaseIntensity(Time.deltaTime * 10f);
                    currentPos = hit.point + currentDir.normalized * 1.5f;
                    points.Add(currentPos);
                    break;
                }
                else if (layerName == "Portal")
                {
                    Portal portalComp = hit.collider.GetComponent<Portal>();
                    if (portalComp != null && portalComp.linkedPortal != null)
                    {
                        points.Add(hit.point);
                        Vector2 newOrigin = (Vector2)portalComp.linkedPortal.transform.position + currentDir.normalized * 0.7f;
                        GameObject portalLaserObj = portalLaserPool.Count > 0 ? portalLaserPool.Dequeue() : Instantiate(LazerPrefab);
                        portalLaserObj.SetActive(true);
                        activeBranches.Add(portalLaserObj);
                        FirePortalLaserBeam(newOrigin, currentDir, portalLaserObj, type, ref activeLights, ref lightPool);
                        break;
                    }
                }
                else if (layerName == "Grow")
                {
                    GrowObject grow = hit.collider.GetComponent<GrowObject>();
                    if (grow != null)
                        grow.IncreaseScale(Time.deltaTime);
                    break;
                }
                if (layerName == "MirrorRotation")
                {
                    RotateMirror rotateMirror = hit.collider.GetComponent<RotateMirror>();
                    if (rotateMirror != null)
                    {
                        rotateMirror.isRotating = true;
                        rotatingMirrors.Add(rotateMirror); // ✅ 통합 관리
                    }
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

        SpawnLightsAlongLaser(points, ref activeLights, ref lightPool);
    }

    public void FirePortalLaserBeam(Vector2 origin, Vector2 direction, GameObject laserObj, int type, ref List<GameObject> activeLights, ref Queue<GameObject> lightPool)
    {
        List<Vector3> points = new List<Vector3> { origin };
        Vector2 currentPos = origin;
        Vector2 currentDir = direction;
        Collider2D lastCollider = null;
        HashSet<RotateMirror> currentHitMirrors = new HashSet<RotateMirror>();

        for (int i = 0; i < maxReflections; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPos, currentDir, laserMaxDistance, interactionLayers);
            if (hit.collider != null && hit.distance > 0.01f && hit.collider != lastCollider)
            {
                points.Add(hit.point);
                lastCollider = hit.collider;
                string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);

                if (layerName == "Reflect")
                    currentDir = Vector2.Reflect(currentDir, hit.normal);
                else if (layerName == "Refract")
                    currentDir = Refract(currentDir, hit.normal, 1f, 1.5f);
                else if (layerName == "Prism")
                {
                    if (type == 1) currentDir = Quaternion.Euler(0, 0, +15) * currentDir;
                    else if (type == 2) currentDir = Quaternion.Euler(0, 0, -15) * currentDir;
                    currentPos = hit.point + currentDir.normalized * 1.5f;
                    points.Add(currentPos);
                    continue;
                }
                else if (layerName == "Core")
                {
                    CoreController core = hit.collider.GetComponent<CoreController>();
                    if (core != null)
                        core.IncreaseIntensity(Time.deltaTime * 10f);
                    currentPos = hit.point + currentDir.normalized * 1.5f;
                    points.Add(currentPos);
                    break;
                }
                else if (layerName == "Grow")
                {
                    GrowObject grow = hit.collider.GetComponent<GrowObject>();
                    if (grow != null)
                        grow.IncreaseScale(Time.deltaTime);
                    break;
                }
                if (layerName == "MirrorRotation")
                {
                    RotateMirror rotateMirror = hit.collider.GetComponent<RotateMirror>();
                    if (rotateMirror != null)
                    {
                        rotateMirror.isRotating = true;
                        rotatingMirrors.Add(rotateMirror); // ✅ 통합 관리
                    }
                }

                currentPos = hit.point + currentDir.normalized * 1.5f;
            }
            else
            {
                points.Add(currentPos + currentDir * laserMaxDistance);
                break;
            }
        }

        LineRenderer lr = laserObj.GetComponent<LineRenderer>();
        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());

        SpawnLightsAlongLaser(points, ref activeLights, ref lightPool);
    }

    public void RecycleLaser(GameObject laser)
    {
        laser.SetActive(false);
        laserPool.Enqueue(laser);
    }

    void SpawnLightsAlongLaser(List<Vector3> points, ref List<GameObject> activeLights, ref Queue<GameObject> lightPool)
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

    Vector2 Refract(Vector2 incident, Vector2 normal, float n1, float n2)
    {
        incident = incident.normalized;
        normal = normal.normalized;
        float r = n1 / n2;
        float cosI = -Vector2.Dot(normal, incident);
        float sinT2 = r * r * (1f - cosI * cosI);
        if (sinT2 > 1f)
            return Vector2.Reflect(incident, normal);
        float cosT = Mathf.Sqrt(1f - sinT2);
        return r * incident + (r * cosI - cosT) * normal;
    }

    private void Update()
    {
        if (Player == null)
            Player = GameObject.Find("Player(Clone)");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Player = GameObject.Find("Player(Clone)");
    }

    public void SetPlayer(GameObject p)
    {
        Player = p;
    }
}
