using System.Collections.Generic;
using UnityEngine;

public static class TransferDataBetweenScenes {

    [RuntimeInitializeOnLoadMethod]
    static void HandleReload() {
        Debug.Log("Kay");
        equipedToSlotWhenExitingLostNFound.Clear();
    }

    public static readonly Dictionary<ItemSlotID, Item> equipedToSlotWhenExitingLostNFound = new Dictionary<ItemSlotID, Item>();
}