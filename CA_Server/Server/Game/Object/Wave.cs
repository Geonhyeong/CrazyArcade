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

            // 플레이어 및 아이템 피격 판정
            List<GameObject> gameObjects = Room.FindAll(CellPos);
            if (gameObjects != null)
            {
                foreach (GameObject go in gameObjects)
                    go.OnAttacked(this);
            }

            if (_lifeTimeTick >= Environment.TickCount64)
                return;

            GameRoom room = Room;
            room.Push(room.LeaveGame, Id);
        }
    }
}
