using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ServerTCP {

    private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private static byte[] _buffer = new byte[1024];

    public static Client[] _clients = new Client[Constants.MAX_PLAYERS];

    public static void SetupServer() {
        Logger.Log("Starting server..");

        for(int i = 0; i < Constants.MAX_PLAYERS; i++) {
            _clients[i] = new Client();
        }

        _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 5555));
        _serverSocket.Listen(10);
        _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

        Logger.Log("Server started.");
    }

    private static void AcceptCallback(IAsyncResult result) {
        Socket socket = _serverSocket.EndAccept(result);
        _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

        // Find a free slot for the player
        for(int i = 0; i < Constants.MAX_PLAYERS; i++) {
            if(_clients[i].socket == null) {
                _clients[i].socket = socket;
                _clients[i].index = i;
                _clients[i].ip = socket.RemoteEndPoint.ToString();
                _clients[i].StartClient();
                Logger.Log(string.Format("Connection form {0} recieved ", _clients[i].ip));
                SendConnectionOK(i);
                return;
            }
        }
    }

    public static void SendData(byte[] bytes) {
        foreach(var client in _clients) {
            if(client.socket != null) {
                SendDataTo(client.index, bytes);
            }
        }
    }

    public static void SendDataTo(int index, byte[] data) {
        byte[] sizeInfo = new byte[4];
        sizeInfo[0] = (byte)data.Length;
        sizeInfo[1] = (byte)(data.Length >> 8);
        sizeInfo[2] = (byte)(data.Length >> 16);
        sizeInfo[3] = (byte)(data.Length >> 24);

        _clients[index].socket.Send(sizeInfo);
        _clients[index].socket.Send(data);
    }

    public static void SendConnectionOK(int index) {
        Logger.Log(string.Format("Sending connection OK to client: {0}", index));
        PacketBuffer buffer = new PacketBuffer();
        buffer.WriteInteger((int)ServerPackets.SConnectionOK);
        buffer.WriteString("Connection success.");
        SendDataTo(index, buffer.ToArray());
        buffer.Dispose();
    }

}


public class Client {
    public int index;
    public string ip;
    public Socket socket;
    public bool closing = false;
    private byte[] _buffer = new byte[1024];

    public void StartClient() {
        socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
        closing = false;
    }

    private void RecieveCallback(IAsyncResult result) {
        Socket socket = (Socket)result.AsyncState;

        try {
            int received = socket.EndReceive(result);
            if(received <= 0) {
                CloseClient(index);
            }
            else {
                byte[] dataBuffer = new byte[received];
                Array.Copy(_buffer, dataBuffer, received);
                ServerHandleNetworkData.HandleNetworkInformation(index, dataBuffer);
                socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
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
