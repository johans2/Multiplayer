using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestClient : MonoBehaviour {

    public List<SyncedBehaviour> testObjects = new List<SyncedBehaviour>();

    ClientEngine engine;

	private void Start () {
        /*
        engine = gameObject.AddComponent<ClientEngine>();
        ClientEngine.syncedBehaviours = testObjects;

        Logger.Log(string.Format("Client has {0} synced obejcts.", ClientEngine.syncedBehaviours.Count));
        */
	}


    private void OnApplicationQuit() {
        /*
        ClientTCPConnection.Disconnect();
        */
    }

}
