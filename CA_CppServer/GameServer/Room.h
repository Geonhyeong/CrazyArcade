#pragma once
#include "JobQueue.h"

class Room : public JobQueue
{
public:
	void Enter(GameObject gameObject);
	void Leave(int objectId);
	void Broadcast(SendBufferRef sendBuffer);

public:
	int32 GetRoomId() { return _roomId; }
	void SetRoomId(int32 roomId) { _roomId = roomId; }

private:
	int32 _roomId;
};

