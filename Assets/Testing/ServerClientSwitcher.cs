using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerClientSwitcher : MonoBehaviour {

    public bool isServer;

	void Start () {
        if(isServer) {
            gameObject.AddComponent<TestServer>();
        }
        else {
            gameObject.AddComponent<TestClient>();
        }
	}
}
