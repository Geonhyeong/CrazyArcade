#pragma once
#include "Protocol.pb.h"

using PacketHandlerFunc = std::function<bool(PacketSessionRef&, BYTE*, int32)>;
extern PacketHandlerFunc GPacketHandler[UINT16_MAX];

// Custom Handlers
bool Handle_INVALID(PacketSessionRef& session, BYTE* buffer, int32 len);
bool Handle_C_MOVE(PacketSessionRef& session, Protocol::C_Move& pkt);
bool Handle_C_SKILL(PacketSessionRef& session, Protocol::C_Skill& pkt);
bool Handle_C_LOGIN(PacketSessionRef& session, Protocol::C_Login& pkt);

class ClientPacketHandler
{
public:
	static void Init()
	{
		for (int32 i = 0; i < UINT16_MAX; i++)
			GPacketHandler[i] = Handle_INVALID;
		GPacketHandler[Protocol::C_MOVE] = [](PacketSessionRef& session, BYTE* buffer, int32 len) { return HandlePacket<Protocol::C_Move>(Handle_C_MOVE, session, buffer, len); };
		GPacketHandler[Protocol::C_SKILL] = [](PacketSessionRef& session, BYTE* buffer, int32 len) { return HandlePacket<Protocol::C_Skill>(Handle_C_SKILL, session, buffer, len); };
		GPacketHandler[Protocol::C_LOGIN] = [](PacketSessionRef& session, BYTE* buffer, int32 len) { return HandlePacket<Protocol::C_Login>(Handle_C_LOGIN, session, buffer, len); };
	}

	static bool HandlePacket(PacketSessionRef& session, BYTE* buffer, int32 len)
	{
		PacketHeader* header = reinterpret_cast<PacketHeader*>(buffer);
		return GPacketHandler[header->id](session, buffer, len);
	}
	static SendBufferRef MakeSendBuffer(Protocol::S_EnterGame& pkt) { return MakeSendBuffer(pkt, Protocol::S_ENTER_GAME); }
	static SendBufferRef MakeSendBuffer(Protocol::S_LeaveGame& pkt) { return MakeSendBuffer(pkt, Protocol::S_LEAVE_GAME); }
	static SendBufferRef MakeSendBuffer(Protocol::S_Spawn& pkt) { return MakeSendBuffer(pkt, Protocol::S_SPAWN); }
	static SendBufferRef MakeSendBuffer(Protocol::S_Despawn& pkt) { return MakeSendBuffer(pkt, Protocol::S_DESPAWN); }
	static SendBufferRef MakeSendBuffer(Protocol::S_Move& pkt) { return MakeSendBuffer(pkt, Protocol::S_MOVE); }
	static SendBufferRef MakeSendBuffer(Protocol::S_ChangeStat& pkt) { return MakeSendBuffer(pkt, Protocol::S_CHANGE_STAT); }
	static SendBufferRef MakeSendBuffer(Protocol::S_Pop& pkt) { return MakeSendBuffer(pkt, Protocol::S_POP); }
	static SendBufferRef MakeSendBuffer(Protocol::S_Trap& pkt) { return MakeSendBuffer(pkt, Protocol::S_TRAP); }
	static SendBufferRef MakeSendBuffer(Protocol::S_Die& pkt) { return MakeSendBuffer(pkt, Protocol::S_DIE); }
	static SendBufferRef MakeSendBuffer(Protocol::S_Connected& pkt) { return MakeSendBuffer(pkt, Protocol::S_CONNECTED); }
	static SendBufferRef MakeSendBuffer(Protocol::S_Login& pkt) { return MakeSendBuffer(pkt, Protocol::S_LOGIN); }

private:
	template<typename PacketType, typename ProcessFunc>
	static bool HandlePacket(ProcessFunc func, PacketSessionRef& session, BYTE* buffer, int32 len)
	{
		PacketType pkt;
		if (pkt.ParseFromArray(buffer + sizeof(PacketHeader), len - sizeof(PacketHeader)) == false)
			return false;

		return func(session, pkt);
	}

	template<typename T>
	static SendBufferRef MakeSendBuffer(T& pkt, uint16 pktId)
	{
		const uint16 dataSize = static_cast<uint16>(pkt.ByteSizeLong());
		const uint16 packetSize = dataSize + sizeof(PacketHeader);

		SendBufferRef sendBuffer = GSendBufferManager->Open(packetSize);
		PacketHeader* header = reinterpret_cast<PacketHeader*>(sendBuffer->Buffer());
		header->size = packetSize;
		header->id = pktId;
		ASSERT_CRASH(pkt.SerializeToArray(&header[1], dataSize));
		sendBuffer->Close(packetSize);

		return sendBuffer;
	}
};
