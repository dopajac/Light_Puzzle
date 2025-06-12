using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LineSpawner : MonoBehaviour
{
    [Header("레이저 세팅")]
    public GameObject LazerPrefab;

    private static bool LazerInitialized = false;
    
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

    void Start()
    {
        StartCoroutine(InitializeLazersWithDelay());
    }

    IEnumerator InitializeLazersWithDelay()
    {
        if (!LazerInitialized)
        {
            yield return new WaitForSeconds(0.1f);

            for (int i = 0; i < 3; i++) // 3갈래 레이저 생성
            {
                GameObject laser = Instantiate(LazerPrefab, Vector3.zero, Quaternion.identity);
                laser.SetActive(true);
                DontDestroyOnLoad(laser);
            }

            LazerInitialized = true;
        }
    }

    private void Update()
    {
        if (Player == null)
        {
            Player = GameObject.Find("Player(Clone)");
        }
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