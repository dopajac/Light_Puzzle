using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class LightFlicker : MonoBehaviour
{
    public static LightFlicker Instance;

    public Light2D globalLight;
    public float flickerInterval = 3f;
    public float normalIntensity = 1f;
    public float flickerIntensity = 0f;

    public bool IsLightOn { get; private set; } = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        HandleFlickerByScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        HandleFlickerByScene(scene.buildIndex);
    }

    private void HandleFlickerByScene(int sceneIndex)
    {
        StopAllCoroutines();

        if (globalLight == null)
        {
            Debug.LogWarning("globalLight가 할당되지 않았습니다.");
            return;
        }

        if (sceneIndex == 5)
        {
            StartCoroutine(FlickerRoutine()); // 반복 깜빡임
        }
        else
        {
            globalLight.intensity = normalIntensity;
            IsLightOn = true;
            StartCoroutine(SingleFlickerOnce()); // 한 번 꺼짐
        }
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            globalLight.intensity = flickerIntensity;
            IsLightOn = false;
            yield return new WaitForSeconds(flickerInterval);

            globalLight.intensity = normalIntensity;
            IsLightOn = true;
            yield return new WaitForSeconds(flickerInterval);
        }
    }

    IEnumerator SingleFlickerOnce()
    {
        yield return new WaitForSeconds(flickerInterval+1);
        globalLight.intensity = flickerIntensity;
        IsLightOn = false;
    }
}
