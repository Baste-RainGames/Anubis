using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DragAndDropObjects : MonoBehaviour {

    public Camera mainCamera;
    public Transform shelvesParent;
    public int numItemsToSpawn;
    public LostAndFoundObject lostAndFoundPrefab;

    private TargetJoint2D joint;
    private LostAndFoundObject dragged;
    private List<Vector3> spawnSpots;

    private void Start() {
        var items = Resources.LoadAll<Item>("Itemz");

        if (items.Length == 0) {
            Debug.LogError("No itemzz");
            return;
        }

        var shelves = shelvesParent.GetComponentsInChildren<EdgeCollider2D>();
        spawnSpots = new List<Vector3>();
        foreach (var shelf in shelves) {
            var edge0 = shelf.points[0];
            var edge1 = shelf.points[1];

            edge0 = shelf.transform.TransformPoint(edge0);
            edge1 = shelf.transform.TransformPoint(edge1);

            var distance = Vector3.Distance(edge0, edge1);
            for (int i = 0; i < Mathf.FloorToInt(distance); i++) {
                spawnSpots.Add(Vector3.MoveTowards(edge0, edge1, i + .5f) + new Vector3(0f, .5f));
            }
        }

        var freeSpawnSpot = new List<Vector3>(spawnSpots);

        var itemBag = new (Item item, int weightSum)[items.Length];
        var sum = 0;
        for (int i = 0; i < itemBag.Length; i++) {
            var item = items[i];
            var weight = Mathf.Max(1, item.spawnProbability);
            sum += weight;
            itemBag[i] = (item, sum);
        }

        for (int i = 0; i < numItemsToSpawn; i++) {
            var random = Random.Range(0, sum);
            Item toSpawn = null;
            for (int j = 0; j < itemBag.Length; j++) {
                if (random < itemBag[j].weightSum) {
                    toSpawn = itemBag[j].item;
                    break;
                }
            }

            if (toSpawn == null) {
                Debug.LogError("Everything's wrong!");
                return;
            }

            Spawn(toSpawn, freeSpawnSpot);
        }
    }

    private void Spawn(Item toSpawn, List<Vector3> spots) {
        var index = Random.Range(0, spots.Count);
        var spot = spots[index];
        spots.RemoveAt(index);

        Instantiate(toSpawn.itemPrefab2D, spot, Quaternion.identity);
    }

    void Update() {
        var mouseWorldPos = (Vector2) mainCamera.ScreenToWorldPoint(Input.mousePosition);

        if (joint)
            joint.target = mouseWorldPos;

        if (Input.GetMouseButtonDown(0))
            TryStartDrag(mouseWorldPos);

        if (joint && Input.GetMouseButtonUp(0))
            LetGoOfDragged();
    }

    private void TryStartDrag(Vector2 mouseWorldPos) {
        var hit = Physics2D.OverlapCircle(mouseWorldPos, .1f);
        if (hit) {
            var lfo = hit.gameObject.GetComponent<LostAndFoundObject>();
            if (lfo)
                StartDragging(lfo, mouseWorldPos);
            else {
                var slot = hit.gameObject.GetComponent<EquipItemToRagdollTrigger>();
                if (slot && slot.Equipped) {
                    var item = slot.Equipped;
                    slot.LetGoOfEquipped(Vector2.zero);
                    StartDragging(item, mouseWorldPos);
                }
            }

        }
    }

    private void StartDragging(LostAndFoundObject obj, Vector2 mouseWorldPos) {
        dragged = obj;
        dragged.gameObject.layer = LayerMask.NameToLayer("Dragged");
        joint = dragged.gameObject.AddComponent<TargetJoint2D>();
        joint.autoConfigureTarget = false;
        joint.target = mouseWorldPos;
    }

    private void LetGoOfDragged() {
        dragged.gameObject.layer = LayerMask.NameToLayer("Default");
        Destroy(joint);
        dragged = null;
    }


    private void OnDrawGizmosSelected() {
        if (spawnSpots != null) {
            Gizmos.color = Color.yellow;
            foreach (var spawnSpot in spawnSpots) {
                Gizmos.DrawSphere(spawnSpot, .2f);
            }
        }
    }

    public void DropIfDragging(LostAndFoundObject lostAndFoundObject) {
        if (dragged == lostAndFoundObject) {
            LetGoOfDragged();
        }
    }
}