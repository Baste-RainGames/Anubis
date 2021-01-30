using System.Collections.Generic;
using UnityEngine;

public class EquipItemToRagdollTrigger : MonoBehaviour {

    public ItemSlotID slot;

    public LostAndFoundObject Equipped { get; private set; }
    private DragAndDropObjects dragController;
    private List<LostAndFoundObject> ignoreUntilExited = new List<LostAndFoundObject>();

    private void Start() {
        dragController = FindObjectOfType<DragAndDropObjects>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.TryGetComponent<LostAndFoundObject>(out var item))
            if (!ignoreUntilExited.Contains(item))
                EquipItem(item);
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.TryGetComponent<LostAndFoundObject>(out var item))
            ignoreUntilExited.Remove(item);
    }

    private void EquipItem(LostAndFoundObject obj) {
        if (dragController)
            dragController.DropIfDragging(obj);

        var rb = obj.rb;

        if (Equipped) {
            LetGoOfEquipped(rb.velocity);
        }

        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        obj.col.enabled = false;
        Equipped = obj;
    }

    public void LetGoOfEquipped(Vector2 velocity) {
        ignoreUntilExited.Add(Equipped);

        Equipped.rb.bodyType = RigidbodyType2D.Dynamic;
        Equipped.col.enabled = true;
        Equipped.rb.velocity = velocity;

        Equipped = null;
    }

    private void Update() {
        if (Equipped)
            Equipped.transform.position = Vector3.Lerp(Equipped.transform.position, transform.position, Time.deltaTime * 4f);
    }
}