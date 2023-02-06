using Google.Protobuf.Protocol;
using System.Collections.Generic;
using System.Timers;

namespace Server.Game
{
    public class Player : GameObject
    {
        public ClientSession Session { get; set; }
        public int BubbleCount { get; set; } = 0;

        public Player()
        {
            ObjectType = GameObjectType.Player;
        }

        public override void OnAttacked(GameObject attacker)
        {
            System.Console.WriteLine($"{Id} : Player is Attacked");

            if (Info.PosInfo.State == CreatureState.Trap || Info.PosInfo.State == CreatureState.Dead)
                return;

            PosInfo.State = CreatureState.Trap;

            // Trap 패킷 Broacast
            S_Trap trapPacket = new S_Trap();
            trapPacket.PlayerId = Id;

            Room.Broadcast(trapPacket);

            // 다른 플레이어가 접근했는지 체크
            Room.Push(CheckCollision);

            // 5초 후 죽음
            Room.PushAfter(5000, OnDead);
        }

        private void CheckCollision()
        {
            if (Room == null)
                return;

            if (Info.PosInfo.State != CreatureState.Trap)
                return;

            List<GameObject> gameObjects = Room.FindAll(CellPos);
            if (gameObjects != null)
            {
                foreach (GameObject go in gameObjects)
                {
                    if (go.ObjectType == GameObjectType.Player)
                    {
                        Player p = go as Player;
                        if (p.Id != Id)
                        {
                            CreatureState state = p.Info.PosInfo.State;
                            if (state == CreatureState.Idle || state == CreatureState.Moving)
                            {
                                OnDead();
                                return;
                            }
                        }    
                    }
                }
            }

            // 0.1초마다 체크
            Room.PushAfter(100, CheckCollision);
        }

        private void OnDead()
        {
            if (Room == null)
                return;

            if (Info.PosInfo.State != CreatureState.Trap)
                return;

            PosInfo.State = CreatureState.Dead;

            // Die 패킷 Broadcast
            S_Die diePacket = new S_Die();
            diePacket.PlayerId = Id;

            Room.Broadcast(diePacket);
        }

        public void GetItem(Item item)
        {
            if (item == null)
                return;

            System.Console.WriteLine($"Player_{Id} get Item_{item.Id}, {item.ItemType}");
            switch (item.ItemType)
            {
                case 1: // ballon
                    if (Stat.AvailBubble < Stat.MaxBubble)
                        Stat.AvailBubble++;
                    break;

                case 2: // potion
                    if (Stat.Power < Stat.MaxPower)
                        Stat.Power++;
                    break;

                case 3: // potion_make_power_max
                    Stat.Power = Stat.MaxPower;
                    break;

                case 4: // skate
                    if (Stat.SpeedLvl < Stat.MaxSpeedLvl)
                        Stat.SpeedLvl++;
                    break;
            }

            S_ChangeStat statPacket = new S_ChangeStat() { StatInfo = new StatInfo() };
            statPacket.PlayerId = Id;
            statPacket.StatInfo = Stat;

               Room.Broadcast(statPacket);
        }
    }
}