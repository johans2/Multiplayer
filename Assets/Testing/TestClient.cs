using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestClient : MonoBehaviour {

    public List<SyncedBehaviour> testObjects = new List<SyncedBehaviour>();

    ClientEngine api;

	IEnumerator Start () {
        ClientPacketHandler.InitializePackageHandlers();
        ClientTCP.ConnectToServer();

        yield return null;

        ClientEngine.syncedBehaviours = testObjects;
        api = gameObject.AddComponent<ClientEngine>();

        Logger.Log(string.Format("Client has {0} synced obejcts.", ClientEngine.syncedBehaviours.Count));
	}


    private void OnApplicationQuit() {
        ClientTCP.Disconnect();
    }

}
