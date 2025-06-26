using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CanvasController : MonoBehaviour
{
    [SerializeField] private TMP_Text TargetText;

    private void Update()
    {
        TargetText.text = "Target Count: " + GameManager.Instance.TargetCount.ToString() + "/" + GameManager.Instance.TargetMaxCount.ToString();
    }
}
