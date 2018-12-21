using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Assertions;

public class ClientEngine : MonoBehaviour {

    public SyncedPrefabRegistry registryPrefab;
    
    private List<SyncedEntity> syncedEntities = new List<SyncedEntity>();
    private SyncedPrefabRegistry registry;
    private ClientTCPConnection clientTCP;
    private Queue frameQueue = Queue.Synchronized(new Queue());
    private Queue spawnObjectQueue = Queue.Synchronized(new Queue());
    private Queue destroyObjectQueue = Queue.Synchronized(new Queue());

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

        while(destroyObjectQueue.Count > 0) {
            DestroySyncedObejct((byte[])destroyObjectQueue.Dequeue());
        }
    }

    public void QueueFrameUpdate(byte[] frameData) {
        frameQueue.Enqueue(frameData);
    }

    public void QueueObjectSpawn(byte[] objectData) {
        spawnObjectQueue.Enqueue(objectData);
    }

    public void QueueObjectDestroy(byte[] objectData) {
        destroyObjectQueue.Enqueue(objectData);
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

        // Set the ID from the received server data
        SyncedEntity entity = go.GetComponent<SyncedEntity>();
        int entityID = buffer.ReadInteger();
        entity.ID = entityID;
        
        // Set the transform properties.
        Transform goTransform = go.transform;
        goTransform.position = buffer.ReadVector3();
        goTransform.rotation = Quaternion.Euler(buffer.ReadVector3());
        goTransform.localScale = buffer.ReadVector3();

        syncedEntities.Add(entity);
        
        buffer.Dispose();
    }

    private void DestroySyncedObejct(byte[] objectData) {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(objectData);
        buffer.ReadInteger(); // Packet ID int
        int entityID = buffer.ReadInteger();

        SyncedEntity entityToDestroy = GetSyncedEntity(entityID);
        syncedEntities.Remove(entityToDestroy);

        Destroy(entityToDestroy.gameObject);

        buffer.Dispose();
    }

    private void DeserializeFrame(byte[] frameData) {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(frameData);
        buffer.ReadInteger(); // Packet id int

        int numEntities = buffer.ReadInteger();

        for(int i = 0; i < numEntities; i++) {

            // Read entity ID, and get it form the list.
            int entityID = buffer.ReadInteger();
            SyncedEntity entity = GetSyncedEntity(entityID);
            
            // Deserialize all behaviours
            foreach(var syncedBehaviour in entity.syncedBehaviours) {
                int dataSize = buffer.ReadInteger();
                byte[] data = buffer.ReadBytes(dataSize);
                syncedBehaviour.Deserialize(data);
            }
        }

        buffer.Dispose();
    }
    
    private SyncedEntity GetSyncedEntity(int id) {

        foreach(var entity in syncedEntities) {
            if(entity.ID == id) {
                return entity;
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
