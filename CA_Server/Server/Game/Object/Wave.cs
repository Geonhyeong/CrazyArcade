using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Wave : GameObject
    {
        private long _lifeTimeTick = Environment.TickCount64 + 500;

        public Wave()
        {
            ObjectType = GameObjectType.Wave;
        }

        public void Update()
        {
            if (Room == null)
                return;

            if (_lifeTimeTick >= Environment.TickCount64)
                return;

            Room.LeaveGame(Id);
        }
    }
}
