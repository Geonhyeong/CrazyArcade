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

        public void OnSpawn()
        {
            Console.WriteLine($"{Id} : Wave Spawn ({CellPos.x}, {CellPos.y})");

            Room.Push(CheckCollision);
            Room.PushAfter(500, Despawn);
        }

        private void CheckCollision()
        {
            if (Room == null)
                return;

            // 플레이어 및 아이템이 존재하는지 확인
            List<GameObject> gameObjects = Room.FindAll(CellPos);
            if (gameObjects != null)
            {
                foreach (GameObject go in gameObjects)
                    go.OnAttacked(this);
            }

            // 0.1초마다 체크
            Room.PushAfter(100, CheckCollision);
        }

        private void Despawn()
        {
            Room.Push(Room.LeaveGame, Id);

            Console.WriteLine($"{Id} : Wave Despawn");
        }
    }
}
