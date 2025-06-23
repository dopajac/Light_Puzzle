using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlayerInLight : MonoBehaviour
{
    [SerializeField] private Transform lightOrigin;
    [SerializeField] private GameObject player;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private float detectionDelay = 1f; // 감지 지연 시간 (초)

    private bool canDetect = false;
    

    void Update()
    {
        if (player == null)
        {
            player = GameObject.Find("Player(Clone)");
        }
        

        Vector2 direction = player.transform.position - lightOrigin.position;
        float distance = direction.magnitude;
    
        RaycastHit2D hit = Physics2D.Raycast(lightOrigin.position, direction.normalized, distance, obstructionMask);

        if (hit.collider == null)
        {
            // Ray가 아무것도 막히지 않고 플레이어까지 닿음
            Debug.Log("빛에 노출됨! Game Over -> 대상: " + player.name);
            // GameOver 처리
        }
        else
        {
            // Debug: 어떤 오브젝트가 빛을 막았는지
            Debug.Log("빛이 " + hit.collider.name + "에 의해 막힘");
        }
    }
    public void SetPlayer(GameObject p)
    {
        player = p;
    }
}
