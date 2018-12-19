using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerEngine : MonoBehaviour {
    
    public List<SyncedBehaviour> syncedBehaviours = new List<SyncedBehaviour>();

    private ServerTCPConnection serverTCP;

    int objectID = 0;
    int sendFrame = 60;
    int frame = 0;

    
    private void Awake() {
        ServerPacketHandler packetHandler = new ServerPacketHandler(this);
        serverTCP = new ServerTCPConnection(packetHandler);
        serverTCP.SetupServer();
    }
    
    private void LateUpdate() {
        frame++;

        if(frame == sendFrame) {
            SerializeFrame();
            frame = 0;
        }
    }

    public static void SpawnObject(GameObject prefab) {
        PacketBuffer buffer = new PacketBuffer();
        

    }

    public static void DestroyObject(SyncedBehaviour syncedBehaviour) {


    }

    private void SerializeFrame() {
        Debug.Log("Serializing frame..");
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

        //ssionDebug.Log(string.Format("Sending data for {0} synced obejcts, ", numSyncedBehaviours));

        serverTCP.SendData(buffer.ToArray());
    }

    

}
