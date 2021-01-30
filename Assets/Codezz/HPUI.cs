using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPUI : MonoBehaviour {

    public Image hpTemplate;
    private List<Image> hps = new List<Image>();

    private void Start() {
        var player = FindObjectOfType<Player>();

        if (player == null) {
            gameObject.SetActive(false);
            return;
        }

        hps.Add(hpTemplate);
        while (hps.Count < player.MaxHealth) {
            hps.Add(Instantiate(hpTemplate, hpTemplate.transform.parent));
        }
    }
}