using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int stageNum = 1;
    public List<GameObject> bullets = new List<GameObject>();

    public int TargetCount; // 현재 맞춘 타겟 수
    [FormerlySerializedAs("targetMaxCount")] public int TargetMaxCount; // 씬에 존재하는 타겟 총 수

    public bool StageisGateOpen = true;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 변경에도 유지
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Bullet"), LayerMask.NameToLayer("Player"), true);
        TargetCount = 0;
        CountTargetsInScene();
        CheckGateOpen();
    }

    private void Start()
    {
        CheckGateOpen();
    }

    private void CountTargetsInScene()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        TargetMaxCount = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Target") &&
                obj.hideFlags == HideFlags.None &&
                obj.scene.IsValid())
            {
                Debug.Log($"[Debug] Found: {obj.name}, Scene: {obj.scene.name}, Active: {obj.activeInHierarchy}");
                TargetMaxCount++;
            }
        }

        Debug.Log($"Target 포함 오브젝트 수: {TargetMaxCount}");
    }

    public bool TryGetBullet(out GameObject bullet)
    {
        bullet = GetPooledBullet();
        return bullet != null;
    }

    public GameObject GetPooledBullet()
    {
        foreach (GameObject bullet in bullets)
        {
            if (!bullet.activeInHierarchy)
                return bullet;
        }

        Debug.Log("사용 가능 총알 없음");
        return null;
    }

    public void Clear()
    {
        stageNum++;
    }

    public int GetStageNum()
    {
        return stageNum;
    }

    public int GetTargetMaxCount()
    {
        return TargetMaxCount;
    }

    public bool CheckGateOpen()
    {
        if (GameManager.Instance.TargetCount >= GameManager.Instance.GetTargetMaxCount())
        {
            StageisGateOpen = true;
            Debug.Log("게이트가 열렸습니다.");
        }
        else
        {
            StageisGateOpen = false;
        }
        return StageisGateOpen;
    }

}