using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ServerTCPConnection {
    
    private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private Client[] clients = new Client[Constants.MAX_PLAYERS];
    private ServerPacketHandler packetHandler;

    //private static byte[] _buffer = new byte[1024];
    
    public ServerTCPConnection(ServerPacketHandler packetHandler /*, TODO: ServerConfig config*/) {
        this.packetHandler = packetHandler;
    }

    public void SetupServer() {
        Logger.Log("Starting server..");

        for(int i = 0; i < Constants.MAX_PLAYERS; i++) {
            clients[i] = new Client(packetHandler);
        }

        serverSocket.Bind(new IPEndPoint(IPAddress.Any, 5555));
        serverSocket.Listen(10);
        serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

        Logger.Log("Server started.");
    }

    private void AcceptCallback(IAsyncResult result) {
        Socket socket = serverSocket.EndAccept(result);
        serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

        // Find a free slot for the player
        for(int i = 0; i < Constants.MAX_PLAYERS; i++) {
            if(clients[i].socket == null) {
                clients[i].socket = socket;
                clients[i].index = i;
                clients[i].ip = socket.RemoteEndPoint.ToString();
                clients[i].StartClient();
                Logger.Log(string.Format("Connection form {0} recieved ", clients[i].ip));
                SendConnectionOK(i);
                return;
            }
        }
    }

    public void SendData(byte[] bytes) {
        foreach(var client in clients) {
            if(client.socket != null) {
                SendDataTo(client.index, bytes);
            }
        }
    }

    public void SendDataTo(int index, byte[] data) {
        byte[] sizeInfo = new byte[4];
        sizeInfo[0] = (byte)data.Length;
        sizeInfo[1] = (byte)(data.Length >> 8);
        sizeInfo[2] = (byte)(data.Length >> 16);
        sizeInfo[3] = (byte)(data.Length >> 24);

        clients[index].socket.Send(sizeInfo);
        clients[index].socket.Send(data);
    }

    public void SendConnectionOK(int index) {
        Logger.Log(string.Format("Sending connection OK to client: {0}", index));
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteInteger((int)ServerPackets.SConnectionOK);
        buffer.WriteString("Connection success.");
        SendDataTo(index, buffer.ToArray());
        buffer.Dispose();
    }

}

/// <summary>
/// A single connected client TCP setup.
/// </summary>
public class Client {
    public int index;
    public string ip;
    public Socket socket;
    public bool closing = false;

    private ServerPacketHandler packetHandler;
    private byte[] _buffer = new byte[1024];
    
    public Client(ServerPacketHandler packetHandler) {
        this.packetHandler = packetHandler;
    }

    public void StartClient() {
        socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), socket);
        closing = false;
    }

    private void OnReceive(IAsyncResult result) {
        Socket socket = (Socket)result.AsyncState;

        try {
            int received = socket.EndReceive(result);
            if(received <= 0) {
                CloseClient(index);
            }
            else {
                byte[] dataBuffer = new byte[received];
                Array.Copy(_buffer, dataBuffer, received);
                packetHandler.HandlePacket(index, dataBuffer);
                socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), socket);
            }
        }
        catch {
            CloseClient(index);
        }

    }

    private void CloseClient(int index) {
        closing = true;
        Logger.Log(string.Format("Connection form {0}  has been terminated", ip));
        // Player left the game
        socket.Close();
    }

}
