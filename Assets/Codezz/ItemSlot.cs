using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

public class ItemSlot : MonoBehaviour
{
    public bool showSlotGizmos;

    //Todo: Om vi legger til Offset per item, her skal Item referansen for preview
    public Item item;

    [Button()]
    void SelectMe()
    {
        Selection.activeTransform = transform;
    }
    GameObject exampleMesh => item.itemPrefab;
    private void OnDrawGizmos()
    {
        if (showSlotGizmos == false || item == null)
            return;
        Gizmos.color = new Color(1, 0, 0, .5f);
        foreach (var item in exampleMesh.GetComponentsInChildren<MeshFilter>())
        {

            Gizmos.DrawMesh(item.sharedMesh, transform.position + item.transform.localPosition, transform.rotation * item.transform.localRotation, transform.localScale.MultipliedWith(item.transform.localScale));

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        print("Gotcha");
        var dood = other.GetComponent<Dood>();
        if (dood)
        {
            dood.OnHit(item.itemStrength);
        }
    }
}
