using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Assertions;

public class ClientEngine : MonoBehaviour {

    public SyncedPrefabRegistry registryPrefab;

    public List<SyncedBehaviour> syncedBehaviours = new List<SyncedBehaviour>(); // Make private

    private SyncedPrefabRegistry registry;
    private ClientTCPConnection clientTCP;
    private Queue frameQueue = Queue.Synchronized(new Queue());
    private Queue spawnObjectQueue = Queue.Synchronized(new Queue());

    private void Awake() {
        Assert.IsNotNull(registryPrefab, "Missing prefab ergistry.");
        registry = Instantiate(registryPrefab);

        ClientPacketHandler packetHandler = new ClientPacketHandler(this);
        clientTCP = new ClientTCPConnection(packetHandler);
        clientTCP.ConnectToServer();
    }

    private void LateUpdate() {
        while(spawnObjectQueue.Count > 0) {
            SpawnSyncedObject((byte[])spawnObjectQueue.Dequeue());
        }

        while(frameQueue.Count > 0) {
            DeserializeFrame((byte[])frameQueue.Dequeue());
        }
    }

    public void QueueFrameUpdate(byte[] frameData) {
        frameQueue.Enqueue(frameData);
    }

    public void QueueObjectSpawn(byte[] objectData) {
        spawnObjectQueue.Enqueue(objectData);
    }

    private void DeserializeFrame(byte[] frameData) {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(frameData);
        buffer.ReadInteger(); // Packet id int

        int numSyncedBehaviours = buffer.ReadInteger();

        for(int i = 0; i < numSyncedBehaviours; i++) {

            // Read the data for the synced behaviour
            int id = buffer.ReadInteger();
            int dataSize = buffer.ReadInteger();
            byte[] data = buffer.ReadBytes(dataSize);

            // Get the object from the synced object list
            SyncedBehaviour behaviour = GetSyncedBehaviour(id);
            
            if(behaviour == null) {
                Logger.Log("Receiving information about a synced objects that does not exist on this client.");
                continue;
            }
            else {
                behaviour.Deserialize(data);
            }
        }
    }

    private void SpawnSyncedObject(byte[] spawnData) {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(spawnData);
        buffer.ReadInteger(); // Packet int id

        // Read prefab ID
        int prefabID = buffer.ReadInteger();

        // Spawn the requested prefab
        GameObject prefab = registry.GetPrefab(prefabID);
        GameObject go = Instantiate(prefab);

        // Sync IDs for all SyncedBehaviour scrips on the object.
        // NOTE:    This is dependant on the ordet of GetComponentsInChildren().
        //          It should work but I'm not 100% sure tbh.
        SyncedBehaviour[] syncedScripts = go.GetComponentsInChildren<SyncedBehaviour>();
        
        for(int i = 0; i < syncedScripts.Length; i++) {
            syncedScripts[i].ID = buffer.ReadInteger();
        }
        
        // Set the transform properties.
        Transform goTransform = go.transform;
        goTransform.position = buffer.ReadVector3();
        goTransform.rotation = Quaternion.Euler(buffer.ReadVector3());
        goTransform.localScale = buffer.ReadVector3();

        syncedBehaviours.AddRange(syncedScripts);
    }

    private void DestroySyncedObejct() {

    }

    private SyncedBehaviour GetSyncedBehaviour(int id) {

        foreach(var syncedBehaviour in syncedBehaviours) {
            if(syncedBehaviour.ID == id) {
                return syncedBehaviour;
            }
        }

        return null;

    }

    private void RequestSpawnObject(Action<GameObject> gameObject) {
        throw new NotImplementedException();
    }

    private void RequestDeleteObject() {
        throw new NotImplementedException();
    }

    private void OnApplicationQuit() {
        clientTCP.Disconnect();
    }

}
