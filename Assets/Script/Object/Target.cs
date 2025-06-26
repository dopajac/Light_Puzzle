using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private GameObject TargetLight;

    public void OnOffLight()
    {
        if (TargetLight.activeSelf)
        {
            OffTargetLight();
        }
        else
        {
            OnTargetLight();
        }
    }
    public void OnTargetLight()
    {
            TargetLight.SetActive(true);
            GameManager.Instance.TargetCount++;
    }

    public void OffTargetLight()
    {
        TargetLight.SetActive(false);
        GameManager.Instance.TargetCount--;
    }
    
}
