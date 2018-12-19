using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ClientEngine : MonoBehaviour {

    public List<SyncedBehaviour> syncedBehaviours = new List<SyncedBehaviour>();

    private Queue frameQueue = Queue.Synchronized(new Queue());
    private ClientTCPConnection clientTCP;
    
    private void Awake() {
        ClientPacketHandler packetHandler = new ClientPacketHandler(this);
        clientTCP = new ClientTCPConnection(packetHandler);
        

        clientTCP.ConnectToServer();
    }

    private void LateUpdate() {
        while(frameQueue.Count > 0) {
            DeserializeFrame((byte[])frameQueue.Dequeue());
        }
    }

    public void QueueFrame(byte[] frame) {
        frameQueue.Enqueue(frame);
    }

    private void DeserializeFrame(byte[] frameData) {
        if(frameData == null) {
            Debug.LogWarning("Received a frame is null.");
            return;
        }
        
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
