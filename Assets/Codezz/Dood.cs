using UnityEngine;

public class Dood : MonoBehaviour {

    public float speed;
    private Player player;

    private void Start() {
        player = FindObjectOfType<Player>();
    }

    private void Update() {
        if (player) {
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, Time.deltaTime * speed);
        }
    }
}