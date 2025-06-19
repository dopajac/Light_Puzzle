using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnPlayer : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    private GameObject currentPlayer;

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(SpawnWithDelay());
    }

    private IEnumerator SpawnWithDelay()
    {
        yield return new WaitForSeconds(0.1f); // 씬 로딩 대기

        GameObject found = GameObject.Find("SpawnPoint");
        if (found != null)
        {
            spawnPoint = found.transform;
            Spawn();
        }
        else
        {
            Debug.LogError("SpawnPoint 오브젝트를 찾을 수 없습니다.");
        }
    }

    private void Spawn()
    {
        if (playerPrefab != null && spawnPoint != null)
        {
            if (currentPlayer == null)
            {
                // 플레이어가 아직 없으면 생성
                var player = currentPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
                DontDestroyOnLoad(player);
                
                FindObjectOfType<BulletSpawner>().SetPlayer(player);
                FindObjectOfType<LineSpawner>().SetPlayer(player);
                FindObjectOfType<CheckPlayerInLight>().SetPlayer(player);
            }
            else
            {
                // 이미 있으면 위치만 이동
                currentPlayer.transform.position = spawnPoint.position;
                currentPlayer.transform.rotation = spawnPoint.rotation;
            }
        }
        else
        {
            Debug.LogError("PlayerPrefab 또는 SpawnPoint가 설정되지 않았습니다.");
        }
    }
}