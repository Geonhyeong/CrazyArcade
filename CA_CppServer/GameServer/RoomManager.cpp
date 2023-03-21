#include "pch.h"
#include "RoomManager.h"
#include "Room.h"
#include <random>

RoomManager GRoomManager;

RoomRef RoomManager::Generate()
{
	WRITE_LOCK;
	int32 roomId = ++_roomId;

	RoomRef room = make_shared<Room>();
	room->SetRoomId(roomId);
	room->SetRoomCode(MakeRoomCode());
	_rooms.insert(make_pair(roomId, room));

	cout << "Room is Generated : " << roomId << endl;

	return room;
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

RoomRef RoomManager::FindByCode(string roomCode)
{
	READ_LOCK;
	for (auto& it : _rooms)
	{
		RoomRef room = it.second;
		if (roomCode == room->GetRoomCode())
			return room;
	}

	return nullptr;
}

RoomRef RoomManager::GetRandomRoom()
{
	READ_LOCK;
	for (auto& it : _rooms)
	{
		RoomRef room = it.second;
		if (room->CanEnter())
			return room;
	}

	return nullptr;
}

string RoomManager::MakeRoomCode()
{
	char codeKey[36] = { '0', '1', '2', '3', '4', '5',
						'6', '7', '8', '9', 'A', 'B',
						'C', 'D', 'E', 'F', 'G', 'H',
						'I', 'J', 'K', 'L', 'M', 'N',
						'O', 'P', 'Q', 'R', 'S', 'T',
						'U', 'V', 'W', 'X', 'Y', 'Z' };

	string roomCode;
	::random_device rd;
	::mt19937 gen(rd());
	::uniform_int_distribution<int> dis(0, 35);

	while (true)
	{
		roomCode = "";
		for (int i = 0; i < 4; i++)
			roomCode += codeKey[dis(gen)];

		if (FindByCode(roomCode) == nullptr)
			break;
	}

	return roomCode;
}
