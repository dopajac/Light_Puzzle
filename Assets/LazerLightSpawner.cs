using System.Collections.Generic;
using UnityEngine;

public class LazerLightSpawner : MonoBehaviour
{
    public GameObject lightPrefab;
    public int maxLightCount = 30;
    public float lightSpacing = 0.2f;

    private Queue<GameObject> lightPool = new Queue<GameObject>();
    private List<GameObject> activeLights = new List<GameObject>();

    void Awake()
    {
        for (int i = 0; i < maxLightCount; i++)
        {
            GameObject obj = Instantiate(lightPrefab, transform);
            obj.SetActive(false);
            lightPool.Enqueue(obj);
        }
    }

    public void ClearLights()
    {
        foreach (GameObject light in activeLights)
        {
            light.SetActive(false);
            lightPool.Enqueue(light);
        }
        activeLights.Clear();
    }

    public void SpawnLightsAlongPath(List<Vector3> points)
    {
        ClearLights();

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 start = points[i];
            Vector3 end = points[i + 1];
            Vector3 dir = (end - start).normalized;
            float distance = Vector3.Distance(start, end);

            int count = Mathf.FloorToInt(distance / lightSpacing);
            for (int j = 0; j <= count; j++)
            {
                if (lightPool.Count == 0)
                    return;

                Vector3 pos = start + dir * (j * lightSpacing);
                GameObject lightObj = lightPool.Dequeue();
                lightObj.transform.position = pos;
                lightObj.SetActive(true);
                activeLights.Add(lightObj);
            }
        }
    }
}