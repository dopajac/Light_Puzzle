using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public GameObject pressAnyKeyText;  // UI에 연결 필요
    
    private void Start()
    {
        StartCoroutine(BlinkObject());
    }
    bool IsAnyKeyboardKeyDown()
    {
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            // 마우스 버튼은 제외 (KeyCode.Mouse0~Mouse6)
            if (key >= KeyCode.Mouse0 && key <= KeyCode.Mouse6)
                continue;

            if (Input.GetKeyDown(key))
                return true;
        }
        return false;
    }
    void Update()
    {
        if (IsAnyKeyboardKeyDown())
        {
            SceneManager.LoadScene(1);
        }
    }
    
    IEnumerator BlinkObject()
    {
        while (true)
        {
            pressAnyKeyText.SetActive(!pressAnyKeyText.activeSelf);
            yield return new WaitForSeconds(0.5f); // 깜빡이는 속도 조절
        }
    }
    
}
