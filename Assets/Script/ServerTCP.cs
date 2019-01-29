using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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
    private byte[] _buffer = new byte[1024]; // TODO: This is a probelm. The client can only send updated a 1024 bytes atm.
    private Thread clientThread;

    public Client(ServerPacketHandler packetHandler) {
        this.packetHandler = packetHandler;
    }

    public void StartClient() {
        //socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), socket); // This fucks things up..
        closing = false;
        clientThread = new Thread(DoRecieve);
        clientThread.Start();
    }

    private void DoRecieve() {
        while(!closing) {
            Recieve();
        }

    }
    
    private void OnReceive(IAsyncResult result) {
        while(!closing) {
            Recieve();
        }
    }
    
    private void Recieve() {
        byte[] _sizeInfo = new byte[4];

        int totalRead = 0;
        int currentRead = 0;

        try {
            currentRead = totalRead = socket.Receive(_sizeInfo);
            Debug.Log("Sizeinfo: " + _sizeInfo.Length);
            if(totalRead <= 0) {
                return;
            }
            else {
                while(totalRead < _sizeInfo.Length && currentRead > 0) {
                    currentRead = socket.Receive(_sizeInfo, totalRead, _sizeInfo.Length - totalRead, SocketFlags.None);
                    totalRead += currentRead;
                }

                int messagesize = 0;
                messagesize |= _sizeInfo[0];
                messagesize |= (_sizeInfo[1] << 8);
                messagesize |= (_sizeInfo[2] << 16);
                messagesize |= (_sizeInfo[3] << 24);

                byte[] data = new byte[messagesize];

                totalRead = 0;
                currentRead = totalRead = socket.Receive(data, totalRead, data.Length - totalRead, SocketFlags.None);

                while(totalRead < messagesize && currentRead > 0) {
                    currentRead = socket.Receive(data, totalRead, data.Length - totalRead, SocketFlags.None);
                    totalRead += currentRead;
                }

                // Handle the received packet..
                Debug.Log("Received " + data.Length + " bytes from client " + index);
                packetHandler.HandlePacket(index, data);
            }

        }
        catch(Exception ex) {
            Logger.Log("You are not connected to the server: " + ex.Message);
            //CloseClient(index);
        }


    }

    private void CloseClient(int index) {
        closing = true;
        clientThread.Join();
        Logger.Log(string.Format("Connection form {0}  has been terminated", ip));
        // Player left the game
        socket.Close();
    }

}
