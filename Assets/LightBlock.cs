using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LightBlock : MonoBehaviour
{
    public BoxCollider2D boxCollider;
    
    public SpriteRenderer spriteRenderer;
    
    public TilemapCollider2D tilemapCollider;

    public bool isLightOn;
    
    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        tilemapCollider = GetComponent<TilemapCollider2D>();
    }
    
    
    
}
