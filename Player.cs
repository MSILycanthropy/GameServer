using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace GameServer
{
    ///<summary>
    ///     The class that represents the Player on the server
    ///</summary>
    public class Player
    {
        ///<summary name="id">
        ///     The client's id of the player
        ///</summary>
        public int id;

        ///<summary name="username">
        ///     The player's username.
        ///</summary>
        public string username;

        ///<summary name="position">
        ///     The player's position as a Vector3. This is generally going to be server authoritative. 
        ///</summary>
        public Vector3 position;

        ///<summary name="rotation">
        ///     The player's rotation as a Quaternion. <DECISION> IS THIS GOING TO BE SERVER AUTHORITATIVE? </DECISION>
        ///</summary>
        public Quaternion rotation;

        ///<summary> 
        ///     The player's velocity used to predict their position. <DECISION> IS THIS GOING TO BE SERVER AUTHORITATIVE? AND HOW USED? </DECISION>
        ///</summary>
        public Vector3 velocity;

        ///<summary>
        ///     The player's inputs
        ///</summary>
        private Vector2 inputs;

        ///<summary>
        ///     The player class constructor. <see cref="Player"/>
        ///</summary>
        ///<param name="_id">The clients id to give to associate with the player.</param>
        ///<param name="_username">The username to associate with the player.</param>
        ///<param name="spawnPosition">The position to spawn the player at.</param>
        public Player(int _id, string _username, Vector3 _spawnPosition)
        {
            id = _id;
            username = _username;
            position = _spawnPosition;

            rotation = Quaternion.Identity;
            velocity = Vector3.Zero;
        }

        ///<summmary>
        ///     Update the players information. 
        ///</summary>
        public void Update()
        {
            Vector3 _forward = Vector3.Transform(new Vector3(0, 0, 1), rotation);
            Vector3 _right = Vector3.Normalize(Vector3.Cross(_forward, new Vector3(0, -1, 0)));
            Vector3 _acceleration = _right * inputs.X + _forward * inputs.Y; 

            if(velocity.Length() > Movement.HARD_MAX_VELOCITY) {
                velocity = Vector3.Normalize(velocity) * Movement.HARD_MAX_VELOCITY;
            }

            velocity = Movement.MoveGround(_acceleration, velocity) / Constants.TICKS_PER_SEC;

            position += velocity;

            Console.WriteLine(position);

            ServerSend.PlayerPosition(this);
        }

        ///<summary>
        ///     Set the players inputs and rotation from the client.
        ///</summary>
        ///<param name="_inputs">The inputs of the player.</param>
        ///<param name="_velocity"> The velocity of the player</param>
        ///<param name="_rotation">The quaternion representing the players rotation.</param>
        public void SetInput(Vector2 _inputs, Quaternion _rotation)
        {
            inputs = _inputs;
            rotation = _rotation;
        }
    }
}