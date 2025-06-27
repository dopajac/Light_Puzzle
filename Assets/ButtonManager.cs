
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public void OnQuit()
    {
        Application.Quit();
        Debug.Log("Quit 버튼 눌림");
    }

    public void ClearDontDestroyOnLoadObjects()
    {
        GameObject[] rootObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in rootObjects)
        {
            if (obj.hideFlags == HideFlags.None && obj.scene.name == null)
            {
                // 이 오브젝트는 DontDestroyOnLoad 영역에 있음
                Destroy(obj);
            }
        }

        Debug.Log("모든 DontDestroyOnLoad 오브젝트 제거 완료");
    }
    
    public void OnRestart()
    {
        ClearDontDestroyOnLoadObjects();
        SceneManager.LoadScene(1);
        Debug.Log("Restart 버튼 눌림");
    }
}