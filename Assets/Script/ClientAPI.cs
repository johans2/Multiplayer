using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ClientAPI : MonoBehaviour {

    public static List<SyncedBehaviour> syncedBehaviours = new List<SyncedBehaviour>();


    private void RequestSpawnObject(Action<GameObject> gameObject) {
        throw new NotImplementedException();
    }

    private void RequestDeleteObject() {
        throw new NotImplementedException();
    }

    public static void DeserializeFrame(byte[] frameData) {

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

        //Debug.Log(string.Format("Received data for {0} objects", numSyncedBehaviours));
        
    }

    private static SyncedBehaviour GetSyncedBehaviour(int id) {

        foreach(var syncedBehaviour in syncedBehaviours) {
            if(syncedBehaviour.ID == id) {
                return syncedBehaviour;
            }
        }

        return null;

    }

}
