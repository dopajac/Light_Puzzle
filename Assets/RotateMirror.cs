using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateMirror : MonoBehaviour
{
    [SerializeField] private GameObject Mirror;               // 회전시킬 실제 거울 오브젝트
    [SerializeField] private float rotationSpeed = 50f;       // 초당 회전 속도 (도 단위)
    
    public bool isRotating = false;

    private void Update()
    {
        if (isRotating && Mirror != null)
        {
            
            Mirror.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        isRotating = false;
    }
}