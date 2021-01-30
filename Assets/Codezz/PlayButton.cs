using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour {
    public string sceneToLoad;

    public void Play() {
        var equipSlots = FindObjectsOfType<EquipItemToRagdollTrigger>();

        var dict = TransferDataBetweenScenes.equipedToSlotWhenExitingLostNFound;
        dict.Clear();
        foreach (var equipSlot in equipSlots) {
            dict[equipSlot.slot] = equipSlot.Equipped != null ? equipSlot.Equipped.item : null;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}