using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestServer : MonoBehaviour {

    public List<SyncedBehaviour> testObjects = new List<SyncedBehaviour>();

    ServerEngine api;
    

	IEnumerator Start () {
        ServerPacketHandler.InitializePackageHandlers();
        ServerTCP.SetupServer();

        api = gameObject.AddComponent<ServerEngine>();

        yield return null;

        api.syncedBehaviours.AddRange(testObjects);
        
	}
    
}
