using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Bubble : GameObject
    {
        public Player Owner { get; set; }
        public int Power { get; set; }

        public Bubble()
        {
            ObjectType = GameObjectType.Bubble;
        }

        public void OnSpawn()
        {
            Console.WriteLine($"{Id} : Bubble Spawn ({CellPos.x}, {CellPos.y})");

            Owner.BubbleCount++;
            Room.PushAfter(2500, Pop);
        }

        private void Pop()
        {
            if (Room == null)
                return;
            if (Info.PosInfo.State == CreatureState.Pop)
                return;

            Owner.BubbleCount--;
            PosInfo.State = CreatureState.Pop;

            // Pop 패킷 Broadcast
            S_Pop popPacket = new S_Pop();
            popPacket.ObjectId = Id;

            Room.Broadcast(popPacket);

            // 플레이어 및 아이템이 존재하는지 확인
            List<GameObject> gameObjects = Room.FindAll(CellPos);
            if (gameObjects != null)
            {
                foreach (GameObject go in gameObjects)
                    go.OnAttacked(this);
            }

            // 웨이브 소환
            for (int waveDir = 0; waveDir < 4; waveDir++)
            {
                for (int i = 1; i <= Power; i++)
                {
                    Vector2Int wavePos = GetFrontCellPos((MoveDir)waveDir, i);
                    if (Room.Map.CanGo(wavePos, true) == false)
                    {
                        GameObject go = Room.Map.Find(wavePos);
                        if (go != null)
                        {
                            go.OnAttacked(this);
                        }
                        break;
                    }

                    Wave wave = ObjectManager.Instance.Add<Wave>();
                    if (wave == null)
                        break;

                    wave.Info.Name = $"Wave_{wave.Id}";
                    wave.Info.PosInfo.MoveDir = (MoveDir)waveDir;
                    wave.Info.PosInfo.PosX = wavePos.x;
                    wave.Info.PosInfo.PosY = wavePos.y;
                    wave.Info.IsEdge = (i == Power);

                    GameRoom room = Room;
                    room.Push(room.EnterGame, wave);
                }
            }

            // 0.5초 후에 소멸
            Room.PushAfter(500, Despawn);
        }

        private void Despawn()
        {
            Room.Push(Room.LeaveGame, Id);
            
            Console.WriteLine($"{Id} : Bubble Despawn");
        }

        public override void OnAttacked(GameObject attacker)
        {
            Console.WriteLine($"{Id} : Bubble Attacked");

            Pop();
        }
    }
}