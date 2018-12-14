using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandleNetworkData {

    private delegate void PacketHandler(int index, byte[] data);
    private static Dictionary<int, PacketHandler> packets;

    public static void InitializePackageHandlers() {
        Logger.Log("Initializing network package handlers.");

        packets = new Dictionary<int, PacketHandler> {
            { (int)ClientPackets.CThankYou, HandleThankYou }
        };
    }

    public static void HandleNetworkInformation(int index, byte[] data) {
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

    private static void HandleThankYou(int index, byte[] data) {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(data);
        buffer.ReadInteger();
        string msg = buffer.ReadString();
        buffer.Dispose();

        Logger.Log(msg);
    }

}
