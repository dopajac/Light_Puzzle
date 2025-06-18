using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    private Rigidbody2D rb;

    private GameObject lastPassedPortal = null;
    private float portalIgnoreTime = 0.5f; 
    
    private GameObject CollisionObject;
    void Start()
    {
        
    }

    
    void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        
        StartCoroutine(AutoDisable(3f)); // 3초 후 자동으로 꺼짐
    }

    IEnumerator AutoDisable(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("충돌 발생 with: " + collision.gameObject.name);
        Debug.Log("레이어: " + LayerMask.LayerToName(collision.gameObject.layer));
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("Reflect"))
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
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Refract"))
        {
            Debug.Log("반사 충돌");
            Vector2 inDirection = rb.velocity.normalized;
            Vector2 normal = collision.contacts[0].normal;

            float n1 = 1.0f;  // 입사 매질 (공기 등)
            float n2 = 1.5f;  // 굴절 매질 (유리 등)

            float cosI = -Vector2.Dot(normal, inDirection);
            float sinT2 = (n1 / n2) * Mathf.Sqrt(1f - cosI * cosI);

            if (sinT2 <= 1f) // 전반사 아닌 경우
            {
                float cosT = Mathf.Sqrt(1f - sinT2 * sinT2);
                Vector2 refractDir = (n1 / n2) * inDirection + (n1 / n2 * cosI - cosT) * normal;
                refractDir.Normalize();

                rb.velocity = refractDir * speed;

                // 방향 회전 적용 (선택)
                float angle = Mathf.Atan2(refractDir.y, refractDir.x) * Mathf.Rad2Deg;
                rb.rotation = angle;
            }
            else
            {
                // 전반사 상황: 그냥 반사시켜도 됨
                Vector2 reflectDir = Vector2.Reflect(inDirection, normal);
                rb.velocity = reflectDir * speed;
                float angle = Mathf.Atan2(reflectDir.y, reflectDir.x) * Mathf.Rad2Deg;
                rb.rotation = angle;
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Prism"))
        {
            Debug.Log("prism in");
            Vector2 inDirection = rb.velocity.normalized;

            // 여기서 충돌한 대상 저장
            CollisionObject = collision.gameObject;

            float angleOffset = 15f;

            for (int i = -1; i <= 1; i++)
            {
                float angle = Mathf.Atan2(inDirection.y, inDirection.x) * Mathf.Rad2Deg;
                float newAngle = angle + angleOffset * i;
                Vector2 newDir = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad)).normalized;

                GameObject newBullet = GameManager.Instance.GetPooledBullet();
                if (newBullet != null)
                {
                    newBullet.transform.position = transform.position;
                    newBullet.transform.rotation = Quaternion.Euler(0, 0, newAngle);
                    newBullet.SetActive(true);

                    Rigidbody2D bulletRb = newBullet.GetComponent<Rigidbody2D>();
                    bulletRb.velocity = newDir * speed;

                    Collider2D bulletCol = newBullet.GetComponent<Collider2D>();
                    Collider2D targetCol = CollisionObject.GetComponent<Collider2D>();

                    if (bulletCol != null && targetCol != null)
                    {
                        Physics2D.IgnoreCollision(bulletCol, targetCol, true);
                        StartCoroutine(ReenableCollisionAfterDelay(bulletCol, targetCol, 3f));
                    }
                }
            }
            CollisionObject = null;
            gameObject.SetActive(false);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Portal"))
        {
            if (collision.gameObject == lastPassedPortal)
            {
                // 최근에 나온 포탈이면 무시
                Debug.Log("최근 포탈 재충돌 무시됨");
                return;
            }

            Debug.Log("Portal 충돌!");

            Portal portal = collision.gameObject.GetComponent<Portal>();
            if (portal != null && portal.linkedPortal != null)
            {
                Vector2 inDirection = rb.velocity.normalized;

                // 연결된 포탈 위치로 이동
                Vector2 exitPos = portal.linkedPortal.transform.position;
                transform.position = exitPos + inDirection * 0.5f;

                rb.velocity = inDirection * speed;

                float angle = Mathf.Atan2(inDirection.y, inDirection.x) * Mathf.Rad2Deg;
                rb.rotation = angle;

                // 방금 나온 포탈 저장
                lastPassedPortal = portal.linkedPortal.gameObject;
                StartCoroutine(ResetLastPortalAfterDelay(portalIgnoreTime));
            }
        }
    }
    
    IEnumerator ResetLastPortalAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        lastPassedPortal = null;
    }
    IEnumerator ReenableCollisionAfterDelay(Collider2D bulletCol, Collider2D targetCol, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bulletCol != null && targetCol != null)
        {
            Physics2D.IgnoreCollision(bulletCol, targetCol, false);
        }
    }
}

