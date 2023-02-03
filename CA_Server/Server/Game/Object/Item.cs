using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Item : GameObject
    {
        public int ItemType { get; set; }

        public Item()
        {
            ObjectType = GameObjectType.Item;
        }

        public override void OnAttacked(GameObject attacker)
        {
            Despawn();
        }

        public void OnSpawn()
        {
            Console.WriteLine($"{Id} : Item Spawn ({CellPos.x}, {CellPos.y})");

            Room.Push(CheckCollision);
        }

        private void CheckCollision()
        {
            if (Room == null)
                return;

            // 플레이어가 존재하는지 확인
            List<GameObject> gameObjects = Room.FindAll(CellPos);
            if (gameObjects != null)
            {
                foreach (GameObject go in gameObjects)
                {
                    if (go.ObjectType == GameObjectType.Player)
                    {
                        Player p = go as Player;
                        CreatureState state = p.Info.PosInfo.State;
                        if (state == CreatureState.Idle || state == CreatureState.Moving)
                        {
                            p.GetItem(this);
                            Despawn();
                            return;
                        }

                    }
                }
            }

            // 0.1초마다 체크
            Room.PushAfter(100, CheckCollision);
        }

        private void Despawn()
        {
            Room.Push(Room.LeaveGame, Id);

            Console.WriteLine($"{Id} : Item Despawn");
        }
    }
}
