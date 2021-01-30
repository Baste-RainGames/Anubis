using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Item", menuName = "Create new Item")]
public class Item : ScriptableObject
{
    public ItemType itemType;
    public GameObject itemPrefab;
}
public enum ItemType { Thrown, Projectile, Club, Sharp, Squeeker}