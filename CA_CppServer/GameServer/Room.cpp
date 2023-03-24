#include "pch.h"
#include "Room.h"
#include "GameSession.h"
#include "RoomManager.h"
#include "ClientPacketHandler.h"
#include "GameRoomManager.h"
#include "GameRoom.h"
#include "Player.h"

void Room::Enter(GameSessionRef gameSession)
{
	// 처음 들어온 사람이 방장
	if (_gameSessions.size() == 0)
		_hostSessionId = gameSession->GetSessionId();

	_gameSessions[gameSession->GetSessionId()] = gameSession;
	gameSession->room = static_pointer_cast<Room>(shared_from_this());

	cout << "Room(" << _roomCode << ")'s SessionCount : " << _gameSessions.size() << endl;

	// 본인에게 S_EnterGame 전송
	Protocol::S_EnterRoom enterRoomPkt;
	enterRoomPkt.set_enterroomok(true);
	{
		Protocol::GameSessionInfo* info = new Protocol::GameSessionInfo();
		info->set_sessionid(gameSession->GetSessionId());
		info->set_nickname(gameSession->GetNickname());
		enterRoomPkt.set_allocated_info(info);
	}
	auto sendBuffer = ClientPacketHandler::MakeSendBuffer(enterRoomPkt);
	gameSession->Send(sendBuffer);

	// 모든 세션에게 리스트의 정보를 브로드캐스트
	Protocol::S_RoomPlayerList listPkt;
	{
		Protocol::RoomInfo* roomInfo = new Protocol::RoomInfo();
		roomInfo->set_roomid(_roomId);
		roomInfo->set_roomcode(_roomCode);
		roomInfo->set_hostsessionid(_hostSessionId);
		listPkt.set_allocated_roominfo(roomInfo);
	}
	{
		for (auto& it : _gameSessions)
		{
			auto gameSession = listPkt.add_gamesessions();
			gameSession->set_sessionid(it.second->GetSessionId());
			gameSession->set_nickname(it.second->GetNickname());
		}
	}
	auto sendBuffer2 = ClientPacketHandler::MakeSendBuffer(listPkt);
	DoAsync(&Room::Broadcast, sendBuffer2);
}

void Room::Leave(GameSessionRef gameSession)
{
	_gameSessions.erase(gameSession->GetSessionId());
	gameSession->room.reset();

	cout << "Room(" << _roomCode << ")'s SessionCount : " << _gameSessions.size() << endl;

	// 혹시 게임방이 있다면 게임방에서도 나가도록 한다
	GameRoomRef gameRoom = GGameRoomManager.Find(_gameRoomId);
	if (gameRoom != nullptr)
	{
		PlayerRef player = gameSession->myPlayer.lock();
		if (player != nullptr)
		{
			gameRoom->DoAsync(&GameRoom::LeaveGame, player->GetObjectId());
		}
	}

	// 남은 사람이 있는지 체크
	if (_gameSessions.size() == 0)
	{
		GRoomManager.Remove(_roomId);
	}
	else
	{
		// 나간 사람이 방장이면 방장을 바꾼다
		if (_hostSessionId == gameSession->GetSessionId())
			_hostSessionId = _gameSessions.begin()->second->GetSessionId();
	}

	// 본인에게는 S_LEAVE_ROOM
	Protocol::S_LeaveRoom leaveRoomPkt;
	auto sendBuffer = ClientPacketHandler::MakeSendBuffer(leaveRoomPkt);
	gameSession->Send(sendBuffer);

	// 다른 사람들에게 S_RoomPlayerList
	Protocol::S_RoomPlayerList listPkt;
	{
		Protocol::RoomInfo* roomInfo = new Protocol::RoomInfo();
		roomInfo->set_roomid(_roomId);
		roomInfo->set_roomcode(_roomCode);
		roomInfo->set_hostsessionid(_hostSessionId);
		listPkt.set_allocated_roominfo(roomInfo);
	}
	{
		for (auto& it : _gameSessions)
		{
			auto gameSession = listPkt.add_gamesessions();
			gameSession->set_sessionid(it.second->GetSessionId());
			gameSession->set_nickname(it.second->GetNickname());
		}
	}
	auto sendBuffer2 = ClientPacketHandler::MakeSendBuffer(listPkt);
	DoAsync(&Room::Broadcast, sendBuffer2);
}

void Room::Broadcast(SendBufferRef sendBuffer)
{
	for (auto& gameSession : _gameSessions)
	{
		gameSession.second->Send(sendBuffer);
	}
}
