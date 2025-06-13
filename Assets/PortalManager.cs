using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    [SerializeField] private GameObject PortalPrefab;
    [SerializeField] private GameObject PortalPrefab2;

    void Start()
    {
        Portal portalA = PortalPrefab.GetComponent<Portal>();
        Portal portalB = PortalPrefab2.GetComponent<Portal>();

        if (portalA != null && portalB != null)
        {
            portalA.linkedPortal = portalB;
            portalB.linkedPortal = portalA;
        }
    }
}

