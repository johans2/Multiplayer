using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncedEntity : MonoBehaviour {

    public int ID;
    public int Owner { get; private set; }
    public bool IsControlledByMe { get; private set; }

    public SyncedBehaviour[] syncedBehaviours;

    private void Awake() {
        syncedBehaviours = GetComponentsInChildren<SyncedBehaviour>();
    }

}
