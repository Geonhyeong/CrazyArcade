using Google.Protobuf.Protocol;
using System.Timers;

namespace Server.Game
{
    public class Player : GameObject
    {
        public int SpeedLvl { get; set; } = 1;
        public int Power { get; set; } = 1;
        public int MaxBubble { get; set; } = 1;

        public ClientSession Session { get; set; }

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
            switch(item.ItemType)
            {
                case 1: // ballon
                    if (MaxBubble < 5 && MaxBubble > 0)
                        MaxBubble++;
                    break;
                case 2: // potion
                    if (Power < 10 && Power > 0)
                        Power++;
                    break;
                case 3: // potion_make_power_max
                    Power = 10;
                    break;
                case 4: // skate
                    if (SpeedLvl < 5 && SpeedLvl > 0)
                        SpeedLvl++;
                    break;
            }

            // TODO : 능력 변화 패킷 전송
        }
    }
}