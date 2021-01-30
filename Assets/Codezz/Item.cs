using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "New Item", menuName = "Create new Item")]
public class Item : ScriptableObject
{
    [AssetsOnly]
    public GameObject itemPrefab;

    public ItemType itemType;
    public int itemStrength = 1;
    public int bounceStrength = 1;
    public Sprite sprite;
    public int spawnProbability;

    [FoldoutGroup("Transform offset adjust")]
    public Vector3 offsetPosition, offsetScale, offsetRotation;
}
public enum ItemType { Thrown, Projectile, Club, Sharp, Squeeker}