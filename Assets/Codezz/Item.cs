using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "New Item", menuName = "Create new Item")]
public class Item : ScriptableObject
{
    public ItemType itemType;
    [AssetsOnly]
    public GameObject itemPrefab;
}
public enum ItemType { Thrown, Projectile, Club, Sharp, Squeeker}