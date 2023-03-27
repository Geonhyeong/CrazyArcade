#include "pch.h"
#include "ClientPacketHandler.h"
#include "GameSession.h"
#include "RedisManager.h"
#include <nlohmann/json.hpp>
#include "RoomManager.h"
#include "Room.h"
#include "SessionManager.h"
#include "GameRoomManager.h"
#include "GameRoom.h"
#include "ObjectManager.h"
#include "Player.h"

using json = nlohmann::json;

PacketHandlerFunc GPacketHandler[UINT16_MAX];

// ���� ������ �۾���

bool Handle_INVALID(PacketSessionRef& session, BYTE* buffer, int32 len)
{
	PacketHeader* header = reinterpret_cast<PacketHeader*>(buffer);
	// TODO : Log
	return false;
}

bool Handle_C_MOVE(PacketSessionRef& session, Protocol::C_Move& pkt)
{
	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);
	PlayerRef player = gameSession->myPlayer.lock();

	if (player != nullptr)
	{
		GameRoomRef gameRoom = player->room.lock();
		if (gameRoom != nullptr)
		{
			gameRoom->DoAsync(&GameRoom::HandleMove, player, pkt);
		}
	}

	return true;
}

bool Handle_C_SKILL(PacketSessionRef& session, Protocol::C_Skill& pkt)
{
	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);
	PlayerRef player = gameSession->myPlayer.lock();

	if (player != nullptr)
	{
		GameRoomRef gameRoom = player->room.lock();
		if (gameRoom != nullptr)
		{
			gameRoom->DoAsync(&GameRoom::HandleSkill, player, pkt);
		}
	}

	return true;
}

bool Handle_C_LOGIN(PacketSessionRef& session, Protocol::C_Login& pkt)
{
	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);

	// Validation üũ (RedisDB�� token ����)
	string value = GRedisManager->GetValue(to_string(pkt.accountdbid()));
	json j = json::parse(value);

	bool isValid = true;
	if (pkt.accountdbid() != j["AccountDbId"] || pkt.token() != j["Token"] || j["Expired"] < ::GetTickCount64())
		isValid = false;

	// ���� �������ε� �� �α��� �õ��Ѱ��� Ȯ��
	if (GSessionManager.FindByAccountDbId(pkt.accountdbid()) != nullptr)
		isValid = false;

	cout << "AccountId: " << pkt.accountdbid() << " Access Complete!" << endl;

	Protocol::S_Login loginPkt;
	loginPkt.set_loginok(isValid);
	auto sendBuffer = ClientPacketHandler::MakeSendBuffer(loginPkt);
	session->Send(sendBuffer);
	
	if (isValid)
		gameSession->SetAccountDbId(pkt.accountdbid());
	else
		session->Disconnect(L"Invalid Session!");
	
	return true;
}

bool Handle_C_CREATE_ROOM(PacketSessionRef& session, Protocol::C_CreateRoom& pkt)
{
	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);
	
	string nickname = pkt.nickname();
	gameSession->SetNickname(nickname);

	// �� ���� �� ����
	RoomRef room = GRoomManager.Generate();
	room->DoAsync(&Room::Enter, gameSession);

	return true;
}

bool Handle_C_ENTER_ROOM(PacketSessionRef& session, Protocol::C_EnterRoom& pkt)
{
	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);

	string nickname = pkt.nickname();
	gameSession->SetNickname(nickname);

	// �� �˻� �� ����
	if (pkt.roomcode() == "RANDOM")
	{
		RoomRef room = GRoomManager.GetRandomRoom();
		if (room != nullptr)
		{
			room->DoAsync(&Room::Enter, gameSession);
		}
		else
		{
			Protocol::S_EnterRoom enterRoomPkt;
			enterRoomPkt.set_enterroomok(false);
			auto sendBuffer = ClientPacketHandler::MakeSendBuffer(enterRoomPkt);
			session->Send(sendBuffer);
		}
	}
	else
	{
		RoomRef room = GRoomManager.FindByCode(pkt.roomcode());
		if (room != nullptr && room->CanEnter())
		{
			room->DoAsync(&Room::Enter, gameSession);
		}
		else
		{
			Protocol::S_EnterRoom enterRoomPkt;
			enterRoomPkt.set_enterroomok(false);
			auto sendBuffer = ClientPacketHandler::MakeSendBuffer(enterRoomPkt);
			session->Send(sendBuffer);
		}
	}

	return true;
}

bool Handle_C_LEAVE_ROOM(PacketSessionRef& session, Protocol::C_LeaveRoom& pkt)
{
	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);

	RoomRef room = gameSession->room.lock();
	if (room != nullptr)
		room->DoAsync(&Room::Leave, gameSession);

	return true;
}

bool Handle_C_START_GAME(PacketSessionRef& session, Protocol::C_StartGame& pkt)
{
	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);
	RoomRef room = gameSession->room.lock();

	if (room != nullptr)
	{
		if (gameSession->GetSessionId() == room->GetHostSessionId() && room->GetSessionCount() > 1)
		{
			// ���� �� ����
			GameRoomRef gameRoom = GGameRoomManager.Add();
			room->SetGameRoomId(gameRoom->GetGameRoomId());
			gameRoom->DoAsync(&GameRoom::Init, room);
			 
			// ������ ���۵Ǿ��ٴ� ��ȣ�� �� �� ���ǵ鿡�� Broadcast
			Protocol::S_StartGame startGamePkt;
			auto sendBuffer = ClientPacketHandler::MakeSendBuffer(startGamePkt);
			room->DoAsync(&Room::Broadcast, sendBuffer);
		}
	}

	return true;
}

bool Handle_C_ENTER_GAME(PacketSessionRef& session, Protocol::C_EnterGame& pkt)
{
	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);
	RoomRef room = gameSession->room.lock();
	
	if (room != nullptr)
	{
		// �÷��̾� ����
		PlayerRef player = make_shared<Player>();
		player->SetObjectId(GObjectManager.GenerateId(Protocol::GameObjectType::PLAYER));
		player->SetOwnerSession(gameSession);
		player->info.set_name("Player_" + to_string(player->GetObjectId()));
		{
			Protocol::PositionInfo* posInfo = player->info.mutable_posinfo();
			posInfo->set_state(Protocol::CreatureState::IDLE);
			posInfo->set_movedir(Protocol::MoveDir::DOWN);
			posInfo->set_posx(1);
			posInfo->set_posy(1);
		}
		{	// ���� ���� �߰�
			Protocol::StatInfo* statInfo = player->info.mutable_statinfo();
			statInfo->set_speedlvl(1);
			statInfo->set_power(1);
			statInfo->set_availbubble(1);
		}
		gameSession->myPlayer = player;

		// �÷��̾ ���ӿ� �����Ű��
		GameRoomRef gameRoom = GGameRoomManager.Find(room->GetGameRoomId());
		gameRoom->DoAsync(&GameRoom::EnterGame, static_pointer_cast<GameObject>(player));
	}

	return true;
}