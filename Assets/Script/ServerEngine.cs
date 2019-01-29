using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System;

public class ServerEngine : MonoBehaviour {

    public SyncedPrefabRegistry registryPrefab;
    public int serverFrameRate = 20;

    private List<SyncedEntity> syncedEntities = new List<SyncedEntity>();
    private SyncedPrefabRegistry registry;
    private ServerTCPConnection serverTCP;
    private int syncedEntityID = 0;
    private Queue clientInput = Queue.Synchronized(new Queue());

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

        Debug.Log("Client inputs: " + clientInput.Count);
        
        if(clientInput.Count > 0) {
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteBytes((byte[])clientInput.Dequeue());
            int packInt = buffer.ReadInteger();
            string msg = buffer.ReadString();
            Logger.Log(msg);
            Debug.Log(msg);
            buffer.Dispose();
        }
    }

    public void SpawnObject(GameObject prefab, Vector3 position, Vector3 rotation, Vector3 scale) {
        // Spawn object, set script IDs and add them to the synced list.
        GameObject go = Instantiate(prefab, position, Quaternion.Euler(rotation));

        SyncedEntity entity = go.GetComponent<SyncedEntity>();
        entity.ID = syncedEntityID;
        syncedEntityID++;
        
        syncedEntities.Add(entity);

        // Send the spawndata to all clients.
        PacketBuffer buffer = new PacketBuffer();
        int prefabID = registryPrefab.GetPrefabID(prefab);
        buffer.WriteInteger((int)ServerPackets.SSpawnObject);
        buffer.WriteInteger(prefabID);
        buffer.WriteInteger(entity.ID);
        
        buffer.WriteVector3(position);
        buffer.WriteVector3(rotation);
        buffer.WriteVector3(scale);

        serverTCP.SendData(buffer.ToArray());

        buffer.Dispose();
    }
    
    public void DestroyObject(int entityID) {
        SyncedEntity entityToDestroy = GetEntity(entityID);

        if(entityToDestroy == null) {
            Debug.LogWarningFormat("Entity with ID {0} does not exist and cannot  be destroyed.", entityID);
            return;
        }

        syncedEntities.Remove(entityToDestroy);
        Destroy(entityToDestroy.gameObject);

        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteInteger((int)ServerPackets.SDestroyObject);
        buffer.WriteInteger(entityID);

        serverTCP.SendData(buffer.ToArray());

        buffer.Dispose();
    }

    public void QueueClientInput(byte[] data) {
        clientInput.Enqueue(data);
    }

    private void ParseClientInput(byte[] data) {
        PacketBuffer buffer = new PacketBuffer();

    }


    private void SerializeFrame() {
        PacketBuffer buffer = new PacketBuffer();

        int numEntities = syncedEntities.Count;

        buffer.WriteInteger((int)ServerPackets.SFrameUpdate);
        buffer.WriteInteger(numEntities);
        
        // TODO: use C# job system for this?
        foreach(var entity in syncedEntities) {

            buffer.WriteInteger(entity.ID);

            foreach(var syncedBehaviour in entity.syncedBehaviours) {
                byte[] data = syncedBehaviour.Serialize();
                int dataSize = data.Length;
                buffer.WriteInteger(dataSize);
                buffer.WriteBytes(data);
            }
        }

        //Debug.Log(string.Format("Sending data for {0} synced obejcts, ", numEntities));

        serverTCP.SendData(buffer.ToArray());

        buffer.Dispose();
    }

    private SyncedEntity GetEntity(int entityID) {
        foreach(var entity in syncedEntities) {
            if(entity.ID == entityID) {
                return entity;
            }
        }

        Debug.LogWarningFormat("Entity with ID {0} does not exist", entityID);
        return null;
    }

    

}
