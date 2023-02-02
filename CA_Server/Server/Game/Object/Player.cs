using Google.Protobuf.Protocol;
using System.Timers;

namespace Server.Game
{
    public class Player : GameObject
    {
        public ClientSession Session { get; set; }
        public Data.Character Data { get; set; }

        public int SpeedLvl { get; set; } = 1;
        public int Power { get; set; } = 1;
        public int AvailBubble { get; set; } = 1;
        public int BubbleCount { get; set; } = 0;

        public Player()
        {
            ObjectType = GameObjectType.Player;
        }

        public override void OnAttacked(GameObject attacker)
        {
            if (Info.PosInfo.State == CreatureState.Trap || Info.PosInfo.State == CreatureState.Dead)
                return;

            System.Console.WriteLine($"{Id} : OnAttacked");

            PosInfo.State = CreatureState.Trap;

            S_Move trapPacket = new S_Move();
            trapPacket.ObjectId = Id;
            trapPacket.PosInfo = PosInfo;

            Room.Broadcast(trapPacket);

            // 5초 후 죽음
            Timer t = new Timer();
            t.Interval = 5000;
            t.Elapsed += OnDeadEvent;
            t.AutoReset = false;
            t.Enabled = true;
        }

        private void OnDeadEvent(object s, ElapsedEventArgs e)
        {
            OnDead();
        }

        public void OnDead()
        {
            if (Room == null)
                return;

            if (Info.PosInfo.State != CreatureState.Trap)
                return;

            PosInfo.State = CreatureState.Dead;

            S_Move deadPacket = new S_Move();
            deadPacket.ObjectId = Id;
            deadPacket.PosInfo = PosInfo;

            Room.Broadcast(deadPacket);
        }

        public void GetItem(Item item)
        {
            if (item == null)
                return;

            System.Console.WriteLine($"Player_{Id} get Item_{item.Id}, {item.ItemType}");
            switch (item.ItemType)
            {
                case 1: // ballon
                    if (AvailBubble < Data.maxBubble)
                        AvailBubble++;
                    break;

                case 2: // potion
                    if (Power < Data.maxPower)
                        Power++;
                    break;

                case 3: // potion_make_power_max
                    Power = Data.maxPower;
                    break;

                case 4: // skate
                    if (SpeedLvl < Data.maxSpeedLvl)
                        SpeedLvl++;
                    break;
            }

            S_Ability abilityPacket = new S_Ability();
            abilityPacket.PlayerId = Id;
            abilityPacket.Speed = SpeedLvl * 0.8f;
            abilityPacket.Power = Power;
            abilityPacket.AvailBubble = AvailBubble;

            Room.Broadcast(abilityPacket);
        }
    }
}