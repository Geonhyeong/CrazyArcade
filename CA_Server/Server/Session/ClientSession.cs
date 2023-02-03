using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game;
using ServerCore;
using System;
using System.Net;

namespace Server
{
    public class ClientSession : PacketSession
    {
        public Player MyPlayer { get; set; }
        public int SessionId { get; set; }

        public void Send(IMessage packet)
        {
            string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
            MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);

            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

            Send(new ArraySegment<byte>(sendBuffer));
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            MyPlayer = ObjectManager.Instance.Add<Player>();
            {
                MyPlayer.Info.Name = $"Player_{MyPlayer.Id}";
                MyPlayer.PosInfo.State = CreatureState.Idle;
                MyPlayer.PosInfo.MoveDir = MoveDir.Down;
                // TODO : 위치는 정해진 몇 곳에서 랜덤 배정 (위치는 미리 데이터로 저장해 놓자)
                MyPlayer.PosInfo.PosX = 0;
                MyPlayer.PosInfo.PosY = -1;

                // 지금은 배찌밖에 없기 때문에 무조건 배찌로 지정
                StatInfo stat = null;
                DataManager.StatDict.TryGetValue(1, out stat);
                MyPlayer.Stat.MergeFrom(stat);
                
                MyPlayer.Session = this;
            }

            GameRoom room = RoomManager.Instance.Find(1);
            room.Push(room.EnterGame, MyPlayer);
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            GameRoom room = RoomManager.Instance.Find(1);
            room.Push(room.LeaveGame, MyPlayer.Info.ObjectId);
            ObjectManager.Instance.Remove(MyPlayer.Info.ObjectId);

            SessionManager.Instance.Remove(this);

            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            //Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }
}