using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{   
    ///<summary>
    ///     Handles sending data to the client from the server    
    ///</summary>
    public class ServerSend
    {   
        ///<summary>
        ///     Send data to one client via TCP
        ///</summary>
        ///<param name="_toClient">The client id to sent the data to</param>
        ///<param name="_packet">The packet to send to the client</param>
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        ///<summary>
        ///     Send data to one client via UDP
        ///</summary>
        ///<param name="_toClient">The client id to sent the data to</param>
        ///<param name="_packet">The packet to send to the client</param>
        private static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].udp.SendData(_packet);
        }

        ///<summary>
        ///     Send data to all clients via TCP
        ///</summary>
        ///<param name="_packet">The packet to send to the clients</param>
        private static void SendTCPDataAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }

        ///<summary>
        ///     Send data to all clients via UDP
        ///</summary>
        ///<param name="_packet">The packet to send to the clients</param>
        private static void SendUDPDataAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }

        ///<summary>
        ///     Send data to all clients except one via TCP
        ///</summary>
        ///<param name="_exceptClient">The client id NOT to send the data to</param>
        ///<param name="_packet">The packet to send to the clients</param>
        private static void SendTCPDataAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }

        ///<summary>
        ///     Send data to all clients except one via UDP
        ///</summary>
        ///<param name="_exceptClient">The client id NOT to send the data to</param>
        ///<param name="_packet">
        ///     The packet to send to the clients
        ///     <see cref="Packet"/>
        ///</param>
        private static void SendUDPDataAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].udp.SendData(_packet);
                }
            }
        }

        ///<summary>
        ///     Sends the welcome packet to a client
        ///</summary>
        ///<param name="_toClient">The client to sent the welcome packet to</param>
        ///<param name="_msg">The message to send with the welcome packet</param>
        public static void Welcome(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
        }

        ///<summary>
        ///     Sends the SpawnPlayer packet to a client
        ///</summary>
        ///<param name="_toClient">The client to sent the welcome packet to</param>
        ///<param name="_player">
        ///     The message to spawn in the game. 
        ///     <see cref="Player"/>
        ///</param>
        public static void SpawnPlayer(int _toClient, Player _player) {
            using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer)) {
                _packet.Write(_player.id);
                _packet.Write(_player.username);
                _packet.Write(_player.position);
                _packet.Write(_player.rotation);

                SendTCPData(_toClient, _packet);
            }
        }

        ///<summary>
        ///     Sends the player position packet to a client
        ///</summary>
        ///<param name="_player">
        ///     The player's position to send
        ///     <see cref="Player"/>
        ///</param>       
        public static void PlayerPosition(Player _player) {
            using (Packet _packet = new Packet((int)ServerPackets.playerPosition)) {
                _packet.Write(_player.id);
                _packet.Write(_player.position);

                SendUDPDataAll(_packet);
            }
        }

        ///<summary>
        ///     Sends the player rotation packet to a client
        ///</summary>
        ///<param name="_player">
        ///     The player's rotation to send
        ///     <see cref="Player"/>
        ///</param>    
        public static void PlayerRotation(Player _player) {
            using(Packet _packet = new Packet((int)ServerPackets.playerRotation)){
                _packet.Write(_player.id);
                _packet.Write(_player.rotation);

                SendUDPDataAll(_player.id, _packet);
            }
        }
    }
}