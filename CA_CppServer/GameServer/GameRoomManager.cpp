#include "pch.h"
#include "GameRoomManager.h"
#include "GameRoom.h"

GameRoomManager GGameRoomManager;

GameRoomRef GameRoomManager::Add()
{
	WRITE_LOCK;
	int32 gameRoomId = ++_gameRoomId;

	GameRoomRef gameRoom = make_shared<GameRoom>();
	gameRoom->SetGameRoomId(gameRoomId);

	_gameRooms.insert(make_pair(gameRoomId, gameRoom));

	cout << "Game Room is Generated : " << gameRoomId << endl;

	return gameRoom;
}

void GameRoomManager::Remove(int32 gameRoomId)
{
	WRITE_LOCK;
	if (Find(gameRoomId) == nullptr)
		return;

	_gameRooms.erase(gameRoomId);

	cout << "Game Room is Removed : " << gameRoomId << endl;
}

GameRoomRef GameRoomManager::Find(int32 gameRoomId)
{
	READ_LOCK;
	auto it = _gameRooms.find(gameRoomId);
	if (it != _gameRooms.end())
		return it->second;

	return nullptr;
}
