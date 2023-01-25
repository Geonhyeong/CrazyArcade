﻿using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class GameObject
    {
        public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
        public int Id
        {
            get { return Info.ObjectId; }
            set { Info.ObjectId = value; }
        }
        public GameRoom Room { get; set; }
        public ObjectInfo Info { get; set; } = new ObjectInfo();
        public PositionInfo PosInfo { get; private set; } = new PositionInfo();

        public GameObject()
        {
            Info.PosInfo = PosInfo;
        }

        public Vector2Int CellPos
        {
            get { return new Vector2Int(PosInfo.PosX, PosInfo.PosY); }
            set
            {
                PosInfo.PosX = value.x;
                PosInfo.PosY = value.y;
            }
        }

        public Vector2Int GetFrontCellPos()
        {
            return GetFrontCellPos(PosInfo.MoveDir);
        }

        public Vector2Int GetFrontCellPos(MoveDir dir, int distance = 1)
        {
            Vector2Int cellPos = CellPos;

            switch (dir)
            {
                case MoveDir.Up:
                    cellPos += Vector2Int.up * distance;
                    break;
                case MoveDir.Down:
                    cellPos += Vector2Int.down * distance;
                    break;
                case MoveDir.Left:
                    cellPos += Vector2Int.left * distance;
                    break;
                case MoveDir.Right:
                    cellPos += Vector2Int.right * distance;
                    break;
            }

            return cellPos;
        }
    }
}