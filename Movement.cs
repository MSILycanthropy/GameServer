using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace GameServer
{
    public class Movement
    {   
        ///Make these be got from the server
        private float AIR_ACCELERATE = 12f;

        private float ACCELERATE = 24f;

        private float MAX_GROUND_VELOCITY = 6f;

        private float MAX_AIR_VELOCITY = 6f;

        private float FRICTION = 4f;

        private float HARD_MAX_VELOCITY = 10f;


        ///<summary>
        ///        Accelerate the player towards a direction 
        ///</summary>
        ///<param name="_accelDir">The direction of acceleration</param>
        ///<param name="_prevVelocity">The previous ticks velocity</param>
        ///<param name="_accelerate">The multiplicative acceleration constant</param>
        ///<param name="_maxVelocity">The maximum velocity. This is applied via the veer projection onto the acceeleration.</param>
        public Vector3 Accelerate(Vector3 _accelDir, Vector3 _prevVelocity, float _accelerate, float _maxVelocity)
        {
            //Get the veer, or the projection of the velocity onto the acceleration
            float veer = Vector3.Dot(_prevVelocity, _accelDir) / _accelDir.Length();

            //Get the velocity that we should be accelerating by based on accelerate aka the max acceleration value
            float velocityToAccelerateBy = _accelerate / Constants.TICKS_PER_SEC;

            //Check that the velocity isnt too fast
            if (veer + velocityToAccelerateBy > _maxVelocity)
            {
                velocityToAccelerateBy = _maxVelocity - veer;
            }

            //Return the newly calculated velocity
            return (_prevVelocity + _accelDir * velocityToAccelerateBy);
        }

        ///<summary>
        ///     Move the player on the ground
        ///</summary>
        ///<param name="_accelDir">The direction of acceleration</param>
        ///<param name="_prevVelocity">The previous tick's velocity</param>
        ///<param name="_accelerate">The multiplicative acceleration constant</param>
        ///<param name="_maxVelocity">The maximum velocity. This is applied via the veer projection onto the acceeleration.</param>
        public Vector3 MoveGround(Vector3 _accelDir, Vector3 _prevVelocity)
        {
            //Get the speed aka the magnitude of velocity
            float speed = _prevVelocity.Length();

            //Avoid divide by zero errors cuz why not
            if (speed != 0)
            {
                //Calculate the amount to lower the speed by
                float lowerSpeedBy = (speed * FRICTION) / Constants.TICKS_PER_SEC;

                //Scale the velocity by the lowered amount or 0 if its negative
                _prevVelocity *= MathF.Max(speed - lowerSpeedBy, 0) / speed;
            }

            //Return the modified aceleration to towards that vector
            return (Accelerate(_accelDir, _prevVelocity, ACCELERATE, MAX_GROUND_VELOCITY));
        }

        ///<summary>
        ///     Move the player in the air
        ///</summary>
        ///<param name="_accelDir">The direction of acceleration</param>
        ///<param name="_prevVelocity">The previous tick's velocity</param>
        public Vector3 MoveAir(Vector3 _accelDir, Vector3 _prevVelocity)
        {
            return (Accelerate(_accelDir, _prevVelocity, AIR_ACCELERATE, MAX_AIR_VELOCITY));
        }
    }
}