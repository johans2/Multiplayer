using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ClientAPI : MonoBehaviour {

    private Dictionary<int, SyncedBehaviour> syncedBehaviours = new Dictionary<int, SyncedBehaviour>();

    private void RequestSpawnObject(Action<GameObject> gameObject) {
        throw new NotImplementedException();
    }

    private void RequestDeleteObject() {
        throw new NotImplementedException();
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
            SyncedBehaviour behaviour;
            if(!syncedBehaviours.TryGetValue(id, out behaviour)) {
                Debug.LogWarning("Receiving information about a synced objects that does not exist on this client.");
                continue;
            }

            behaviour.Deserialize(data);
        }




    }

}
