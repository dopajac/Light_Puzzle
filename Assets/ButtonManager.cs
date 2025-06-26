
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

    public void OnRestart()
    {
        SceneManager.LoadScene(1);
        Debug.Log("Restart 버튼 눌림");
    }
}