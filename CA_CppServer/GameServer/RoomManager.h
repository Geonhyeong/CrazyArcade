#pragma once

class Room;

class RoomManager
{
public:
	RoomRef Generate(int mapId);
	void Remove(int roomId);
	RoomRef Find(int roomId);

private:
	USE_LOCK;
	int32 _roomId = 0;
	map<int32, RoomRef> _rooms;
};

extern RoomManager GRoomManager;

