#include "pch.h"
#include "Block.h"
#include "GameRoom.h"
#include "ClientPacketHandler.h"
#include "ObjectManager.h"
#include "Item.h"
#include <random>

void Block::OnAttacked(GameObjectRef attacker)
{
	GameRoomRef gameRoom = room.lock();
	if (gameRoom == nullptr)
		return;

	// Pop 패킷 전송
	Protocol::S_Pop popPacket;
	popPacket.set_objectid(info.objectid());
	auto sendBuffer = ClientPacketHandler::MakeSendBuffer(popPacket);
	gameRoom->DoAsync(&GameRoom::Broadcast, sendBuffer);

	// 0.5초 후 소멸
	gameRoom->DoTimer(500, [=]() { Despawn(); });
}

void Block::Despawn()
{
	GameRoomRef gameRoom = room.lock();
	if (gameRoom == nullptr)
		return;

	DropItem();

	gameRoom->DoAsync(&GameRoom::LeaveGame, info.objectid());
}

void Block::DropItem()
{
	// item 생성 (일단 랜덤으로)
	::random_device rd;
	::mt19937 gen(rd());
	::uniform_int_distribution<int> dis(0, 99);

	int randInt = dis(gen);
	int32 itemType = 0;
	if (randInt < 40)
		return;
	else if (randInt < 60)
		itemType = ItemType::BALLOON;
	else if (randInt < 75)
		itemType = ItemType::POTION;
	else if (randInt < 80)
		itemType = ItemType::POTION_MAKE_POWER_MAX;
	else
		itemType = ItemType::SKATE;

	ItemRef item = make_shared<Item>();
	item->SetObjectId(GObjectManager.GenerateId(Protocol::GameObjectType::ITEM));
	item->itemType = itemType;
	item->info.set_name("Item_" + ::to_string(itemType));
	{
		Protocol::PositionInfo* posInfo = item->info.mutable_posinfo();
		posInfo->set_state(Protocol::CreatureState::IDLE);
		posInfo->set_posx(info.posinfo().posx());
		posInfo->set_posy(info.posinfo().posy());
	}

	room.lock()->DoAsync(&GameRoom::EnterGame, static_pointer_cast<GameObject>(item));
}
