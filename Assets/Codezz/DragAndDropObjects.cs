using UnityEngine;

public class DragAndDropObjects : MonoBehaviour {

    public Camera mainCamera;
    public float equipDistance;

    public int numItemsToSpawn;
    public LostAndFoundObject lostAndFoundPrefab;

    private TargetJoint2D joint;
    private Rigidbody2D dragged;

    private void Start() {
        var items = Resources.LoadAll<Item>("Itemz");

        if (items.Length == 0) {
            Debug.LogError("No itemzz");
            return;
        }

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

            Spawn(toSpawn);
        }
    }

    private void Spawn(Item toSpawn) {
        var spawned = Instantiate(lostAndFoundPrefab);
        spawned.SetItem(toSpawn);
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
            StartDragging(hit.attachedRigidbody, mouseWorldPos);
        }
    }

    private void StartDragging(Rigidbody2D rb, Vector2 mouseWorldPos) {
        dragged = rb;
        dragged.gameObject.layer = LayerMask.NameToLayer("Dragged");
        dragged.isKinematic = false;
        joint = dragged.gameObject.AddComponent<TargetJoint2D>();
        joint.autoConfigureTarget = false;
        joint.target = mouseWorldPos;
    }

    private void LetGoOfDragged() {
        dragged.gameObject.layer = LayerMask.NameToLayer("Default");
        Destroy(joint);
        dragged = null;
    }

    private Vector3? debugPos;
    private Ray? debugRay;
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        if (debugPos.HasValue)
            Gizmos.DrawSphere(debugPos.Value, .2f);
        if (debugRay.HasValue) {
            var ray = debugRay.Value;
            Gizmos.DrawRay(ray.origin, ray.origin + 10000f * ray.direction);
        }
    }
}