#include "pch.h"
#include "RoomManager.h"
#include "Room.h"

RoomManager GRoomManager;

RoomRef RoomManager::Generate(int mapId)
{
	WRITE_LOCK;
	int32 roomId = ++_roomId;

	RoomRef room = make_shared<Room>();
	room->SetRoomId(roomId);
	_rooms.insert(make_pair(roomId, room));

	cout << "Room is Generated : " << roomId << endl;
}

void RoomManager::Remove(int roomId)
{
	WRITE_LOCK;
	if (Find(roomId) == nullptr)
		return;

	_rooms.erase(roomId);

	cout << "Room is Removed : " << roomId << endl;
}

RoomRef RoomManager::Find(int roomId)
{
	READ_LOCK;
	auto it = _rooms.find(roomId);
	if (it != _rooms.end())
		return it->second;

	return nullptr;
}
