using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestServer : MonoBehaviour {

    public List<SyncedBehaviour> testObjects = new List<SyncedBehaviour>();

    ServerAPI api;
    

	IEnumerator Start () {
        ServerHandleNetworkData.InitializePackageHandlers();
        ServerTCP.SetupServer();

        api = gameObject.AddComponent<ServerAPI>();

        yield return null;

        api.syncedBehaviours.AddRange(testObjects);
        
	}
    
}
