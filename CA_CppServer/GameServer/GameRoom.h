#pragma once
#include "JobQueue.h"
#include "GameObject.h"

class GameRoom : public JobQueue
{
public:
	~GameRoom()
	{
		cout << "~GameRoom" << endl;
	}
	void Init(RoomRef ownerRoom);
	void EnterGame(GameObjectRef gameObject);
	void LeaveGame(int32 objectId);
	void Broadcast(SendBufferRef sendBuffer);

public:
	int32 GetGameRoomId() { return _gameRoomId; }
	void SetGameRoomId(int32 gameRoomId) { _gameRoomId = gameRoomId; }

private:
	int32 _gameRoomId;
	RoomRef _ownerRoom;
	map<int32, PlayerRef> _players;
};