using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private int StageNum = 1;
    public List<GameObject> bullets = new List<GameObject>();
    
    void Awake()
    {
        // 싱글톤 중복 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);  // 씬 전환 시에도 유지
    }
    void Start() 
    {
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Bullet"), LayerMask.NameToLayer("Player"), true);
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
        Debug.Log("사용 가능 총알 x");
        return null; // 사용 가능한 총알 없음
    }
    public void Clear()
    {
        StageNum++;
    }
    public int GetStageNum()
    {
        return StageNum;
    }
}
