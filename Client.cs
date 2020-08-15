using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
namespace GameServer
{
    ///<summary> 
    ///     Client class holding the UDP and TCP subclasses 
    ///</summary>
    class Client
    {
        ///<variable name="databufferSize" value="4096"> 
        ///     The size of the data buffer, defaulted to 4096
        ///</variable>
        public static int dataBufferSize = 4096;

        ///<variable name="id"> 
        ///     The clients ID
        ///</variable>
        public int id;

        ///<variable name="tcp"> 
        ///     Represents the instance of the tcp class <see cref="Client.TCP"/> created with the clients id
        ///</variable>
        public TCP tcp;

        ///<variable name="udp"> 
        ///     Represents the instance of the udp class <see cref="Client.UDP"/> created with the clients id
        ///</variable>
        public UDP udp;

        ///<variable name="player"> 
        ///     Represents the instance of the Player class <see cref="Player" /> that will be sent into the game <see cref="Client.SendIntoGame(string)"/>
        ///     and disconnected <see cref="Client.Disconnect" />
        ///</variable>
        public Player player;

        ///<summary> 
        ///     Client class constructor
        ///</summary>
        ///<param name="_clientID"> An int representing the clients id in the server> </param>
        public Client(int _clientID)
        {
            id = _clientID;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        ///<summary> 
        ///     The TCP class that handles TCP connections <see cref="Client.TCP.Connect(TcpClient)"/>,
        ///     sending data via tcp <see cref="Client.TCP.SendData(Packet)"/>, and handling received data <see cref="Client.TCP.HandleData(byte[])"/>.
        ///</summary>
        public class TCP
        {
            ///<variable name="socket"> 
            ///     The TcpClient <see cref="System.Net.Sockets.TcpClient"/> that handles recieve and send buffers. 
            ///</variable>
            public TcpClient socket;

            ///<variable name="stream"> 
            ///     The NetworkStream <see cref="System.Net.Sockets.NetworkStream" /> from the socket <see cref="Client.TCP.socket"> that 
            ///     reads to receive buffer. <see cref="Client.TCP.receiveBuffer"> 
            ///</variable>
            private NetworkStream stream;

            ///<variable name="recievedData">
            ///   The Packet <see cref="Packet">  that holds the received data 
            ///</variable>
            private Packet receivedData;

            ///<variable name="receiveBuffer">
            ///     The byte array that holds the data read from the stream <see cref="Client.TCP.stream"/>
            ///</variable>
            private byte[] receiveBuffer;

            ///<variable name="id">
            ///     Storing the id we receive from <see cref="Client.id"/>
            ///</variable>
            private readonly int id;

            ///<summary> 
            ///     The TCP class constructor
            ///</summary>
            ///<param name="_id"> The id of the client that we receive from <see cref="Client.id"/> </param>
            public TCP(int _id)
            {
                id = _id;
            }

            ///<summary>
            ///     The connect function. It initializes the socket <see cref="Client.TCP.socket">, the stream <see cref="Client.TCP.stream">, 
            ///     the receive buffer <see cref="Client.TCP.receiveBuffer"/>, and get the recieved data <see cref="Client.TCP.receivedData"/>
            ///</summary>
            ///<param name="_socket">
            ///     The TcpClient <see cref="System.Net.Sockets.TcpClient"/> representing the socket connection
            ///</param>
            public void Connect(TcpClient _socket)
            {
                //Set the socket and its receive and send buffer sizes
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                //Get the stream from the socket
                stream = socket.GetStream();

                //Initialize receivedData and receiveBuffer
                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                //Begin reading from the stream, this calls the ReceiveCallback function 
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                //Send the Welcome packet
                ServerSend.Welcome(id, "Dick and balls");
            }

            ///<summary>
            ///     Send a packet to the client
            ///</summary>
            ///<param name="_packet">
            ///     The Packet <see cref="Packet"/> to send to the client
            ///</param>
            public void SendData(Packet _packet)
            {   
                //Put it in a try catch so the server doesn't crash on an exception
                try
                {   
                    //If the socket isnt null, write to the stream.
                    if (socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {   
                    //Log the exception.
                    Console.WriteLine($"Error sending data to player {id} via TCP: {e}");
                }
            }

            ///<summary>
            ///     Receiving the callback from beginning to read the stream. <see cref="Client.TCP.Connect(TcpClient)">
            ///</summary>
            ///<param name="_result">
            ///         The IAsyncResult <see cref="System.IAsyncResult"/> that gets passed through by NetworkStrea.BeginRead 
            ///         <see cref="System.Net.Sockets.NetworkStream.BeginRead(byte[], int, int, AsyncCallback, object)"/>
            ///</param>
            private void ReceiveCallback(IAsyncResult _result)
            {   
                //Try catch to avoid crashing server
                try
                {
                    //End reading the stream and hold the length in a variable
                    int _byteLength = stream.EndRead(_result);

                    //If the length is zero somehow then just gtfo and disconnect.
                    if (_byteLength <= 0)
                    {
                        Server.clients[id].Disconnect();
                        return;
                    }

                    //Create an array to store the data, which is of the same length as we got before.
                    byte[] _data = new byte[_byteLength];

                    //Copy that data to the receiveBuffer
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    //Check if we need to reset the receivedData packet so it can be reused
                    receivedData.Reset(HandleData(_data));

                    //Restart the read
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    //Log the error
                    Console.WriteLine($"Error: {e}");

                    //Disconnect on error
                    Server.clients[id].Disconnect();
                }
            }

            ///<summary>
            ///     Handle the data to determine if we need to reset the packet so it can be reused <see cref="Packet.Reset(bool)"/>
            ///</summary>
            ///<param name="_data">
            ///     The bytearray of the data to be handled.
            ///</param>
            ///<returns>
            ///     If we reset or not
            ///</returns>
            private bool HandleData(byte[] _data)
            {   
                int _packetLength = 0;

                //set receivedData to the data that we pass in 
                receivedData.SetBytes(_data);

                //the reason that 4 is used here is because there are 4 bytes at the start of the array iirc
                if (receivedData.UnreadLength() >= 4)
                {
                    //Read the length
                    _packetLength = receivedData.ReadInt();

                    //If its somehow 0 or somehow less than 0, we gotta reset that shit
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }

                //while the packetlength is bigger than 0 and <= to the length of the data we got 
                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
                {
                    //Get the bytes from the data we got and store it somewhere else for processing.
                    byte[] _packetBytes = receivedData.ReadBytes(_packetLength);

                    //We want to only use the main thread cuz threads confusing.
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        //Create and dipose of a packet
                        using (Packet _packet = new Packet(_packetBytes))
                        {   
                            //read the packet's id and handle it correctly
                            int _packetId = _packet.ReadInt();
                            Server.packetHandlers[_packetId](id, _packet);
                        }
                    });
                    
                    //Same thing as above, we want to check if the remaining length of the data we got is >= 4 since there are 4-non-data-bytes, and then 
                    //decide if we need to reset or not.
                    _packetLength = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        _packetLength = receivedData.ReadInt();
                        if (_packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }
                
                //I forget what this one does but it handles an error iirc.
                if (_packetLength <= 1)
                {
                    return true;
                }

                //If we make it this far we don't need to reset.
                return false;
            }

            ///<summary>
            ///     Handle disconnect.
            ///</summary>
            public void Disconnect()
            {
                //Close the socket and set all the shit to null
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        ///<summary>
        ///     The UDP class that handles connections <see cref="Client.UDP.Connect(IPEndPoint)"/>, sending data <see cref="Client.UDP.SendData(Packet)"/>
        ///     and Handling Data <see cref="Client.UDP.HandleData(Packet)"/>
        ///</summary>
        public class UDP
        {
            ///<variable name="endPoint">
            ///     The IPEndPoint <see cref="System.Net.IPEndPoint"/> that represents the endpoint for udp connection
            ///</variable>
            public IPEndPoint endPoint;

            ///<variable name="id">
            ///     The id of the client <see cref="Client.id">
            ///</variable>
            private int id;

            ///<summary>
            ///     UDP class constructor
            ///</summary>
            ///<param name="_id">
            ///     The id of the client <see cref="Client.id">
            ///</param>
            public UDP(int _id)
            {
                id = _id;
            }

            
            public void Connect(IPEndPoint _endPoint)
            {
                endPoint = _endPoint;
            }

            public void SendData(Packet _packet)
            {
                Server.SendUDPData(endPoint, _packet);
            }

            public void HandleData(Packet _packetData)
            {
                int _packetLength = _packetData.ReadInt();
                byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id, _packet);
                    }
                });
            }

            public void Disconnect()
            {
                endPoint = null;
            }
        }

        public void SendIntoGame(string _playerName)
        {
            player = new Player(id, _playerName, new Vector3(0, 0, 0));

            foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    if (_client.id != id)
                    {
                        ServerSend.SpawnPlayer(id, _client.player);
                    }
                }
            }

            foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    ServerSend.SpawnPlayer(_client.id, player);
                }
            }
        }

        public void Disconnect()
        {
            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected");

            player = null;
            tcp.Disconnect();
            udp.Disconnect();
        }
    }
}
