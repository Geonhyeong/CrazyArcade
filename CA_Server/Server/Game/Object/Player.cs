using Google.Protobuf.Protocol;
using System.Timers;

namespace Server.Game
{
    public class Player : GameObject
    {
        private int _speedLvl = 1;
        private int _power = 1;
        private int _maxBubble = 1;

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
    }
}