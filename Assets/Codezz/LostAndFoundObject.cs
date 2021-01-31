using System;
using UnityEngine;

public class LostAndFoundObject : MonoBehaviour {
    public Rigidbody2D rb;
    public Collider2D[] colliders;

    [NonSerialized]
    public Item item;
}