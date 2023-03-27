#include "pch.h"
#include "Item.h"
#include "GameRoom.h"
#include "Player.h"

void Item::OnSpawn()
{
	GameRoomRef gameRoom = room.lock();
	if (gameRoom == nullptr)
		return;

	ItemRef mySelf = static_pointer_cast<Item>(shared_from_this());
	gameRoom->DoAsync([=]() { CheckCollision(mySelf); });
}

void Item::OnAttacked(GameObjectRef attacker)
{
	Despawn();
}

void Item::CheckCollision(ItemRef mySelf)
{
	if (_isExist == false)
		return;

	GameRoomRef gameRoom = room.lock();
	if (gameRoom == nullptr)
		return;

	// 플레이어가 존재하는지 확인
	vector<GameObjectRef> gameObjects = gameRoom->FindAllObjects(GetCellPos());
	for (GameObjectRef go : gameObjects)
	{
		if (go->objectType == Protocol::GameObjectType::PLAYER)
		{
			PlayerRef player = reinterpret_pointer_cast<Player>(go);
			Protocol::CreatureState state = player->info.posinfo().state();
			if (state == Protocol::CreatureState::IDLE || state == Protocol::CreatureState::MOVING)
			{
				player->GetItem(mySelf);
				Despawn();
				return;
			}
		}
	}

	// 0.1초마다 체크
	gameRoom->DoTimer(100, [=]() {	CheckCollision(mySelf); });
}

void Item::Despawn()
{
	GameRoomRef gameRoom = room.lock();
	if (gameRoom == nullptr)
		return;

	_isExist = false;
	gameRoom->DoAsync(&GameRoom::LeaveGame, info.objectid());
}
