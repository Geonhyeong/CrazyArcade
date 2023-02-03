using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;

namespace Server.Game
{
    public class GameRoom : JobSerializer
    {
        public int RoomId { get; set; }
        public Map Map { get; private set; } = new Map();

        private Dictionary<int, Player> _players = new Dictionary<int, Player>();
        private Dictionary<int, Block> _blocks = new Dictionary<int, Block>();
        private Dictionary<int, Bubble> _bubbles = new Dictionary<int, Bubble>();
        private Dictionary<int, Wave> _waves = new Dictionary<int, Wave>();
        private Dictionary<int, Item> _items = new Dictionary<int, Item>();

        public void Init(int mapId)
        {
            Map.LoadMap(mapId);
            Map.LoadObjects(this);
        }

        // 누군가 주기적으로 호출해줘야 한다
        public void Update()
        {
            Flush();
        }

        public void EnterGame(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            if (type == GameObjectType.Player)
            {
                Player player = gameObject as Player;
                _players.Add(gameObject.Id, player);
                player.Room = this;
                Console.WriteLine($"{player.Id} : Player Spawn ({player.CellPos.x}, {player.CellPos.y})");

                // 본인한테 정보 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = player.Info;
                    player.Session.Send(enterPacket);

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if (player != p)
                            spawnPacket.Objects.Add(p.Info);
                    }

                    foreach (Block block in _blocks.Values)
                        spawnPacket.Objects.Add(block.Info);

                    foreach (Bubble bubble in _bubbles.Values)
                        spawnPacket.Objects.Add(bubble.Info);

                    foreach (Wave wave in _waves.Values)
                        spawnPacket.Objects.Add(wave.Info);

                    foreach (Item item in _items.Values)
                        spawnPacket.Objects.Add(item.Info);

                    player.Session.Send(spawnPacket);
                }
            }
            else if (type == GameObjectType.Block)
            {
                Block block = gameObject as Block;
                _blocks.Add(gameObject.Id, block);
                block.Room = this;
                Console.WriteLine($"{block.Id} : Block Spawn ({block.CellPos.x}, {block.CellPos.y})");
            }
            else if (type == GameObjectType.Bubble)
            {
                Bubble bubble = gameObject as Bubble;
                _bubbles.Add(gameObject.Id, bubble);
                bubble.Room = this;
                
                Map.ApplySpawn(bubble); // 충돌체이므로 Map에 적용
                bubble.OnSpawn();
            }
            else if (type == GameObjectType.Wave)
            {
                Wave wave = gameObject as Wave;
                _waves.Add(gameObject.Id, wave);
                wave.Room = this;

                wave.OnSpawn();
            }
            else if (type == GameObjectType.Item)
            {
                Item item = gameObject as Item;
                _items.Add(gameObject.Id, item);
                item.Room = this;

                item.OnSpawn();
            }

            // 타인한테 정보 전송
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Objects.Add(gameObject.Info);
                foreach (Player p in _players.Values)
                {
                    if (p.Id != gameObject.Id)
                        p.Session.Send(spawnPacket);
                }
            }
        }

        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            if (type == GameObjectType.Player)
            {
                Player player = null;
                if (_players.Remove(objectId, out player) == false)
                    return;

                player.Room = null;
                Console.WriteLine($"{player.Id} : Player Despawn");

                // 본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
            }
            else if (type == GameObjectType.Block)
            {
                Block block = null;
                if (_blocks.Remove(objectId, out block) == false)
                    return;

                Map.ApplyDespawn(block);
                block.Room = null;
                Console.WriteLine($"{block.Id} : Block Despawn");
            }
            else if (type == GameObjectType.Bubble)
            {
                Bubble bubble = null;
                if (_bubbles.Remove(objectId, out bubble) == false)
                    return;

                Map.ApplyDespawn(bubble);
                bubble.Room = null;
            }
            else if (type == GameObjectType.Wave)
            {
                Wave wave = null;
                if (_waves.Remove(objectId, out wave) == false)
                    return;

                wave.Room = null;
            }
            else if (type == GameObjectType.Item)
            {
                Item item = null;
                if (_items.Remove(objectId, out item) == false)
                    return;

                item.Room = null;
            }

            // 타인한테 정보 전송
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);
                foreach (Player p in _players.Values)
                {
                    if (p.Id != objectId)
                        p.Session.Send(despawnPacket);
                }
            }
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null || player.Room != this)
                return;

            // TODO : 검증
            ObjectInfo info = player.Info;
            PositionInfo movePosInfo = movePacket.PosInfo;

            // 다른 좌표로 이동할 경우, 갈 수 있는지 체크
            if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
            {
                if (Map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                {
                    return;
                }
            }

            // 서버에서 위치 이동
            player.PosInfo.State = movePosInfo.State;
            player.PosInfo.MoveDir = movePosInfo.MoveDir;
            player.PosInfo.PosX = movePosInfo.PosX;
            player.PosInfo.PosY = movePosInfo.PosY;

            // 다른 플레이어한테도 알려준다
            S_Move resMovePacket = new S_Move();
            resMovePacket.ObjectId = player.Info.ObjectId;
            resMovePacket.PosInfo = movePacket.PosInfo;
            Console.WriteLine($"S_Move ({resMovePacket.PosInfo.PosX}, {resMovePacket.PosInfo.PosY})");

            Broadcast(resMovePacket);
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null || player.Room != this)
                return;

            // 스킬 사용 가능 여부 체크
            Vector2Int skillPos = new Vector2Int(skillPacket.Info.PosX, skillPacket.Info.PosY);
            if (Map.CanGo(skillPos) == false)
                return;
            if (skillPacket.Info.Power > player.Stat.MaxPower || skillPacket.Info.Power < 1)
                return;
            if (player.BubbleCount >= player.Stat.AvailBubble)
                return;

            // 버블 생성
            Bubble bubble = ObjectManager.Instance.Add<Bubble>();
            if (bubble == null)
                return;

            bubble.Owner = player;
            bubble.Power = player.Stat.Power;
            bubble.Info.Name = $"Bubble_{bubble.Id}";
            bubble.PosInfo.State = CreatureState.Idle;
            bubble.CellPos = skillPos;

            Push(EnterGame, bubble);
        }

        public List<GameObject> FindAll(Vector2Int cellPos)
        {
            List<GameObject> list = new List<GameObject>();

            foreach (Player player in _players.Values)
            {
                if (cellPos.x == player.CellPos.x && cellPos.y == player.CellPos.y)
                    list.Add(player);
            }

            foreach (Item item in _items.Values)
            {
                if (cellPos.x == item.CellPos.x && cellPos.y == item.CellPos.y)
                    list.Add(item);
            }

            if (list.Count == 0)
                return null;

            return list;
        }

        public void Broadcast(IMessage packet)
        {
            foreach (Player p in _players.Values)
            {
                p.Session.Send(packet);
            }
        }
    }
}