using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

public class ItemSlot : MonoBehaviour
{
    [ShowInInspector]
    public static bool showSlotGizmos;

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
        foreach (var mf in exampleMesh.GetComponentsInChildren<MeshFilter>())
        {

            Gizmos.DrawMesh(mf.sharedMesh, 
                transform.position + mf.transform.localPosition + offsetPos(), 
                transform.rotation * mf.transform.localRotation * Quaternion.Euler(item.offsetRotation), 
                transform.localScale.MultipliedWith(mf.transform.localScale).MultipliedWith(item.offsetScale));

        }
    }

    Vector3 offsetPos()
    {
        return transform.up * item.offsetPosition.y + transform.right * item.offsetPosition.x + transform.forward * item.offsetPosition.z;
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

public enum ItemSlotID {
    leftHandSlot,
    rightHandSlot,
    headSlot,
    torsoSlot,
    leftArmSlot,
    rightArmSlot,
    leftLegSlot,
    rightLegSlot
}
