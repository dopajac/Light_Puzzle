using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LIghtBlockOnOff : MonoBehaviour
{
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other is TilemapCollider2D) return;
        if (other.gameObject.layer == LayerMask.NameToLayer("LightBlock"))
        {
            LightBlock lightBlock = other.gameObject.GetComponent<LightBlock>();
            
            if (lightBlock.isLightOn == true)
            {Debug.Log("실행완료");
                lightBlock.tilemapRenderer.enabled = false;
                lightBlock.tilemapCollider.enabled = false;
            }
            else
            {
                lightBlock.tilemapRenderer.enabled = true;
                lightBlock.tilemapCollider.enabled = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other is TilemapCollider2D) return;
        if (other.gameObject.layer == LayerMask.NameToLayer("LightBlock"))
        {
            LightBlock lightBlock = other.gameObject.GetComponent<LightBlock>();

            if (lightBlock.isLightOn == true)
            {
                Debug.Log("실행완");
                lightBlock.tilemapRenderer.enabled = true;
                lightBlock.tilemapCollider.enabled = true;
            }
            else
            {
                lightBlock.tilemapRenderer.enabled = false;
                lightBlock.tilemapCollider.enabled = false;
            }
        }
    }
}
