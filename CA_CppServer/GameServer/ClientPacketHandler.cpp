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

// 직접 컨텐츠 작업자

bool Handle_INVALID(PacketSessionRef& session, BYTE* buffer, int32 len)
{
	PacketHeader* header = reinterpret_cast<PacketHeader*>(buffer);
	// TODO : Log
	return false;
}

bool Handle_C_MOVE(PacketSessionRef& session, Protocol::C_Move& pkt)
{
	return false;
}

bool Handle_C_SKILL(PacketSessionRef& session, Protocol::C_Skill& pkt)
{
	return false;
}

bool Handle_C_LOGIN(PacketSessionRef& session, Protocol::C_Login& pkt)
{
	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);

	// Validation 체크 (RedisDB에 token 검증)
	string value = GRedisManager->GetValue(to_string(pkt.accountdbid()));
	json j = json::parse(value);

	bool isValid = true;
	if (pkt.accountdbid() != j["AccountDbId"] || pkt.token() != j["Token"] || j["Expired"] < ::GetTickCount64())
		isValid = false;

	// 현재 접속중인데 또 로그인 시도한건지 확인
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

	// 방 생성 및 입장
	RoomRef room = GRoomManager.Generate();
	room->DoAsync(&Room::Enter, gameSession);

	return true;
}

bool Handle_C_ENTER_ROOM(PacketSessionRef& session, Protocol::C_EnterRoom& pkt)
{
	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);

	string nickname = pkt.nickname();
	gameSession->SetNickname(nickname);

	// 방 검색 후 입장
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
		if (gameSession->GetSessionId() == room->GetHostSessionId())
		{
			// 게임이 시작되었다는 신호를 방 안 세션들에게 Broadcast
			Protocol::S_StartGame startGamePkt;
			auto sendBuffer = ClientPacketHandler::MakeSendBuffer(startGamePkt);
			room->DoAsync(&Room::Broadcast, sendBuffer);
			 
			// 게임 방 생성
			GameRoomRef gameRoom = GGameRoomManager.Add();
			room->SetGameRoomId(gameRoom->GetGameRoomId());
			gameRoom->DoAsync(&GameRoom::Init, room);
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
		// 플레이어 생성
		PlayerRef player = make_shared<Player>();
		player->SetObjectId(GObjectManager.GenerateId(Protocol::GameObjectType::PLAYER));
		player->SetOwnerSession(gameSession);
		player->info->set_name("Player_" + to_string(player->GetObjectId()));
		{
			Protocol::PositionInfo posInfo = player->info->posinfo();
			posInfo.set_state(Protocol::CreatureState::IDLE);
			posInfo.set_movedir(Protocol::MoveDir::DOWN);
			// TODO : 위치는 정해진 몇 곳에서 랜덤 배정
			posInfo.set_posx(0);
			posInfo.set_posy(-1);
			player->info->set_allocated_posinfo(&posInfo);
		}
		// TODO : 스탯 관련 추가
		gameSession->myPlayer = player;

		// 플레이어를 게임에 입장시키기
		GameRoomRef gameRoom = GGameRoomManager.Find(room->GetGameRoomId());
		gameRoom->DoAsync(&GameRoom::EnterGame, static_pointer_cast<GameObject>(player));
	}

	return true;
}