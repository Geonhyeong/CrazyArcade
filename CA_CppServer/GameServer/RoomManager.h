#pragma once

class Room;

class RoomManager
{
public:
	RoomRef Generate(string roomCode = "");
	void Remove(int roomId);
	RoomRef Find(int roomId);
	RoomRef FindByCode(string roomCode);
	RoomRef GetRandomRoom();

private:
	string MakeRoomCode();

private:
	USE_LOCK;
	int32 _roomId = 0;
	map<int32, RoomRef> _rooms;
};

extern RoomManager GRoomManager;