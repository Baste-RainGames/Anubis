using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HPUI : MonoBehaviour {

    public Image hpTemplate;
    public Sprite hpFull;
    public Sprite hpEmpty;

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
        for (int i = 0; i < player.currentHealth; i++) {
            if (hps[i].sprite != hpFull) {
                hps[i].sprite = hpFull;
                hps[i].transform.DOPunchScale(1.5f * Vector3.one, .2f);
            }
        }

        for (int i = player.currentHealth; i < hps.Count; i++)
            if (hps[i].sprite != hpEmpty) {
                hps[i].sprite = hpEmpty;
                hps[i].transform.DOPunchScale(-.5f * Vector3.one, .2f);
            }
    }

}