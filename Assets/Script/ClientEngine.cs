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
    private Queue serverUpdates = Queue.Synchronized(new Queue());

    private void Awake() {
        Assert.IsNotNull(registryPrefab, "Missing prefab ergistry.");
        registry = Instantiate(registryPrefab);

        ClientPacketHandler packetHandler = new ClientPacketHandler(this);
        clientTCP = new ClientTCPConnection(packetHandler);
        clientTCP.ConnectToServer();
    }

    private void LateUpdate() {
        while(serverUpdates.Count > 0) {
            ParseServerUpdate((byte[])serverUpdates.Dequeue());
        }
    }

    public void QueueServerUpdate(byte[] serverUpdate) {
        serverUpdates.Enqueue(serverUpdate);
    }
    
    private void ParseServerUpdate(byte[] serverUpdate) {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(serverUpdate);
        ServerPackets updateType = (ServerPackets)buffer.ReadInteger();

        switch(updateType) {
            case ServerPackets.SFrameUpdate:
                DeserializeFrame(buffer);
                break;
            case ServerPackets.SSpawnObject:
                SpawnSyncedObject(buffer);
                break;
            case ServerPackets.SDestroyObject:
                SpawnSyncedObject(buffer);
                break;
            default:
                break;
        }
    }

    private void SpawnSyncedObject(PacketBuffer buffer) {
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

    private void DestroySyncedObejct(PacketBuffer buffer) {
        int entityID = buffer.ReadInteger();

        SyncedEntity entityToDestroy = GetSyncedEntity(entityID);
        syncedEntities.Remove(entityToDestroy);

        Destroy(entityToDestroy.gameObject);

        buffer.Dispose();
    }

    private void DeserializeFrame(PacketBuffer buffer) {
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
