using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientHandleNetworkData {

    public ClientAPI clientAPI; // TODO: make better reference.

    private delegate void PacketHandler(byte[] data);
    private static Dictionary<int, PacketHandler> packets;

    public static void InitializePackageHandlers() {
        Logger.Log("Initializing network package handlers.");

        packets = new Dictionary<int, PacketHandler> {
            { (int)ServerPackets.SConnectionOK, HandleConnectionOK },
            { (int)ServerPackets.SFrameUpdate, HandleFrameUpdate }
        };
    }

    public static void HandleNetworkInformation(byte[] data) {
        int packetNum;
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(data);
        packetNum = buffer.ReadInteger();
        buffer.Dispose();
        PacketHandler handler;

        if(packets.TryGetValue(packetNum, out handler)) {
            handler.Invoke(data);
        }
    }

    private static void HandleConnectionOK(byte[] data) {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(data);
        buffer.ReadInteger();
        string msg = buffer.ReadString();
        buffer.Dispose();

        Logger.Log(msg);

        ClientTCP.ThankYouServer();
    }

    private static void HandleFrameUpdate(byte[] data) {
        // Send to clietn api..?
        ClientAPI.DeserializeFrame(data);
    }
}
