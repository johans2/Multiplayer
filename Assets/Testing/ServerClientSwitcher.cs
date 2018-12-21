using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerClientSwitcher : MonoBehaviour {

    public ServerEngine serverEngine;
    public ClientEngine clientEngine;
    public static bool IsServer { get; private set; }

    public List<SyncedBehaviour> testObjects = new List<SyncedBehaviour>();
    public bool isServer;

    ServerEngine s;
    ClientEngine c;

	void Start () {
        if(isServer) {
            //ServerEngine se = gameObject.AddComponent<ServerEngine>();
            //se.syncedBehaviours = testObjects;
            s = Instantiate(serverEngine);
            IsServer = true;
        }
        else {
            //ClientEngine ce = gameObject.AddComponent<ClientEngine>();
            //ce.syncedBehaviours = testObjects;
            c = Instantiate(clientEngine);
            IsServer = false;
        }
	}

    private void Update() {
        if(IsServer) {
            UpdateServer();
        }
        else {
            UpdateClient();
        }
    }

    private void UpdateServer() {
        if(Input.GetKeyDown(KeyCode.Alpha1)) {
            s.SpawnObject(testObjects[0].gameObject, Vector3.zero, Vector3.zero, Vector3.one);
        }
        if(Input.GetKeyDown(KeyCode.Alpha2)) {
            s.SpawnObject(testObjects[1].gameObject, new Vector3(-1,0,0), Vector3.zero, Vector3.one);
        }
        if(Input.GetKeyDown(KeyCode.Alpha3)) {
            s.SpawnObject(testObjects[2].gameObject, new Vector3(1, 0, 0), Vector3.zero, Vector3.one);
        }

        if(Input.GetKeyDown(KeyCode.X)) {
            SyncedEntity[] entities = FindObjectsOfType<SyncedEntity>();

            if(entities.Length > 0) {
                int index = Random.Range(0, entities.Length);
                Debug.Log("Destroying object " + entities[index].ID);
                s.DestroyObject(entities[index].ID);
            }


        }

    }

    private void UpdateClient() {


    }
}
