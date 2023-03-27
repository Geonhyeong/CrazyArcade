#pragma once
#include "JobQueue.h"
#include "GameObject.h"

class GameRoom : public JobQueue
{
public:
	~GameRoom();

	void Init(RoomRef ownerRoom);
	void EnterGame(GameObjectRef gameObject);
	void LeaveGame(int32 objectId);
	void HandleMove(PlayerRef player, Protocol::C_Move movePacket);
	void HandleSkill(PlayerRef player, Protocol::C_Skill skillPacket);
	void CheckAndEnd();
	void Broadcast(SendBufferRef sendBuffer);

	vector<GameObjectRef> FindAllObjects(Vector2Int cellPos);
	int32 GetGameRoomId() { return _gameRoomId; }
	void SetGameRoomId(int32 gameRoomId) { _gameRoomId = gameRoomId; }

public:
	Map gameMap;

private:
	int32 _gameRoomId;
	RoomRef _ownerRoom;
	map<int32, PlayerRef> _players;
	map<int32, BlockRef> _blocks;
	map<int32, BubbleRef> _bubbles;
	map<int32, WaveRef> _waves;
	map<int32, ItemRef> _items;
};

// TEMP
const vector<Vector2Int> START_POINTS = {
	Vector2Int(-10, 4),
	Vector2Int(-1, -6),
	Vector2Int(0, 2),
	Vector2Int(-10, -4),
	Vector2Int(0, -2),
	Vector2Int(-10, 0),
	Vector2Int(-4, 4),
	Vector2Int(-10, -6)
};