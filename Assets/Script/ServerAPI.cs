using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerAPI : MonoBehaviour {



    private List<SyncedBehaviour> syncedBehaviours = new List<SyncedBehaviour>();

    int objectID = 0;

    public static void SpawnObject(GameObject prefab) {
        PacketBuffer buffer = new PacketBuffer();



    }

    public static void DestroyObject(SyncedBehaviour syncedBehaviour) {


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

        ServerTCP.SendData(buffer.ToArray());

    }

    

}
