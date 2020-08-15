using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    class Server
    {   
        ///<summary>
        ///     The max players that can be on the server at one time <DECISION> This might have to change if the server runs multiple games? and spectators? </DECISION>
        ///</summary>
        public static int MaxPlayers { get; private set; }

        ///<summary>
        ///     The port the server will use
        ///</summary>
        public static int Port { get; private set; }

        ///<summary>
        ///     A dictionary representing the clients. Using their ids as keys. <see cref="Client"/>
        ///</summary>
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        ///<summary>
        ///     The TcpListener the server will reference. <see cref="System.Net.Sockets.TcpListener"/>
        ///</summary>
        private static TcpListener tcpListener;

        ///<summary>
        ///     The UdpClient the server will reference. <see cref="System.Net.Sockets.UdpClient"/>
        ///</summary>
        private static UdpClient udpListener;

        ///<summary>
        ///     A delegate to represent the format of packet handlers.
        ///</summary>
        public delegate void PacketHandler(int _fromClient, Packet _packet);

        ///<summary>
        ///     The packet handlers associated to their enum keys.
        ///</summary>
        public static Dictionary<int, PacketHandler> packetHandlers;

        ///<summary>
        ///     
        ///</summary>
        ///<param name="_maxPlayers">
        ///
        ///</param>
        ///<param name="_port">
        ///
        ///</param>
        public static void Start(int _maxPlayers, int _port)
        {
            MaxPlayers = _maxPlayers;
            Port = _port;

            Console.WriteLine("Starting...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallBack), null);

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Console.WriteLine($"Server started on Port: {Port}");
        }

        ///<summary>
        ///     The callback function for TCP connections
        ///</summary>
        ///<param>
        ///     The IAsyncResult that gets passed with the callback. <see cref="System.IAsyncResult"/>
        ///</param>
        private static void TCPConnectCallBack(IAsyncResult _result)
        {   
            //Stop accepting connections and store the currently accepted one
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);

            //Restart accepting new clients.
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallBack), null);
            Console.WriteLine($"Incoming connection from {_client.Client.RemoteEndPoint}");

            //Connect the client
            for (int i = 1; i < MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }

            Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        ///<summary>
        ///     The callback function for UDP connections
        ///</summary>
        ///<param>
        ///     The IAsyncResult that gets passed with the callback. <see cref="System.IAsyncResult"/>
        ///</param>
        private static void UDPReceiveCallback(IAsyncResult _result){
            try 
            {
                //Stop receiving new connections and store what we've gotten
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);

                //Start receiving connections again
                udpListener.BeginReceive(UDPReceiveCallback, null);

                //If the data isn't big enough just stop.
                if(_data.Length < 4) {
                    return;
                }

                //Do some things based on the clientID of the packet
                using(Packet _packet = new Packet(_data)) {
                    int _clientId = _packet.ReadInt();

                    //We start at 1 for the clientid
                    if(_clientId == 0) {
                        return;
                    }

                    //If there isn't an end point, then connect to one
                    if(clients[_clientId].udp.endPoint == null) {
                        clients[_clientId].udp.Connect(_clientEndPoint);
                        return;
                    }

                    //If the endpoints are the same then handle the data of the packet
                    if(clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString()) {
                        clients[_clientId].udp.HandleData(_packet);
                    }
                }
            }
            catch (Exception e){
                Console.WriteLine($"Error recieving data via UDP: {e}");
            }
        }

        ///<summary>
        ///     Send data to the client via UDP
        ///</summary>
        ///<param name="_clientEndPoint">
        ///     The endpoint on the client side to send the data to 
        ///</param>
        ///<param name ="_packet">
        ///     The packet to send <see cref="Packet"/>
        ///</param>
        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet) {
            try {
                //if the client endpoint exists, the start sending the datta to the client.
                if(_clientEndPoint != null) {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch(Exception e) {
                Console.WriteLine($"Error sending data to {_clientEndPoint} via UDP: {e}");
            }
        }

        ///<summary>
        ///     Initialize all the data on the server including filling the clients dictionary and packet handlers dictionary.
        ///     <see cref="Server.clients"/> <see cref="Server.packetHandlers"/>
        ///</summary>
        private static void InitializeServerData()
        {
            //Fill up the server with empty clients that we can let people take over
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            //Create all of the different packet handlers and give them the correct enum to tie to. 
            packetHandlers = new Dictionary<int, PacketHandler>() {
                {(int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived},
                {(int)ClientPackets.playerMovement, ServerHandle.PlayerMovement},
            };

            Console.WriteLine("Packets initialized");
        }
    }
}