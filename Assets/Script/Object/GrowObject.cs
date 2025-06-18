using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowObject : MonoBehaviour
{
    private float MaxGrow = 0.7f;
    private float MinGrow = 0.2f;
    private float growSpeed = 0.5f;
    private float shrinkSpeed = 0.2f;

    private float lastHitTime = -10f; // 마지막으로 레이저에 맞은 시간

    [SerializeField] private GameObject lightPrefab;

    private void Start()
    {
        lightPrefab.SetActive(false);
    }

    public void IncreaseScale(float amount)
    {
        lastHitTime = Time.time;

        Vector3 currentScale = transform.localScale;
        float targetScale = Mathf.Min(MaxGrow, currentScale.x + amount * growSpeed);
        transform.localScale = new Vector3(targetScale, targetScale, currentScale.z);
        lightPrefab.SetActive(true);
    }

    void Update()
    {
        // 마지막 피격 후 2초가 지났으면 서서히 줄어듬
        if (Time.time - lastHitTime > 2f)
        {
            Vector3 currentScale = transform.localScale;
            float targetScale = Mathf.Max(MinGrow, currentScale.x - Time.deltaTime * shrinkSpeed);
            transform.localScale = new Vector3(targetScale, targetScale, currentScale.z);
        }
    }
}