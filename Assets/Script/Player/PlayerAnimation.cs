using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isOnLadder = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            Debug.LogError("Animator 컴포넌트를 찾을 수 없습니다.");
        if (spriteRenderer == null)
            Debug.LogError("SpriteRenderer 컴포넌트를 찾을 수 없습니다.");
    }

    private void Update()
    {
        HandleRunAnimation();
        HandleFlip();
        HandleJump();
        HandleLadder();
        HandleShoot();
    }

    private void HandleRunAnimation()
    {
        bool isMoving = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) ||
                        Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);

        animator.SetBool("Run", isMoving);
    }

    private void HandleFlip()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            spriteRenderer.flipX = true;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            spriteRenderer.flipX = false;
        }
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("Jump");
        }
    }

    public void OnJumpEnd()
    {
        animator.SetTrigger("Fall");
    }
    
    private void HandleLadder()
    {
        bool isClimbing = isOnLadder && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) ||
                                         Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow));

        animator.SetBool("Ladder", isClimbing);
    }

    private void HandleShoot()
    {
        if (Input.GetMouseButtonDown(0)) // 우클릭
        {
            Debug.Log("쏨");
            animator.SetTrigger("Shoot");
        }
    }

    // ladder 오브젝트에 닿은 상태 체크
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("ladder"))
        {
            isOnLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("ladder"))
        {
            isOnLadder = false;
        }
    }
}