using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ItemSlot : MonoBehaviour
{
    [ShowInInspector]
    public static bool showSlotGizmos;

    //Todo: Om vi legger til Offset per item, her skal Item referansen for preview
    public Item exampleItem; 
    GameObject exampleMesh => exampleItem.itemPrefab;
    private void OnDrawGizmos()
    {
        if (showSlotGizmos == false || exampleItem == null)
            return;
        Gizmos.color = new Color(1, 0, 0, .5f);
        foreach (var item in exampleMesh.GetComponentsInChildren<MeshFilter>())
        {
            
            Gizmos.DrawMesh(item.sharedMesh, transform.position + item.transform.localPosition, transform.rotation * item.transform.localRotation,  transform.localScale + item.transform.localScale);

        }
    }
}
