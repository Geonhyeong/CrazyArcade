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
	// ó�� ���� ����� ����
	if (_gameSessions.size() == 0)
		_hostSessionId = gameSession->GetSessionId();

	_gameSessions[gameSession->GetSessionId()] = gameSession;
	gameSession->room = static_pointer_cast<Room>(shared_from_this());

	cout << "Room(" << _roomCode << ")'s SessionCount : " << _gameSessions.size() << endl;

	// ���ο��� S_EnterGame ����
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

	// ��� ���ǿ��� ����Ʈ�� ������ ��ε�ĳ��Ʈ
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

	// Ȥ�� ���ӹ��� �ִٸ� ���ӹ濡���� �������� �Ѵ�
	GameRoomRef gameRoom = GGameRoomManager.Find(_gameRoomId);
	if (gameRoom != nullptr)
	{
		PlayerRef player = gameSession->myPlayer.lock();
		if (player != nullptr)
		{
			gameRoom->DoAsync(&GameRoom::LeaveGame, player->GetObjectId());
		}
	}

	// ���� ����� �ִ��� üũ
	if (_gameSessions.size() == 0)
	{
		GRoomManager.Remove(_roomId);
	}
	else
	{
		// ���� ����� �����̸� ������ �ٲ۴�
		if (_hostSessionId == gameSession->GetSessionId())
			_hostSessionId = _gameSessions.begin()->second->GetSessionId();
	}

	// ���ο��Դ� S_LEAVE_ROOM
	Protocol::S_LeaveRoom leaveRoomPkt;
	auto sendBuffer = ClientPacketHandler::MakeSendBuffer(leaveRoomPkt);
	gameSession->Send(sendBuffer);

	// �ٸ� ����鿡�� S_RoomPlayerList
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
