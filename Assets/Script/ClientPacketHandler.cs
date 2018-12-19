using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientPacketHandler {

    private ClientEngine engine;
    private delegate void PacketHandler(byte[] data);
    private static Dictionary<int, PacketHandler> packets;
    

    public ClientPacketHandler(ClientEngine engine) {
        this.engine = engine;
        InitializePackageHandlers();
    }
    
    public void InitializePackageHandlers() {
        Logger.Log("Initializing network package handlers.");

        packets = new Dictionary<int, PacketHandler> {
            { (int)ServerPackets.SConnectionOK, HandleConnectionOK },
            { (int)ServerPackets.SFrameUpdate, HandleFrameUpdate },
            { (int)ServerPackets.SSpawnObject, HandleSpawnObject }
        };
    }

    public void HandlePacket(byte[] data) {
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

    private void HandleConnectionOK(byte[] data) {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(data);
        buffer.ReadInteger();
        string msg = buffer.ReadString();
        buffer.Dispose();

        Logger.Log(msg);
    }

    private void HandleFrameUpdate(byte[] data) {
        Debug.Log("Got a frame update!");
        engine.QueueFrame(data);
    }

    private void HandleSpawnObject(byte[] data) {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteBytes(data);
    }

}
