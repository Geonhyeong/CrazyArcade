using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;

namespace Server.Game
{
    public class GameRoom
    {
        private object _lock = new object();
        public int RoomId { get; set; }
        public Map Map { get; private set; } = null;

        private Dictionary<int, Player> _players = new Dictionary<int, Player>();
        private Dictionary<int, Block> _blocks = new Dictionary<int, Block>();
        private Dictionary<int, Bubble> _bubbles = new Dictionary<int, Bubble>();
        private Dictionary<int, Wave> _waves = new Dictionary<int, Wave>();
        private Dictionary<int, Item> _items = new Dictionary<int, Item>();

        public void Init(int mapId)
        {
            Map = new Map(RoomId);
            Map.LoadMap(mapId);
        }

        public void Update()
        {
            lock (_lock)
            {
                foreach (Bubble bubble in _bubbles.Values)
                {
                    bubble.Update();
                }

                foreach (Wave wave in _waves.Values)
                {
                    wave.Update();
                }
            }
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

        public void EnterGame(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            lock (_lock)
            {
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
                        {
                            spawnPacket.Objects.Add(block.Info);
                        }
                        foreach (Bubble bubble in _bubbles.Values)
                        {
                            spawnPacket.Objects.Add(bubble.Info);
                        }
                        foreach (Wave wave in _waves.Values)
                        {
                            spawnPacket.Objects.Add(wave.Info);
                        }
                        foreach (Item item in _items.Values)
                        {
                            spawnPacket.Objects.Add(item.Info);
                        }
                        player.Session.Send(spawnPacket);
                    }
                }
                else if (type == GameObjectType.Block)
                {
                    Block block = gameObject as Block;
                    _blocks.Add(gameObject.Id, block);
                    block.Room = this;
                    Map.ApplyMove(block, block.CellPos);
                    Console.WriteLine($"{block.Id} : Block Spawn ({block.CellPos.x}, {block.CellPos.y})");
                }
                else if (type == GameObjectType.Bubble)
                {
                    Bubble bubble = gameObject as Bubble;
                    _bubbles.Add(gameObject.Id, bubble);
                    bubble.Room = this;
                    Map.ApplyMove(bubble, bubble.CellPos);
                    Console.WriteLine($"{bubble.Id} : Bubble Spawn ({bubble.CellPos.x}, {bubble.CellPos.y})");

                    // 플레이어의 현재 물풍선 갯수 증가
                    Player owner = bubble.Owner as Player;
                    owner.BubbleCount++;
                }
                else if (type == GameObjectType.Wave)
                {
                    Wave wave = gameObject as Wave;
                    _waves.Add(gameObject.Id, wave);
                    wave.Room = this;
                    Console.WriteLine($"{wave.Id} : Wave Spawn ({wave.CellPos.x}, {wave.CellPos.y})");
                }
                else if (type == GameObjectType.Item)
                {
                    Item item = gameObject as Item;
                    _items.Add(gameObject.Id, item);
                    item.Room = this;
                    Console.WriteLine($"{item.Id} : Item Spawn ({item.CellPos.x}, {item.CellPos.y})");
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
        }

        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            lock (_lock)
            {
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

                    block.Room = null;
                    Map.ApplyLeave(block);
                    Console.WriteLine($"{block.Id} : Block Despawn");
                }
                else if (type == GameObjectType.Bubble)
                {
                    Bubble bubble = null;
                    if (_bubbles.Remove(objectId, out bubble) == false)
                        return;

                    bubble.Room = null;
                    Map.ApplyLeave(bubble);
                    Console.WriteLine($"{bubble.Id} : Bubble Despawn");

                    // 플레이어의 현재 물풍선 갯수 감소
                    Player owner = bubble.Owner as Player;
                    owner.BubbleCount--;
                }
                else if (type == GameObjectType.Wave)
                {
                    Wave wave = null;
                    if (_waves.Remove(objectId, out wave) == false)
                        return;

                    wave.Room = null;
                    Console.WriteLine($"{wave.Id} : Wave Despawn");
                }
                else if (type == GameObjectType.Item)
                {
                    Item item = null;
                    if (_items.Remove(objectId, out item) == false)
                        return;

                    item.Room = null;
                    Console.WriteLine($"{item.Id} : Item Despawn");
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
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            lock (_lock)
            {
                // TODO : 검증
                ObjectInfo info = player.Info;
                PositionInfo movePosInfo = movePacket.PosInfo;

                // 다른 좌표로 이동할 경우, 갈 수 있는지 체크
                if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
                {
                    if (Map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                    {
                        // 다른 플레이어한테도 알려준다
                        S_Move noMovePacket = new S_Move() { PosInfo = new PositionInfo() };
                        noMovePacket.ObjectId = player.Info.ObjectId;
                        noMovePacket.PosInfo.State = movePacket.PosInfo.State;
                        noMovePacket.PosInfo.MoveDir = movePacket.PosInfo.MoveDir;
                        noMovePacket.PosInfo.PosX = info.PosInfo.PosX;
                        noMovePacket.PosInfo.PosY = info.PosInfo.PosY;
                        
                        Broadcast(noMovePacket);

                        return;
                    }
                }

                // 서버에서 위치 이동
                player.PosInfo.MoveDir = movePosInfo.MoveDir;
                player.PosInfo.State = movePosInfo.State;
                player.PosInfo.PosX = movePosInfo.PosX;
                player.PosInfo.PosY = movePosInfo.PosY;

                // 다른 플레이어한테도 알려준다
                S_Move resMovePacket = new S_Move();
                resMovePacket.ObjectId = player.Info.ObjectId;
                resMovePacket.PosInfo = movePacket.PosInfo;
                //Console.WriteLine($"S_Move ({resMovePacket.PosInfo.PosX}, {resMovePacket.PosInfo.PosY})");

                Broadcast(resMovePacket);

                if (player.Info.PosInfo.State == CreatureState.Idle || player.Info.PosInfo.State == CreatureState.Moving)
                {
                    List<GameObject> gameObjects = FindAll(player.CellPos);
                    if (gameObjects != null)
                    {
                        foreach (GameObject go in gameObjects)
                        {
                            if (go.ObjectType == GameObjectType.Player)
                            {
                                // 만약 그 위치에 Trap 상태의 다른 플레이어가 있으면 즉사시킨다
                                Player p = go as Player;
                                if (p.Id != player.Id && p.Info.PosInfo.State == CreatureState.Trap)
                                    p.OnDead();
                            }
                            else if (go.ObjectType == GameObjectType.Item)
                            {
                                // 아이템이 있으면 획득하고 아이템 삭제
                                Item item = go as Item;
                                player.GetItem(item);
                                LeaveGame(item.Id);
                            }
                        }
                    }
                }
            }
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;

            lock (_lock)
            {
                ObjectInfo info = player.Info;

                // 스킬 사용 가능 여부 체크
                Vector2Int skillPos = new Vector2Int(skillPacket.Info.PosInfo.PosX, skillPacket.Info.PosInfo.PosY);
                if (Map.CanGo(skillPos) == false)
                    return;
                if (skillPacket.Info.Power > 5 || skillPacket.Info.Power < 1)
                    return;
                if (player.BubbleCount >= player.MaxBubble)
                    return;

                // 버블 생성
                Bubble bubble = ObjectManager.Instance.Add<Bubble>();
                if (bubble == null)
                    return;

                bubble.Owner = player;
                bubble.Power = player.Power;
                bubble.Info.Name = $"Bubble_{bubble.Id}";
                bubble.PosInfo.State = skillPacket.Info.PosInfo.State;
                bubble.PosInfo.PosX = skillPacket.Info.PosInfo.PosX;
                bubble.PosInfo.PosY = skillPacket.Info.PosInfo.PosY;
                EnterGame(bubble);
            }
        }

        public void Broadcast(IMessage packet)
        {
            lock (_lock)
            {
                foreach (Player p in _players.Values)
                {
                    p.Session.Send(packet);
                }
            }
        }
    }
}