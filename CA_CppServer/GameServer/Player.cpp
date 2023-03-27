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

	// S_ChangeStat ��Ŷ Broadcast
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

	// Trap ��Ŷ Broadcast
	Protocol::S_Trap trapPacket;
	trapPacket.set_playerid(info.objectid());
	auto sendBuffer = ClientPacketHandler::MakeSendBuffer(trapPacket);
	gameRoom->DoAsync(&GameRoom::Broadcast, sendBuffer);

	// �ٸ� �÷��̾ �����ߴ��� üũ
	gameRoom->DoAsync([=]() { CheckCollision(); });

	// 5�� �� ����
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

	// ���� ��ġ�� �÷��̾ �ִ��� üũ
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

	// 0.1�ʸ��� üũ
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

	// Die ��Ŷ Broadcast
	Protocol::S_Die diePacket;
	diePacket.set_playerid(info.objectid());
	auto sendBuffer = ClientPacketHandler::MakeSendBuffer(diePacket);
	gameRoom->DoAsync(&GameRoom::Broadcast, sendBuffer);

	// ������ �������� Ȯ��
	gameRoom->DoAsync(&GameRoom::CheckAndEnd);
}
