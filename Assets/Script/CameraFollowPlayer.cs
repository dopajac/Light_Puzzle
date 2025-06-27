using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollowPlayer : MonoBehaviour
{
    public GameObject Player;

    public float smoothSpeed = 3;
    public Vector2 offset;
    public float limitMinX, limitMaxX, limitMinY, limitMaxY;
    float cameraHalfWidth, cameraHalfHeight;

    private bool isIntroDone = false;
    public float introScrollSpeed = 7f; // 인트로 이동 속도
    public float introEndX = 23f; // 인트로 종료 위치 (X축 기준)

    private void Start()
    {
        cameraHalfWidth = Camera.main.aspect * Camera.main.orthographicSize;
        cameraHalfHeight = Camera.main.orthographicSize;

        StartCoroutine(IntroScrollRoutine());
    }

    private IEnumerator IntroScrollRoutine()
    {
        Vector3 currentPos = transform.position;
        float fixedY = currentPos.y; // 인트로 동안 고정할 Y값

        while (currentPos.x < introEndX)
        {
            currentPos.x += introScrollSpeed * Time.deltaTime;
            transform.position = new Vector3(currentPos.x, fixedY, -10f);
            yield return null;
        }

        isIntroDone = true;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (Player == null)
        {
            Player = GameObject.Find("Player(Clone)");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Player = GameObject.Find("Player(Clone)");
    }

    public void SetPlayer(GameObject p)
    {
        Player = p;
    }

    private void LateUpdate()
    {
        if (!isIntroDone) return;

        if (Player == null)
        {
            Player = GameObject.Find("Player(Clone)");
            if (Player == null) return;
        }

        Vector3 desiredPosition = new Vector3(
            Mathf.Clamp(Player.transform.position.x + offset.x, limitMinX + cameraHalfWidth, limitMaxX - cameraHalfWidth),
            Mathf.Clamp(Player.transform.position.y + offset.y, limitMaxY - cameraHalfHeight,limitMinY + cameraHalfHeight ),
            -10);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
    }
}
