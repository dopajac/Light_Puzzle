using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    public static LightFlicker Instance;

    public Light2D globalLight;
    public float flickerInterval = 3f;
    public float flickerDuration = 0.1f;
    public float normalIntensity = 1f;
    public float flickerIntensity = 0f;

    public bool IsLightOn { get; private set; } = true;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(FlickerRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // 꺼짐
            globalLight.intensity = flickerIntensity;
            IsLightOn = false;
            yield return new WaitForSeconds(flickerInterval);

            // 다시 켜짐
            globalLight.intensity = normalIntensity;
            IsLightOn = true;
            yield return new WaitForSeconds(flickerInterval);
        }
    }
}