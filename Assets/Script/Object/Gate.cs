using UnityEngine;
using UnityEngine.SceneManagement;

public class Gate : MonoBehaviour
{
    private bool isPlayerNear = false;

    [SerializeField] private GameObject Floating_F;
    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("게이트와 상호작용 중입니다.");
            SceneManager.LoadScene(2);
            // 여기서 문 열기, 장면 전환 등 원하는 동작 추가
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Floating_F.gameObject.SetActive(true);
            isPlayerNear = true;
            Debug.Log("게이트 근처에 도착함.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Floating_F.gameObject.SetActive(false);
            isPlayerNear = false;
            Debug.Log("게이트에서 멀어짐.");
        }
    }
}