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
            // TODO : item 생성 (일단 랜덤으로)
            Random rand = new Random();
            int randInt = rand.Next(0, 100);
            int itemType = 0;
            if (randInt < 40)
                return;
            else if (randInt < 60)
                itemType = 1;
            else if (randInt < 75)
                itemType = 2;
            else if (randInt < 80)
                itemType = 3;
            else
                itemType = 4;

            Item item = ObjectManager.Instance.Add<Item>();
            if (item == null)
                return;

            item.ItemType = itemType;
            item.Info.Name = $"Item_{item.ItemType}";
            item.PosInfo.State = CreatureState.Idle;
            item.PosInfo.PosX = PosInfo.PosX;
            item.PosInfo.PosY = PosInfo.PosY;
            Room.EnterGame(item);
        }
    }
}
