#include "pch.h"
#include "Wave.h"
#include "GameRoom.h"

void Wave::OnSpawn()
{
	GameRoomRef gameRoom = room.lock();
	if (gameRoom == nullptr)
		return;

	gameRoom->DoAsync([=]() { CheckCollision(static_pointer_cast<Wave>(shared_from_this())); });
	// 0.5�� �Ŀ� �Ҹ�
	gameRoom->DoTimer(500, [=]() { Despawn(); });
}

void Wave::CheckCollision(WaveRef mySelf)
{
	GameRoomRef gameRoom = room.lock();
	if (gameRoom == nullptr)
		return;

	// �� ��ġ�� ������Ʈ�� �ִ��� Ȯ��
	vector<GameObjectRef> gameObjects = gameRoom->FindAllObjects(GetCellPos());
	for (GameObjectRef go : gameObjects)
		go->OnAttacked(shared_from_this());

	// 0.1�ʸ��� üũ
	gameRoom->DoTimer(100, [=]() { CheckCollision(mySelf); });
}

void Wave::Despawn()
{
	GameRoomRef gameRoom = room.lock();
	if (gameRoom == nullptr)
		return;

	gameRoom->DoAsync(&GameRoom::LeaveGame, info.objectid());
}
