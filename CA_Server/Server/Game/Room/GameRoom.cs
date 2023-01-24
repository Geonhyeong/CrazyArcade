using Google.Protobuf;
using Google.Protobuf.Protocol;
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
            }
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
                        player.Session.Send(spawnPacket);
                    }
                }
                else if (type == GameObjectType.Block)
                {
                    Block block = gameObject as Block;
                    _blocks.Add(gameObject.Id, block);
                    block.Room = this;
                }
                else if (type == GameObjectType.Bubble)
                {
                    Bubble bubble = gameObject as Bubble;
                    _bubbles.Add(gameObject.Id, bubble);
                    bubble.Room = this;
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
                }
                else if (type == GameObjectType.Bubble)
                {
                    Bubble bubble = null;
                    if (_bubbles.Remove(objectId, out bubble) == false)
                        return;

                    bubble.Room = null;
                    Map.ApplyLeave(bubble);
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
                        return;
                }

                // 서버에서 위치 이동
                info.PosInfo = movePosInfo;

                // 다른 플레이어한테도 알려준다
                S_Move resMovePacket = new S_Move();
                resMovePacket.ObjectId = player.Info.ObjectId;
                resMovePacket.PosInfo = movePacket.PosInfo;
                //Console.WriteLine($"S_Move ({resMovePacket.PosInfo.PosX}, {resMovePacket.PosInfo.PosY})");

                Broadcast(resMovePacket);
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
                if (skillPacket.Info.Power > 10 || skillPacket.Info.Power < 1)
                    return;

                // 버블 생성
                Bubble bubble = ObjectManager.Instance.Add<Bubble>();
                if (bubble == null)
                    return;

                bubble.Owner = player;
                bubble.Power = skillPacket.Info.Power;
                bubble.Info.Name = $"Bubble_{bubble.Id}";
                bubble.Info.PosInfo = skillPacket.Info.PosInfo;
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