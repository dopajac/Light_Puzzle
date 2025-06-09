using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnPlayer : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] Transform spawnPoint;

    
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
        // 새 씬에서 SpawnPoint 찾아서 할당
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
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            }

            else
            {
                
            }

            
        }
        else
        {
            Debug.LogError("PlayerPrefab 또는 SpawnPoint가 설정되지 않았습니다.");
        }
    }
}