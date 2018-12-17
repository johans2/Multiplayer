using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestClient : MonoBehaviour {

    public List<SyncedBehaviour> testObjects = new List<SyncedBehaviour>();

    ClientAPI api;

	IEnumerator Start () {
        ClientHandleNetworkData.InitializePackageHandlers();
        ClientTCP.ConnectToServer();

        yield return null;

        ClientAPI.syncedBehaviours = testObjects;
        api = gameObject.AddComponent<ClientAPI>();

        Logger.Log(string.Format("Client has {0} synced obejcts.", ClientAPI.syncedBehaviours.Count));
	}


    private void OnApplicationQuit() {
        ClientTCP.Disconnect();
    }

}
