using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    ///<summary>
    ///     Houses all of the game logic.
    ///</summary>
    public class GameLogic
    {
        ///<summary>
        ///     The update loop of the game, constantly updates the clients <see cref="Player.Update"/> and also updates the main thread <see cref="ThreadManager.UpdateMain"/>
        ///</summary>
        public static void Update() {
            
            //Update the clients if they aren't null
            foreach(Client _client in Server.clients.Values) {
                if(_client.player != null) {
                    _client.player.Update();
                }
            }
            
            ThreadManager.UpdateMain();
        }
    }
}