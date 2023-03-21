#include "pch.h"
#include "GameSession.h"
#include "ClientPacketHandler.h"
#include "SessionManager.h"
#include "Room.h"

void GameSession::OnConnected()
{
	GSessionManager.Generate(static_pointer_cast<GameSession>(shared_from_this()));

	// S_Connected 패킷 전송 (단지 연결되었다는 신호용 패킷이기 때문에 비어있다)
	Protocol::S_Connected connectedPkt;
	auto sendBuffer = ClientPacketHandler::MakeSendBuffer(connectedPkt);
	Send(sendBuffer);
}

void GameSession::OnDisconnected()
{
	GSessionManager.Remove(static_pointer_cast<GameSession>(shared_from_this()));

	if (auto myRoom = room.lock())
		myRoom->DoAsync(&Room::Leave, static_pointer_cast<GameSession>(shared_from_this()));

	Protocol::S_Disconnected disconnectedPkt;
	auto sendBuffer = ClientPacketHandler::MakeSendBuffer(disconnectedPkt);
	Send(sendBuffer);
}

void GameSession::OnRecvPacket(BYTE* buffer, int32 len)
{
	PacketSessionRef session = GetPacketSessionRef();
	PacketHeader* header = reinterpret_cast<PacketHeader*>(buffer);

	// TODO : packetId 대역 체크
	ClientPacketHandler::HandlePacket(session, buffer, len);
}

void GameSession::OnSend(int32 len)
{
}