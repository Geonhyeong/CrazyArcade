#include "pch.h"
#include "GameRoom.h"
#include "ObjectManager.h"
#include "Player.h"
#include "GameRoomManager.h"
#include "GameSession.h"
#include "ClientPacketHandler.h"

void GameRoom::Init(RoomRef ownerRoom)
{
	_ownerRoom = ownerRoom;
}

void GameRoom::EnterGame(GameObjectRef gameObject)
{
	// TODO
	if (gameObject == nullptr)
		return;

	Protocol::GameObjectType type = ObjectManager::GetObjectTypeById(gameObject->GetObjectId());

	if (type == Protocol::GameObjectType::PLAYER)
	{
		PlayerRef player = reinterpret_pointer_cast<Player>(gameObject);
		_players.insert(make_pair(player->GetObjectId(), player));
		player->room = static_pointer_cast<GameRoom>(shared_from_this());
		
		cout << "Player " << player->GetObjectId() << " : Spawn {" << player->GetCellPos().x << "}, {" << player->GetCellPos().y << "}" << endl;

		// TODO : 본인한테 정보 전송
		{
			GameSessionRef gameSession = player->GetOwnerSession();

			Protocol::S_EnterGame* enterPacket = new Protocol::S_EnterGame();
			enterPacket->set_allocated_player(player->info);
			auto sendBuffer = ClientPacketHandler::MakeSendBuffer(*enterPacket);
			gameSession->Send(sendBuffer);

			Protocol::S_Spawn* spawnPacket = new Protocol::S_Spawn();
			for (auto& it : _players)
			{
				PlayerRef p = it.second;
				if (player != p)
				{
					auto object = spawnPacket->add_objects();
					object->CopyFrom(*(p->info));
				}
			}
			auto sendBuffer2 = ClientPacketHandler::MakeSendBuffer(*spawnPacket);
			gameSession->Send(sendBuffer2);
		}
	}
	else
	{

	}

	// 타인한테 정보 전송
	{
		Protocol::S_Spawn* spawnPacket = new Protocol::S_Spawn();
		auto object = spawnPacket->add_objects();
		object->CopyFrom(*(gameObject->info));
		auto sendBuffer = ClientPacketHandler::MakeSendBuffer(*spawnPacket);
		
		for (auto& it : _players)
		{
			PlayerRef p = it.second;
			if (p->GetObjectId() != gameObject->GetObjectId())
				p->GetOwnerSession()->Send(sendBuffer);
		}
	}
}

void GameRoom::LeaveGame(int32 objectId)
{
	// TODO
	Protocol::GameObjectType type = ObjectManager::GetObjectTypeById(objectId);

	if (type == Protocol::GameObjectType::PLAYER)
	{
		PlayerRef player = _players.find(objectId)->second;
		player->room.reset();
		_players.erase(objectId);

		cout << "Player " << player->GetObjectId() << " : Despawn" << endl;
		
		// 본인한테 정보 전송
		{
			GameSessionRef gameSession = player->GetOwnerSession();

			Protocol::S_LeaveGame leavePacket;
			auto sendBuffer = ClientPacketHandler::MakeSendBuffer(leavePacket);
			gameSession->Send(sendBuffer);
		}

		// 남은 사람이 있는지 체크
		if (_players.size() == 0)
			GGameRoomManager.Remove(_gameRoomId);
	}
	else
	{

	}
	// 타인한테 정보 전송
	{
		Protocol::S_Despawn* despawnPacket = new Protocol::S_Despawn();
		despawnPacket->add_objectids(objectId);
		auto sendBuffer = ClientPacketHandler::MakeSendBuffer(*despawnPacket);

		for (auto& it : _players)
		{
			PlayerRef p = it.second;
			if (p->GetObjectId() != objectId)
				p->GetOwnerSession()->Send(sendBuffer);
		}
	}
}

void GameRoom::Broadcast(SendBufferRef sendBuffer)
{
}
