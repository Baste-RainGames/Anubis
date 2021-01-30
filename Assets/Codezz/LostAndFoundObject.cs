using UnityEngine;

public class LostAndFoundObject : MonoBehaviour {
    public SpriteRenderer spriteRenderer;
    public Item item { get; private set; }

    public void SetItem(Item item) {
        this.item = item;
        spriteRenderer.sprite = item.sprite;
    }
}