using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerClientSwitcher : MonoBehaviour {

    public List<SyncedBehaviour> testObjects = new List<SyncedBehaviour>();
    public bool isServer;

	void Start () {
        if(isServer) {
            TestServer s = gameObject.AddComponent<TestServer>();
            s.testObjects = testObjects;
        }
        else {
            TestClient c = gameObject.AddComponent<TestClient>();
            c.testObjects = testObjects;
        }
	}
}
