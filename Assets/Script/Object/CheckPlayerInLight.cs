using UnityEngine;

public class CheckPlayerInLight : MonoBehaviour
{
    [SerializeField] private Transform lightOrigin;
    [SerializeField] private GameObject player;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private float lightRange = 9f;

    private PolygonCollider2D collider;

    private void Start()
    {
        collider = GetComponent<PolygonCollider2D>();
    }

    private void Update()
    {
        if (player == null)
        {
            player = GameObject.Find("Player(Clone)");
            if (player == null) return;
        }

        if (collider.OverlapPoint(player.transform.position))
        {
            Debug.Log("죽었다! 플레이어가 빛에 감지됨 → 대상: " + player.name);
            Player playerScript = player.GetComponent<Player>();
            playerScript.Respawn();
        }
    }

    public void SetPlayer(GameObject p)
    {
        player = p;
    }
}