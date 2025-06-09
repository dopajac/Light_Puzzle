using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;  // 만든 총알 프리팹
    public GameObject Player;  // 만든 총알 프리팹
    [SerializeField]private float bulletSpeed = 10f;

    private List<GameObject> bullets = new List<GameObject>();
    
    public int count = 0; // 총알 인덱스
    private void Start()
    {
        for(int i = 0; i < 10; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullets.Add(bullet);
        }
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 왼쪽 클릭
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // 1. 마우스 클릭 위치를 월드 좌표로 변환
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // 2. 방향 벡터 구하기
        Vector2 direction = (mouseWorldPos - Player.transform.position).normalized;
        
        // 3. 총알에 힘 주기 (Rigidbody2D 필요)
        GameObject bullet = bullets[count];
        bullet.gameObject.SetActive(true);
        
        bullet.transform.position = Player.transform.position;
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = direction * bulletSpeed;
        
        count++;

        if (count == 10)
        {
            count = 0; // 총알을 다 쏘면 다시 처음부터
        }

    }
}