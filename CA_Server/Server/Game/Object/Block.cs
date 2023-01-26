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
            t.Elapsed += OnPopEvent;
            t.AutoReset = false;
            t.Enabled = true;
        }

        private void OnPopEvent(object s, ElapsedEventArgs e)
        {
            DropItem();

            Room.LeaveGame(Id);
        }

        private void DropItem()
        {
            // item 생성 (일단 랜덤으로)
            Random rand = new Random();
            int randInt = rand.Next(0, 8); // 꽝 : 50%
            if (randInt % 2 != 0)  // 꽝
                return;

            Item item = ObjectManager.Instance.Add<Item>();
            if (item == null)
                return;

            item.ItemType = randInt / 2 + 1;
            item.Info.Name = $"Item_{item.ItemType}";
            item.PosInfo.State = CreatureState.Idle;
            item.PosInfo.PosX = PosInfo.PosX;
            item.PosInfo.PosY = PosInfo.PosY;
            Room.EnterGame(item);
        }
    }
}
