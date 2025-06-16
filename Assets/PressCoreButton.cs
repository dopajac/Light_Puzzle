using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressCoreButton : MonoBehaviour
{
    private bool isPlayerNear = false;
    [SerializeField] private GameObject Floating_F;
    private CoreController coreController;

    void Start()
    {
        coreController = FindObjectOfType<CoreController>();
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F))
        {
            if (coreController != null && coreController.slider.value >= 1f)
            {
                Vector2 spawnPos = coreController.Core.transform.position;
                LineSpawner.Instance.ToggleCoreLaser(spawnPos);
            }
            else
            {
                Debug.Log("슬라이더가 아직 가득 차지 않음.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("플레이어가 코어 버튼에 접근했습니다.");

            if (coreController != null)
            {
                Floating_F.gameObject.SetActive(true);
                isPlayerNear = true;
                Debug.Log("버튼 근처에 도착함.");
            }
            else
            {
                Debug.LogWarning("CoreController를 찾을 수 없습니다.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Floating_F.gameObject.SetActive(false);
            isPlayerNear = false;
        }
    }
}