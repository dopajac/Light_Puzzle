using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private GameObject TargetLight;

    public void OnTargetLight()
    {
        {
            TargetLight.SetActive(true);
            GameManager.Instance.TargetCount++;
        }
    }
}
