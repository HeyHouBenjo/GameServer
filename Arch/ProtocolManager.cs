using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using GameServer.Management;

namespace GameServer.Arch {

    public static class Listener {
        private static TcpListener _tcpListener;
        private static UdpClient _udpListener;

        private static Dictionary<int, Client> Clients => Server.Clients;

        public static void Start() {
            _tcpListener = new TcpListener(IPAddress.Any, Server.Port);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);

            _udpListener = new UdpClient(Server.Port);
            _udpListener.BeginReceive(UdpReceiveCallback, null);
        }

        private static void TcpConnectCallback(IAsyncResult result) {
            var client = _tcpListener.EndAcceptTcpClient(result);
            _tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            for (var i = 1; i <= Server.MaxPlayers; i++) {
                if (Clients[i].Tcp.Socket != null)
                    continue;

                Clients[i].Tcp.Connect(client);
                return;
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        private static void UdpReceiveCallback(IAsyncResult result) {
            try {
                var clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var data = _udpListener.EndReceive(result, ref clientEndPoint);
                _udpListener.BeginReceive(UdpReceiveCallback, null);

                if (data.Length < 4)
                    return;

                using var packet = new Packet(data);
                var clientId = packet.ReadInt();

                if (clientId == 0)
                    return;

                if (Clients[clientId].Tcp.Socket == null)
                    return;

                if (Clients[clientId].Udp.EndPoint == null) {
                    Clients[clientId].Udp.Connect(clientEndPoint);
                    return;
                }

                if (Clients[clientId].Udp.EndPoint.ToString() == clientEndPoint.ToString())
                    Clients[clientId].Udp.HandleData(packet);

            } catch (Exception ex) {
                Console.WriteLine($"Error receiving UDP data: {ex}");
            }
        }

        public static void SendUdpData(IPEndPoint clientEndPoint, Packet packet) {
            try {
                if (clientEndPoint != null)
                    _udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
            } catch (Exception ex) {
                Console.WriteLine($"Error sending data to {clientEndPoint} via UDP: {ex}");
            }
        }
    }

    public class TcpManager {
        private readonly int _id;
        private byte[] _receiveBuffer;
        private Packet _receivedData;
        private NetworkStream _stream;
        public TcpClient Socket;

        public TcpManager(int id) {
            _id = id;
        }

        public void Connect(TcpClient socket) {
            Socket = socket;
            Socket.ReceiveBufferSize = Constants.DataBufferSize;
            Socket.SendBufferSize = Constants.DataBufferSize;

            _stream = Socket.GetStream();

            _receivedData = new Packet();
            _receiveBuffer = new byte[Constants.DataBufferSize];

            _stream.BeginRead(_receiveBuffer, 0, Constants.DataBufferSize, ReceiveCallback, null);

            Server.Clients[_id].OnConnect();
        }

        public void SendData(Packet packet) {
            try {
                if (Socket != null)
                    _stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
            } catch (Exception ex) {
                Console.WriteLine($"Error sending data to player {_id} via TCP: {ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult result) {
            try {
                var byteLength = _stream.EndRead(result);
                if (byteLength <= 0) {
                    Server.Clients[_id].Disconnect();
                    return;
                }

                var data = new byte[byteLength];
                Array.Copy(_receiveBuffer, data, byteLength);

                _receivedData.Reset(HandleData(data));
                _stream.BeginRead(_receiveBuffer, 0, Constants.DataBufferSize, ReceiveCallback, null);
            } catch (Exception ex) {
                Console.WriteLine($"Error receiving TCP data: {ex}");
                Server.Clients[_id].Disconnect();
            }
        }

        private bool HandleData(byte[] data) {
            var packetLength = 0;

            _receivedData.SetBytes(data);

            if (_receivedData.UnreadLength() >= 4) {
                packetLength = _receivedData.ReadInt();
                if (packetLength <= 0)
                    return true;
            }

            while (packetLength > 0 && packetLength <= _receivedData.UnreadLength()) {
                var packetBytes = _receivedData.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() => ReadPacket.Read(packetBytes, _id));

                packetLength = 0;
                if (_receivedData.UnreadLength() < 4)
                    continue;

                packetLength = _receivedData.ReadInt();
                if (packetLength <= 0)
                    return true;
            }

            return packetLength <= 1;

        }

        public void Disconnect() {
            Socket.Close();
            _stream = null;
            _receivedData = null;
            _receiveBuffer = null;
            Socket = null;
        }
    }

    public class UdpManager {

        private readonly int _id;
        public IPEndPoint EndPoint;

        public UdpManager(int id) {
            _id = id;
        }

        public void Connect(IPEndPoint endPoint) {
            EndPoint = endPoint;
        }

        public void SendData(Packet packet) {
            Listener.SendUdpData(EndPoint, packet);
        }

        public void HandleData(Packet packetData) {
            var packetLength = packetData.ReadInt();
            var packetBytes = packetData.ReadBytes(packetLength);

            ThreadManager.ExecuteOnMainThread(() => ReadPacket.Read(packetBytes, _id));
        }

        public void Disconnect() {
            EndPoint = null;
        }
    }

    internal static class ReadPacket {
        public static void Read(byte[] packetBytes, int clientId) {
            using var packet = new Packet(packetBytes);

            var packetTypeId = packet.ReadInt();
            var packetActionId = packet.ReadInt();
            Server.PacketHandlers[packetTypeId][packetActionId](clientId, packet);
        }
    }
}
