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

    public void Clear()
    {
        StageNum++;
    }
    public int GetStageNum()
    {
        return StageNum;
    }
}
