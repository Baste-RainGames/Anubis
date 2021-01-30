using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPUI : MonoBehaviour {

    public Image hpTemplate;
    private List<Image> hps = new List<Image>();
    private Player player;

    private void Start() {
        player = FindObjectOfType<Player>();

        if (player == null) {
            gameObject.SetActive(false);
            return;
        }

        hps.Add(hpTemplate);
        while (hps.Count < player.MaxHealth) {
            hps.Add(Instantiate(hpTemplate, hpTemplate.transform.parent));
        }

        UpdateHealthUI();
    }

    private void Update() {
        if (player == null)
            return;

        UpdateHealthUI();
    }

    private void UpdateHealthUI() {
        for (int i = 0; i < player.currentHealth; i++)
            hps[i].color = Color.red;
        for (int i = player.currentHealth; i < hps.Count; i++)
            hps[i].color = Color.black;
    }

}