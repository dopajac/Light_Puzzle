using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LIghtBlockOnOff : MonoBehaviour
{
    private void onTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("LightBlock"))
        {
            LightBlock lightBlock = other.gameObject.GetComponent<LightBlock>();

            if (lightBlock.isLightOn == true)
            {
                lightBlock.spriteRenderer.enabled = false;
                lightBlock.tilemapCollider.enabled = false;
            }
            else
            {
                lightBlock.spriteRenderer.enabled = true;
                lightBlock.tilemapCollider.enabled = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("LightBlock"))
        {
            LightBlock lightBlock = other.gameObject.GetComponent<LightBlock>();

            if (lightBlock.isLightOn == true)
            {
                lightBlock.spriteRenderer.enabled = true;
                lightBlock.tilemapCollider.enabled = true;
            }
            else
            {
                lightBlock.spriteRenderer.enabled = false;
                lightBlock.tilemapCollider.enabled = false;
            }
        }
    }
}
