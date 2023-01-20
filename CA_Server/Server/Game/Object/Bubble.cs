using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Bubble : GameObject
    {
        public GameObject Owner { get; set; }

        private long _lifeTimeTick = Environment.TickCount64 + 50;
        //private long _popTimeTick = Environment.TickCount64 + 100; 

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

            // TODO : 버블팝
            PosInfo.State = CreatureState.Dead;

            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;
            Room.Broadcast(movePacket);
            
            Console.WriteLine("Bubble Pop");

            // TODO : 웨이브 소환
            
            // TODO : 소멸타임틱에 소멸
            Room.LeaveGame(Id);
        }
    }
}
