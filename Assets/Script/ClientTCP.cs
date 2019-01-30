using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;

public class ClientTCPConnection {

    private ClientPacketHandler packetHandler;
    private static Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private bool receive = false;
    
    public ClientTCPConnection(ClientPacketHandler packetHandler) {
        this.packetHandler = packetHandler;
    }
    
    public void ConnectToServer() {
        Logger.Log("Connecting to server..");
        clientSocket.BeginConnect("127.0.0.1", 5555, new AsyncCallback(ConnectCallback), clientSocket);
    }

    private void ConnectCallback(IAsyncResult result) {
        clientSocket.EndConnect(result);
        receive = true;
        Logger.Log("Connected to server. Receiving data..");
        
        while(receive) {
            OnReceive();
        }
    }

    private void OnReceive() {
        byte[] _sizeInfo = new byte[4];
        
        int totalRead = 0;
        int currentRead = 0;

        try {
            currentRead = totalRead = clientSocket.Receive(_sizeInfo);
            if(totalRead <= 0) {
                Logger.Log("You are not connected to the server (readbuffer 0 bytes)");
            }
            else {
                // Read message size info
                while(totalRead < _sizeInfo.Length && currentRead > 0) {
                    currentRead = clientSocket.Receive(_sizeInfo, totalRead, _sizeInfo.Length - totalRead, SocketFlags.None);
                    totalRead += currentRead;
                }

                int messagesize = 0;
                messagesize |= _sizeInfo[0];
                messagesize |= (_sizeInfo[1] << 8);
                messagesize |= (_sizeInfo[2] << 16);
                messagesize |= (_sizeInfo[3] << 24);

                // Read message data
                byte[] data = new byte[messagesize];

                totalRead = 0;
                currentRead = totalRead = clientSocket.Receive(data, totalRead, data.Length - totalRead, SocketFlags.None);

                while(totalRead < messagesize && currentRead > 0) {
                    currentRead = clientSocket.Receive(data, totalRead, data.Length - totalRead, SocketFlags.None);
                    totalRead += currentRead;
                }

                // Handle the received packet
                packetHandler.HandlePacket(data);
            }
        }
        catch(Exception ex) {
            Logger.Log("You are not connected to the server: " + ex.Message);
            Disconnect();
        }
    }

    // WIP:
    public void SendDataToServer(byte[] data) {
        byte[] sizeInfo = new byte[4];
        sizeInfo[0] = (byte)data.Length;
        sizeInfo[1] = (byte)(data.Length >> 8);
        sizeInfo[2] = (byte)(data.Length >> 16);
        sizeInfo[3] = (byte)(data.Length >> 24);

        clientSocket.Send(sizeInfo);
        clientSocket.Send(data);
        Logger.Log("Sending " + data.Length + "bytes to the server");
    }

    public void ThankYouServer() {
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteInteger((int)ClientPackets.CThankYou);
        buffer.WriteString("HELLO SERVER " + Time.timeSinceLevelLoad);
        

        SendDataToServer(buffer.ToArray());
        buffer.Dispose();
    }

    public void Disconnect() {
        Debug.Log("Disconnecting client");
        receive = false;
        clientSocket.Close();
    }

}
