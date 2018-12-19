using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerEngine : MonoBehaviour {
    
    public List<SyncedBehaviour> syncedBehaviours = new List<SyncedBehaviour>();
    public int serverFrameRate = 20;

    private ServerTCPConnection serverTCP;

    int objectID = 0;
    
    private void Awake() {
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
