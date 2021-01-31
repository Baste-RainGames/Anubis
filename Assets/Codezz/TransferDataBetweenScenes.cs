using System.Collections.Generic;
using UnityEngine;

public static class TransferDataBetweenScenes {

    [RuntimeInitializeOnLoadMethod]
    static void HandleReload() {
        equipedToSlotWhenExitingLostNFound.Clear();
    }

    public static readonly Dictionary<ItemSlotID, Item> equipedToSlotWhenExitingLostNFound = new Dictionary<ItemSlotID, Item>();
}