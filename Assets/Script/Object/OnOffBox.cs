using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnOffBox : MonoBehaviour
{
    [SerializeField]private GameObject LightBox;
    [SerializeField]private PolygonCollider2D collider;
    private void Start()
    {
        StartCoroutine(ToggleRoutine());
    }

    IEnumerator ToggleRoutine()
    {
        while (true)
        {
            LightBox.gameObject.SetActive(true);  // 켜짐
            collider.enabled = false;
            yield return new WaitForSeconds(2f);

            LightBox.gameObject.SetActive(false); // 꺼짐
            collider.enabled = true;
            yield return new WaitForSeconds(2f);
        }
    }
}
