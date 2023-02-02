using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Bubble : GameObject
    {
        public GameObject Owner { get; set; }

        public int Power { get; set; }

        private bool _isPop = false;
        private long _lifeTimeTick = Environment.TickCount64 + 2500;
        private long _popTimeTick = Environment.TickCount64 + 3000;

        public Bubble()
        {
            ObjectType = GameObjectType.Bubble;
        }

        public void Update()
        {
            if (Owner == null || Room == null)
                return;

            if (_lifeTimeTick >= Environment.TickCount64)
                return;

            RegisterPop();

            // 플레이어 및 아이템 피격 판정
            List<GameObject> gameObjects = Room.FindAll(CellPos);
            if (gameObjects != null)
            {
                foreach (GameObject go in gameObjects)
                    go.OnAttacked(this);
            }

            // 소멸타임틱에 소멸
            if (_popTimeTick >= Environment.TickCount64)
                return;

            GameRoom room = Room;
            room.Push(room.LeaveGame, Id);
        }

        private void RegisterPop()
        {
            if (_isPop)
                return;

            _isPop = true;
            _lifeTimeTick = Environment.TickCount64;
            _popTimeTick = _lifeTimeTick + 500;

            PosInfo.State = CreatureState.Pop;

            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;
            Room.Broadcast(movePacket);

            // 웨이브 소환
            for (int waveDir = 0; waveDir < 4; waveDir++)
            {
                for (int i = 1; i <= Power; i++)
                {
                    Vector2Int wavePos = GetFrontCellPos((MoveDir)waveDir, i);
                    if (Room.Map.CanGo(wavePos, true) == false)
                    {
                        // Block 피격 판정
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
        }

        public override void OnAttacked(GameObject attacker)
        {
            Console.WriteLine($"{Id} : Bubble Attacked");

            RegisterPop();
        }
    }
}
