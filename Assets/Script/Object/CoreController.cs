using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class CoreController : MonoBehaviour
{
    private int Maxintensity = 50;
    private int Minintensity = 0;
    [SerializeField]private Light2D lightSource;
    
    public Slider slider;

    public GameObject Core;
    void Start()
    {
        slider.gameObject.SetActive(false);
    }

    // Update is called once per frame
    public float currentIntensity = 0f;

    public void IncreaseIntensity(float amount)
    {
        if (slider.gameObject.activeSelf == false)
        {
            Debug.Log("슬라이더 활성화"); 
            slider.gameObject.SetActive(true);
        }
        currentIntensity = Mathf.Clamp(currentIntensity + amount, Minintensity, Maxintensity);
        Debug.Log("증가 후 intensity: " + currentIntensity);
    }

    void Update()
    {
        lightSource.intensity = currentIntensity;
        slider.value = currentIntensity / Maxintensity;
    }
    
}
