using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Mirror"))
        {
            // 1. 입사 방향
            Vector2 inDirection = rb.velocity.normalized;

            // 2. 충돌 지점에서 법선 벡터
            Vector2 normal = collision.contacts[0].normal;

            // 3. 반사 방향 계산
            Vector2 reflectDir = Vector2.Reflect(inDirection, normal);

            // 4. velocity 강제 재설정
            rb.velocity = reflectDir * speed;

            // 5. (선택) 방향 회전도 반영하고 싶다면:
            float angle = Mathf.Atan2(reflectDir.y, reflectDir.x) * Mathf.Rad2Deg;
            rb.rotation = angle;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

