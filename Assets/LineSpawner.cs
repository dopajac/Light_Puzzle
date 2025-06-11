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
        StartCoroutine(InitializeLazerWithDelay());
    }

    IEnumerator InitializeLazerWithDelay()
    {
        if (!LazerInitialized)
        {
            yield return new WaitForSeconds(0.1f); // ✅ 0.1초 딜레이

            GameObject Lazer = Instantiate(LazerPrefab, new Vector3(0,0,0), Quaternion.identity);
            Lazer.SetActive(true); 
            DontDestroyOnLoad(Lazer);

            LazerInitialized = true;
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