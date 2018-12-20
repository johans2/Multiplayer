using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ServerEngine : MonoBehaviour {

    public SyncedPrefabRegistry registryPrefab;

    public List<SyncedBehaviour> syncedBehaviours = new List<SyncedBehaviour>();
    public int serverFrameRate = 20;

    private SyncedPrefabRegistry registry;
    private ServerTCPConnection serverTCP;

    int syncedBehaviourID = 0;
    
    private void Awake() {
        Assert.IsNotNull(registryPrefab);

        registry = Instantiate(registryPrefab);
        ServerPacketHandler packetHandler = new ServerPacketHandler(this);
        serverTCP = new ServerTCPConnection(packetHandler);
        serverTCP.SetupServer();
        Application.targetFrameRate = serverFrameRate;
        QualitySettings.vSyncCount = 0;
    }
    
    private void LateUpdate() {
        if(Application.targetFrameRate != serverFrameRate) {
            Application.targetFrameRate = serverFrameRate;
        }

        SerializeFrame();
    }

    public void SpawnObject(GameObject prefab, Vector3 position, Vector3 rotation, Vector3 scale) {
        // Spawn object, set script IDs and add them to the synced list.
        GameObject go = Instantiate(prefab, position, Quaternion.Euler(rotation));
        
        SyncedBehaviour[] syncedScripts = go.GetComponentsInChildren<SyncedBehaviour>();

        foreach(var syncedBehaviour in syncedScripts) {
            syncedBehaviour.ID = syncedBehaviourID;
            syncedBehaviourID++;
        }

        syncedBehaviours.AddRange(syncedScripts);

        // Send the spawndata to all clients.
        PacketBuffer buffer = new PacketBuffer();
        int prefabID = registryPrefab.GetPrefabID(prefab);
        buffer.WriteInteger((int)ServerPackets.SSpawnObject);
        buffer.WriteInteger(prefabID);
        buffer.WriteVector3(position);
        buffer.WriteVector3(rotation);
        buffer.WriteVector3(scale);

        serverTCP.SendData(buffer.ToArray());
    }

    public void DestroyObject(SyncedBehaviour syncedBehaviour) {


    }

    private void SerializeFrame() {
        PacketBuffer buffer = new PacketBuffer();

        int numSyncedBehaviours = syncedBehaviours.Count;

        buffer.WriteInteger((int)ServerPackets.SFrameUpdate);
        buffer.WriteInteger(numSyncedBehaviours);
        
        // TODO: use C# job system for this?
        foreach(var syncedBehaviour in syncedBehaviours) {

            int id = syncedBehaviour.ID;
            byte[] data = syncedBehaviour.Serialize();
            int dataSize = data.Length;

            buffer.WriteInteger(id);
            buffer.WriteInteger(dataSize);
            buffer.WriteBytes(data);
        }

        Debug.Log(string.Format("Sending data for {0} synced obejcts, ", numSyncedBehaviours));

        serverTCP.SendData(buffer.ToArray());
    }

    

}
