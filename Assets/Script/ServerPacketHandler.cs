using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerPacketHandler {

    private ServerEngine engine;
    private delegate void PacketHandler(int index, byte[] data);
    private static Dictionary<int, PacketHandler> packets;

    public ServerPacketHandler(ServerEngine engine) {
        this.engine = engine;
        InitializePackageHandlers();
    }
    
    public void InitializePackageHandlers() {
        Logger.Log("Initializing server package handlers.");

        packets = new Dictionary<int, PacketHandler> {
            { (int)ClientPackets.CThankYou, HandleThankYou }
        };
    }

    public void HandlePacket(int index, byte[] data) {
        int packetNum;
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(data);
        packetNum = buffer.ReadInteger();
        buffer.Dispose();
        PacketHandler handler;

        if(packets.TryGetValue(packetNum, out handler)) {
            handler.Invoke(index, data);
        }
    }

    private void HandleThankYou(int index, byte[] data) {
        /*PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(data);
        buffer.ReadInteger();
        string msg = buffer.ReadString();
        buffer.Dispose();

        Debug.Log(msg);
        Logger.Log(msg);
        */
        engine.QueueClientInput(data);
    }

}
