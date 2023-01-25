using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Server.Game
{
    public class Block : GameObject
    {
        public Block()
        {
            ObjectType = GameObjectType.Block;
        }

        public override void OnAttacked(GameObject attacker)
        {
            // Pop 상태로 변환
            S_Move changeStatePacket = new S_Move() { PosInfo = new PositionInfo() };
            changeStatePacket.ObjectId = Id;
            changeStatePacket.PosInfo.State = CreatureState.Pop;
            changeStatePacket.PosInfo.PosX = PosInfo.PosX;
            changeStatePacket.PosInfo.PosY = PosInfo.PosY;

            Room.Broadcast(changeStatePacket);

            // 0.5초 후 소멸
            Timer t = new Timer();
            t.Interval = 500;
            t.Elapsed += (s, e) => Room.LeaveGame(Id);
            t.AutoReset = false;
            t.Enabled = true;
        }
    }
}
