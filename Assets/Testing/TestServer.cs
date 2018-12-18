using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestServer : MonoBehaviour {

    public List<SyncedBehaviour> testObjects = new List<SyncedBehaviour>();

    ServerEngine engine;
    

	private void Start () {
        /*
        engine = gameObject.AddComponent<ServerEngine>();
        engine.syncedBehaviours.AddRange(testObjects);
        */
	}
    
}
