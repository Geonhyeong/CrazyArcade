#include "pch.h"
#include "ClientPacketHandler.h"
#include "GameSession.h"
#include "RedisManager.h"
#include <nlohmann/json.hpp>
#include "RoomManager.h"
#include "Room.h"
#include "SessionManager.h"

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
	room->DoAsync(&Room::Leave, gameSession);

	return true;
}

//bool Handle_C_LOGIN(PacketSessionRef& session, Protocol::C_LOGIN& pkt)
//{
//	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);
//
//	// TODO : Validation 체크
//
//	Protocol::S_LOGIN loginPkt;
//	loginPkt.set_success(true);
//	
//	// DB에서 플레이 정보를 긁어온다
//	// GameSession에 플레이 정보를 저장 (메모리)
//	
//	// ID 발급 (DB 아이디가 아니고, 인게임 아이디)
//	static Atomic<uint64> idGenerator = 1;
//
//	{
//		auto player = loginPkt.add_players();
//		player->set_name(u8"DB에서 긁어온 이름1");
//		player->set_playertype(Protocol::PLAYER_TYPE_KNIGHT);
//
//		PlayerRef playerRef = make_shared<Player>();
//		playerRef->playerId = idGenerator++;
//		playerRef->name = player->name();
//		playerRef->type = player->playertype();
//		playerRef->ownerSession = gameSession;
//
//		gameSession->_players.push_back(playerRef);
//	}
//
//	{
//		auto player = loginPkt.add_players();
//		player->set_name(u8"DB에서 긁어온 이름2");
//		player->set_playertype(Protocol::PLAYER_TYPE_MAGE);
//
//		PlayerRef playerRef = make_shared<Player>();
//		playerRef->playerId = idGenerator++;
//		playerRef->name = player->name();
//		playerRef->type = player->playertype();
//		playerRef->ownerSession = gameSession;
//
//		gameSession->_players.push_back(playerRef);
//	}
//
//	auto sendBuffer = ClientPacketHandler::MakeSendBuffer(loginPkt);
//	session->Send(sendBuffer);
//
//	return true;
//}
//
//bool Handle_C_ENTER_GAME(PacketSessionRef& session, Protocol::C_ENTER_GAME& pkt)
//{
//	GameSessionRef gameSession = static_pointer_cast<GameSession>(session);
//
//	uint64 index = pkt.playerindex();
//	// TODO : Validation
//
//	gameSession->_currentPlayer = gameSession->_players[index]; // READ_ONLY?
//	gameSession->_room = GRoom;
//
//	GRoom->DoAsync(&Room::Enter, gameSession->_currentPlayer);
//
//	Protocol::S_ENTER_GAME enterGamePkt;
//	enterGamePkt.set_success(true);
//	auto sendBuffer = ClientPacketHandler::MakeSendBuffer(enterGamePkt);
//	gameSession->_currentPlayer->ownerSession->Send(sendBuffer);
//
//	return true;
//
//}
//
//bool Handle_C_CHAT(PacketSessionRef& session, Protocol::C_CHAT& pkt)
//{
//	std::cout << pkt.msg() << endl;
//
//	Protocol::S_CHAT chatPkt;
//	chatPkt.set_msg(pkt.msg());
//	auto sendBuffer = ClientPacketHandler::MakeSendBuffer(chatPkt);
//
//	GRoom->DoAsync(&Room::Broadcast, sendBuffer);
//
//	return true;
//}