#pragma once
#include "JobQueue.h"

enum
{
	MAX_SESSION_COUNT = 8
};

class Room : public JobQueue
{
public:
	~Room()
	{
		cout << "~Room" << endl;
	}

	// 싱글쓰레드처럼 작업해도 된다
	void Enter(GameSessionRef gameSession);
	void Leave(GameSessionRef gameSession);
	void Broadcast(SendBufferRef sendBuffer);

	bool CanEnter() { return _gameSessions.size() < MAX_SESSION_COUNT; }

public:
	int32 GetRoomId() { return _roomId; }
	void SetRoomId(int32 roomId) { _roomId = roomId; }
	string GetRoomCode() { return _roomCode; }
	void SetRoomCode(string roomCode) { _roomCode = roomCode; }
	int32 GetHostSessionId() { return _hostSessionId; }
	int32 GetGameRoomId() { return _gameRoomId; }
	void SetGameRoomId(int32 gameRoomId) { _gameRoomId = gameRoomId; }
	int32 GetSessionCount() { return _gameSessions.size(); }

private:
	int32 _roomId;
	string _roomCode;
	map<int32, GameSessionRef> _gameSessions;
	int32 _hostSessionId;
	int32 _gameRoomId;
};