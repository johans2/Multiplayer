using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerClientSwitcher : MonoBehaviour {

    public List<SyncedBehaviour> testObjects = new List<SyncedBehaviour>();
    public bool isServer;

	void Start () {
        if(isServer) {
            ServerEngine se = gameObject.AddComponent<ServerEngine>();
            se.syncedBehaviours = testObjects;
        }
        else {
            ClientEngine ce = gameObject.AddComponent<ClientEngine>();
            ce.syncedBehaviours = testObjects;
        }
	}
}
