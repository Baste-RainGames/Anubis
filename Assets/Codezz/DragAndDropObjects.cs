using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragAndDropObjects : MonoBehaviour {

    public Camera mainCamera;
    public Camera uiCamera;
    public Rigidbody dragger;
    public GraphicRaycaster rc;
    public float equipDistance;

    public int numItemsToSpawn;
    public LostAndFoundObject lostAndFoundPrefab;

    private SpringJoint joint;
    private Rigidbody dragged;

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
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        PositionDraggerUnderMouse(ray);

        if (Input.GetMouseButtonDown(0))
            TryDrag(ray);

        if (joint && Input.GetMouseButtonUp(0))
            LetGoOfDragged();
    }

    private void PositionDraggerUnderMouse(Ray ray) {
        var plane = new Plane(Vector3.back, dragger.transform.position);

        if (plane.Raycast(ray, out var distance)) {
            var hitPoint = ray.origin + distance * ray.direction;
            dragger.MovePosition(hitPoint);
        }
    }

    private void TryDrag(Ray ray) {
        if (Physics.Raycast(ray, out var hit)) {
            if (hit.rigidbody) {
                StartDragging(hit.rigidbody);
            }
        }
    }

    private void StartDragging(Rigidbody rb) {
        dragged = rb;
        dragged.isKinematic = false;
        joint = dragger.gameObject.AddComponent<SpringJoint>();

        var pos = dragger.transform.position;
        pos.y = rb.transform.position.y;
        dragger.position = pos;

        joint.connectedBody = rb;
        joint.autoConfigureConnectedAnchor = false;

        joint.connectedAnchor = Vector3.zero;
    }

    private void LetGoOfDragged() {
        Destroy(joint);

        PointerEventData dummyEventData = new PointerEventData(FindObjectOfType<EventSystem>());
        dummyEventData.position = Input.mousePosition;

        var list = new List<RaycastResult>();
        rc.Raycast(dummyEventData, list);

        if (list.Count > 0) {
            var attachTo = list[0].gameObject.GetComponent<RectTransform>();

            var worldToScreenPoint = uiCamera.WorldToScreenPoint(attachTo.position);
            // debugRay = uiCamera.ScreenPointToRay(worldToScreenPoint);

            var ray = mainCamera.ScreenPointToRay(worldToScreenPoint);
            var plane = new Plane(Vector3.back, dragger.transform.position);

            if (plane.Raycast(ray, out var distance)) {
                var worldPos = ray.origin + distance * ray.direction;

                var distanceToTarget = Vector3.Distance(dragged.position, worldPos);
                if (distanceToTarget < equipDistance) {
                    dragged.velocity = Vector3.zero;
                    dragged.isKinematic = true;
                    dragged.MovePosition(worldPos);
                }
            }

        }
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