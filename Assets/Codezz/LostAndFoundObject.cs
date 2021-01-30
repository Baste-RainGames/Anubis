using UnityEngine;

public class LostAndFoundObject : MonoBehaviour {
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;
    public Collider2D col;
    public Item item { get; private set; }

    public void SetItem(Item item) {
        this.item = item;
        spriteRenderer.sprite = item.sprite;
    }
}