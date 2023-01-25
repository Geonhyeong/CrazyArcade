﻿using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Game
{
    public struct Vector2Int
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y) { this.x = x; this.y = y; }

        public static Vector2Int up { get { return new Vector2Int(0, 1); } }
        public static Vector2Int down { get { return new Vector2Int(0, -1); } }
        public static Vector2Int left { get { return new Vector2Int(-1, 0); } }
        public static Vector2Int right { get { return new Vector2Int(1, 0); } }

        public static Vector2Int operator+(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x + b.x, a.y + b.y);
        }

        public static Vector2Int operator*(Vector2Int a, int b)
        {
            return new Vector2Int(a.x * b, a.y * b);
        }
    }

    public class Map
    {
        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int MinY { get; set; }
        public int MaxY { get; set; }

        private int _roomId;
        private bool[,] _collision;
        private GameObject[,] _objects; // 충돌체만 관리

        public Map(int roomId)
        {
            _roomId = roomId;
        }

        public bool CanGo(Vector2Int cellPos, bool checkObjects = true)
        {
            if (cellPos.x < MinX || cellPos.x > MaxX)
                return false;
            if (cellPos.y < MinY || cellPos.y > MaxY)
                return false;

            int x = cellPos.x - MinX;
            int y = MaxY - cellPos.y;
            return !_collision[y, x] && (!checkObjects || _objects[y, x] == null);
        }

        public GameObject Find(Vector2Int cellPos)
        {
            if (cellPos.x < MinX || cellPos.x > MaxX)
                return null;
            if (cellPos.y < MinY || cellPos.y > MaxY)
                return null;

            int x = cellPos.x - MinX;
            int y = MaxY - cellPos.y;
            return _objects[y, x];
        }

        public bool ApplyLeave(GameObject gameObject)
        {
            PositionInfo posInfo = gameObject.PosInfo;
            if (posInfo.PosX < MinX || posInfo.PosX > MaxX)
                return false;
            if (posInfo.PosY < MinY || posInfo.PosY > MaxY)
                return false;
            
            int x = posInfo.PosX - MinX;
            int y = MaxY - posInfo.PosY;
            if (_objects[y, x] == gameObject)
                _objects[y, x] = null;

            return true;
        }

        public bool ApplyMove(GameObject gameObject, Vector2Int dest)
        {
            ApplyLeave(gameObject);

            PositionInfo posInfo = gameObject.PosInfo;
            if (CanGo(dest, true) == false)
                return false;
            
            {
                int x = dest.x - MinX;
                int y = MaxY - dest.y;
                _objects[y, x] = gameObject;
            }

            // 실제 좌표 이동
            posInfo.PosX = dest.x;
            posInfo.PosY = dest.y;
            return true;
        }

        public void LoadMap(int mapId, string pathPrefix = "../../../../../Common/MapData")
        {
            string mapName = "Map_" + mapId.ToString("000");

            // 맵 관련 파일
            string text = File.ReadAllText($"{pathPrefix}/{mapName}.txt");
            StringReader reader = new StringReader(text);

            MinX = int.Parse(reader.ReadLine());
            MaxX = int.Parse(reader.ReadLine());
            MinY = int.Parse(reader.ReadLine());
            MaxY = int.Parse(reader.ReadLine());

            int xCount = MaxX - MinX + 1;
            int yCount = MaxY - MinY + 1;
            _collision = new bool[yCount, xCount];
            _objects = new GameObject[yCount, xCount];

            // collision 정보 읽기
            for (int y = 0; y < yCount; y++)
            {
                string line = reader.ReadLine();
                for (int x = 0; x < xCount; x++)
                {
                    _collision[y, x] = (line[x] == '1' ? true : false);
                }
            }

            for (int y = 0; y < yCount; y++)
            {
                string line = reader.ReadLine();
                for (int x = 0; x < xCount; x++)
                {
                    if (line[x] == '0')
                        continue;

                    Block block = ObjectManager.Instance.Add<Block>();
                    {
                        block.Info.Name = $"Block_{line[x] - '0'}";
                        block.PosInfo.State = CreatureState.Idle;
                        block.PosInfo.PosX = x + MinX;
                        block.PosInfo.PosY = MaxY - y;
                    }
                    RoomManager.Instance.Find(_roomId).EnterGame(block);
                }
            }
        }
    }
}
