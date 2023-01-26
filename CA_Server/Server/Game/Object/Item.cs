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
            Room.LeaveGame(Id);
        }
    }
}
