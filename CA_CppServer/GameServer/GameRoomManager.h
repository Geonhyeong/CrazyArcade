#pragma once

class GameRoomManager
{
public:
	GameRoomRef Add();
	void Remove(int32 gameRoomId);
	GameRoomRef Find(int32 gameRoomId);

private:
	USE_LOCK;
	int32 _gameRoomId = 0;
	map<int32, GameRoomRef> _gameRooms;
};

extern GameRoomManager GGameRoomManager;

