#include "pch.h"
#include "Player.h"
#include "GameRoom.h"
#include "ClientPacketHandler.h"
#include "Item.h"

void Player::GetItem(ItemRef item)
{
	if (item == nullptr)
		return;

	cout << "Player " << info.objectid() << " get Item_" << item->GetObjectId() << ", " << item->itemType << endl;

	Protocol::StatInfo* stat = info.mutable_statinfo();
	switch (item->itemType)
	{
		case ItemType::BALLOON:
		{
			if (stat->availbubble() < MAX_BUBBLE_COUNT)
				stat->set_availbubble(stat->availbubble() + 1);
			break;
		}
		case ItemType::POTION:
		{
			if (stat->power() < MAX_POWER)
				stat->set_power(stat->power() + 1);
			break;
		}
		case ItemType::POTION_MAKE_POWER_MAX:
		{
			if (stat->power() < MAX_POWER)
				stat->set_power(MAX_POWER);
			break;
		}
		case ItemType::SKATE:
		{
			if (stat->speedlvl() < MAX_SPEED_LEVEL)
				stat->set_speedlvl(stat->speedlvl() + 1);
			break;
		}
		default:
			break;
	}

	// S_ChangeStat 패킷 Broadcast
	Protocol::S_ChangeStat changeStatPacket;
	changeStatPacket.set_playerid(info.objectid());
	changeStatPacket.mutable_statinfo()->CopyFrom(info.statinfo());
	auto sendBuffer = ClientPacketHandler::MakeSendBuffer(changeStatPacket);
	room.lock()->DoAsync(&GameRoom::Broadcast, sendBuffer);
}

void Player::OnAttacked(GameObjectRef attacker)
{
	GameRoomRef gameRoom = room.lock();
	if (gameRoom == nullptr)
		return;

	Protocol::CreatureState currentState = info.posinfo().state();
	if (currentState == Protocol::CreatureState::TRAP || currentState == Protocol::CreatureState::DEAD)
		return;

	info.mutable_posinfo()->set_state(Protocol::CreatureState::TRAP);

	// Trap 패킷 Broadcast
	Protocol::S_Trap trapPacket;
	trapPacket.set_playerid(info.objectid());
	auto sendBuffer = ClientPacketHandler::MakeSendBuffer(trapPacket);
	gameRoom->DoAsync(&GameRoom::Broadcast, sendBuffer);

	// 다른 플레이어가 접근했는지 체크
	gameRoom->DoAsync([=]() { CheckCollision(); });

	// 5초 후 죽음
	PlayerRef mySelf = static_pointer_cast<Player>(shared_from_this());
	gameRoom->DoTimer(5000, [=]() { OnDead(mySelf); });
}

void Player::CheckCollision()
{
	GameRoomRef gameRoom = room.lock();
	if (gameRoom == nullptr)
		return;

	Protocol::CreatureState currentState = info.posinfo().state();
	if (currentState != Protocol::CreatureState::TRAP)
		return;

	// 현재 위치에 플레이어가 있는지 체크
	vector<GameObjectRef> gameObjects = gameRoom->FindAllObjects(GetCellPos());
	for (GameObjectRef go : gameObjects)
	{
		if (go->objectType == Protocol::GameObjectType::PLAYER)
		{
			PlayerRef player = reinterpret_pointer_cast<Player>(go);
			if (player->GetObjectId() != info.objectid())
			{
				Protocol::CreatureState state = player->info.posinfo().state();
				if (state == Protocol::CreatureState::IDLE || state == Protocol::CreatureState::MOVING)
				{
					OnDead(static_pointer_cast<Player>(shared_from_this()));
					return;
				}
			}
		}
	}

	// 0.1초마다 체크
	gameRoom->DoTimer(100, [=]() { CheckCollision(); });
}

void Player::OnDead(PlayerRef mySelf)
{
	GameRoomRef gameRoom = room.lock();
	if (gameRoom == nullptr)
		return;

	Protocol::CreatureState currentState = info.posinfo().state();
	if (currentState != Protocol::CreatureState::TRAP)
		return;

	info.mutable_posinfo()->set_state(Protocol::CreatureState::DEAD);

	// Die 패킷 Broadcast
	Protocol::S_Die diePacket;
	diePacket.set_playerid(info.objectid());
	auto sendBuffer = ClientPacketHandler::MakeSendBuffer(diePacket);
	gameRoom->DoAsync(&GameRoom::Broadcast, sendBuffer);

	// 게임이 끝나는지 확인
	gameRoom->DoAsync(&GameRoom::CheckAndEnd);
}
