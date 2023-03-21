#include "pch.h"
#include "Room.h"
#include "GameSession.h"
#include "RoomManager.h"
#include "ClientPacketHandler.h"

void Room::Enter(GameSessionRef gameSession)
{
	_gameSessions[gameSession->GetSessionId()] = gameSession;
	gameSession->room = static_pointer_cast<Room>(shared_from_this());

	cout << "Room(" << _roomCode << ")'s SessionCount : " << _gameSessions.size() << endl;

	// TODO : S_ENTER_ROOM
	// 본인에게 현재 존재하는 다른 세션들의 정보를 보냄
	// 다른 세션들에게 본인의 정보를 보냄
	Protocol::S_EnterRoom enterRoomPkt;
	enterRoomPkt.set_enterroomok(true);

	auto sendBuffer = ClientPacketHandler::MakeSendBuffer(enterRoomPkt);
	gameSession->Send(sendBuffer);

}

void Room::Leave(GameSessionRef gameSession)
{
	_gameSessions.erase(gameSession->GetSessionId());
	gameSession->room.reset();

	cout << "Room(" << _roomCode << ")'s SessionCount : " << _gameSessions.size() << endl;

	// TODO : S_LEAVE_ROOM

	// 남은 사람이 없으면 방을 없앤다
	if (_gameSessions.size() == 0)
		GRoomManager.Remove(_roomId);
}

void Room::Broadcast(SendBufferRef sendBuffer)
{
	for (auto& gameSession : _gameSessions)
	{
		gameSession.second->Send(sendBuffer);
	}
}
