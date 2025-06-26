using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LightBlock : MonoBehaviour
{
    public BoxCollider2D boxCollider;
    
    public TilemapRenderer tilemapRenderer;
    
    public TilemapCollider2D tilemapCollider;

    public bool isLightOn;
    
    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
        tilemapCollider = GetComponent<TilemapCollider2D>();
        if (isLightOn == false)
        {
            tilemapRenderer.enabled = false;
            tilemapCollider.enabled = false;
        }
        else
        {
            
            tilemapRenderer.enabled = true;
            tilemapCollider.enabled = true;
        }
    }
    
    
    
}
