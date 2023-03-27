#include "pch.h"
#include "Wave.h"
#include "GameRoom.h"

void Wave::OnSpawn()
{
	GameRoomRef gameRoom = room.lock();
	if (gameRoom == nullptr)
		return;

	gameRoom->DoAsync([=]() { CheckCollision(static_pointer_cast<Wave>(shared_from_this())); });
	// 0.5초 후에 소멸
	gameRoom->DoTimer(500, [=]() { Despawn(); });
}

void Wave::CheckCollision(WaveRef mySelf)
{
	GameRoomRef gameRoom = room.lock();
	if (gameRoom == nullptr)
		return;

	// 이 위치에 오브젝트가 있는지 확인
	vector<GameObjectRef> gameObjects = gameRoom->FindAllObjects(GetCellPos());
	for (GameObjectRef go : gameObjects)
		go->OnAttacked(shared_from_this());

	// 0.1초마다 체크
	gameRoom->DoTimer(100, [=]() { CheckCollision(mySelf); });
}

void Wave::Despawn()
{
	GameRoomRef gameRoom = room.lock();
	if (gameRoom == nullptr)
		return;

	gameRoom->DoAsync(&GameRoom::LeaveGame, info.objectid());
}
