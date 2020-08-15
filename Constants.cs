using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{   
    ///<summary>
    ///     Constants used various places throughout the server.
    ///</summary>
    public class Constants
    {
        ///<summary name="TICKS_PER_SEC">
        ///     The tickrate of the server.
        ///</summary>
        public const int TICKS_PER_SEC = 30;

        ///<summary name="MS_PER_TICK">
        ///     The milliseconds per tick.
        ///</summary>
        public const int MS_PER_TICK = 1000 / TICKS_PER_SEC;
    }
}