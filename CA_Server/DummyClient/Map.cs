using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace DummyClient
{
    public class Map : JobSerializer
    {
        public static Map Instance { get; } = new Map();

        public int minX { get; } = -40;
        public int maxX { get; } = 38;
        public int minY { get; } = -30;
        public int maxY { get; } = 28;
        
        Random rand = new Random();

        public PositionInfo GetRandomPos(int curX, int curY)
        {
            int num = rand.Next(0, 4);
            PositionInfo posInfo = new PositionInfo();
            posInfo.MoveDir = (MoveDir)num;
            posInfo.State = CreatureState.Moving;

            switch (posInfo.MoveDir)
            {
                case MoveDir.Up:
                    posInfo.PosX = curX;
                    posInfo.PosY = curY + 1;
                    break;
                case MoveDir.Down:
                    posInfo.PosX = curX;
                    posInfo.PosY = curY - 1;
                    break;
                case MoveDir.Left:
                    posInfo.PosX = curX - 1;
                    posInfo.PosY = curY;
                    break;
                case MoveDir.Right:
                    posInfo.PosX = curX + 1;
                    posInfo.PosY = curY;
                    break;
            }

            return posInfo;
        }

        public void Update()
        {
            Flush();
        }

        public void Move(ServerSession serverSession)
        {
            if (serverSession.DummyId == 1)
            {   // TEMP

            }

            C_Move movePacket = new C_Move();
            movePacket.PosInfo = GetRandomPos(serverSession.PosX, serverSession.PosY);
            if (movePacket.PosInfo.PosX < minX || movePacket.PosInfo.PosX > maxX || movePacket.PosInfo.PosY < minY || movePacket.PosInfo.PosY > maxY)
            {

            }
            else
            {
                serverSession.Send(movePacket);

                serverSession.Dir = movePacket.PosInfo.MoveDir;
                serverSession.PosX = movePacket.PosInfo.PosX;
                serverSession.PosY = movePacket.PosInfo.PosY;
            }

            int tickAfter = rand.Next(500, 5000);
            PushAfter(tickAfter, Move, serverSession);
        }
    }
}
