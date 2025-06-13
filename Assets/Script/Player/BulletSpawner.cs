using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BulletSpawner : MonoBehaviour
{
    [Header("총알 세팅")]
    public GameObject bulletPrefab;
    
    private static bool bulletsInitialized = false;
    
    
    [Header("플레이어 참조")]
    public GameObject Player;

    private void Awake()
    {
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

    private void Start()
    {
        // 총알 풀 초기화 (딱 한 번만)
        if (!bulletsInitialized)
        {
            for (int i = 0; i < 20; i++) // 필요하면 개수도 늘려
            {
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                bullet.SetActive(false); // ← 총알은 풀에 넣되 처음에는 비활성화
                DontDestroyOnLoad(bullet); // 씬 넘겨도 유지
                GameManager.Instance.bullets.Add(bullet);
            }
            bulletsInitialized = true;
        }
    }

    private void Update()
    {
        // 플레이어가 할당되지 않은 경우 계속 찾아줌
        if (Player == null)
        {
            Player = GameObject.Find("Player(Clone)");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 바뀔 때마다 플레이어 다시 탐색
        Player = GameObject.Find("Player(Clone)");
    }

    public void SetPlayer(GameObject p)
    {
        Player = p;
    }

   
}