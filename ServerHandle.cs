using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace GameServer
{
    ///<summary>
    ///     The methods for all of the different types of events the server can handle from the client.
    ///     <see cref="Server.PacketHandler"/>
    ///</summary>
    public class ServerHandle
    {
        ///<summary>
        ///     Handles the Welcome packet
        ///</summary>
        ///<param name="_fromClient">
        ///     What client id the packet is coming from
        ///</param>
        ///<param name="_packet">
        ///     The packet that was received from the client
        ///     <see cref="Packet"/>
        ///</packet>
        public static void WelcomeReceived(int _fromClient, Packet _packet) {
            //Read the data from the packet in the order it was written
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();

            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}");
            if(_fromClient != _clientIdCheck) {
                Console.WriteLine($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }

            //Send the client into the game
            Server.clients[_fromClient].SendIntoGame(_username);
        }
        ///<summary>
        ///     Handles the Player Movement Package
        ///</summary>
        ///<param name="_fromClient">
        ///     What client id the packet is coming from
        ///</param>
        ///<param name="_packet">
        ///     The packet that was received from the client
        ///     <see cref="Packet"/>
        ///</packet>
        public static void PlayerMovement(int _fromClient, Packet _packet) {
            //Read the data from the packet
            bool[] _inputs = new bool[_packet.ReadInt()];

            for (int i = 0; i < _inputs.Length; i++)
            {
                _inputs[i] = _packet.ReadBool();
            }

            Quaternion _rotation = _packet.ReadQuaternion();
            
            //Set the inputs
            Server.clients[_fromClient].player.SetInput(_inputs, _rotation);
        }

    }
}