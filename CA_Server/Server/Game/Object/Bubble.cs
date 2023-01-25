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

            // 소멸타임틱에 소멸
            if (_popTimeTick >= Environment.TickCount64)
                return;

            Room.LeaveGame(Id);
        }

        private void RegisterPop()
        {
            if (_isPop)
                return;

            PosInfo.State = CreatureState.Pop;

            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;
            Room.Broadcast(movePacket);

            Console.WriteLine("Bubble Pop");

            // 웨이브 소환
            for (int waveDir = 0; waveDir < 4; waveDir++)
            {
                for (int i = 1; i <= Power; i++)
                {
                    Vector2Int wavePos = GetFrontCellPos((MoveDir)waveDir, i);
                    if (Room.Map.CanGo(wavePos, true) == false)
                        break;

                    Wave wave = ObjectManager.Instance.Add<Wave>();
                    if (wave == null)
                        break;

                    wave.Info.Name = $"Wave_{wave.Id}";
                    wave.Info.PosInfo.MoveDir = (MoveDir)waveDir;
                    wave.Info.PosInfo.PosX = wavePos.x;
                    wave.Info.PosInfo.PosY = wavePos.y;
                    wave.Info.IsEdge = (i == Power);
                    Room.EnterGame(wave);
                }
            }

            _isPop = true;
            _popTimeTick = Environment.TickCount64 + 500;
        }
    }
}
