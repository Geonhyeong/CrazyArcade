#include "pch.h"
#include "Bubble.h"
#include "Player.h"
#include "GameRoom.h"
#include "ClientPacketHandler.h"
#include "ObjectManager.h"
#include "Wave.h"

Bubble::Bubble(PlayerRef owner, int32 power)
	: _owner(owner), _power(power)
{
	objectType = Protocol::GameObjectType::BUBBLE;
}

void Bubble::OnSpawn()
{
	_owner->bubbleCount++;
	
	GameRoomRef gameRoom = room.lock();
	if (gameRoom == nullptr)
		return;

	BubbleRef mySelf = static_pointer_cast<Bubble>(shared_from_this());
	gameRoom->DoTimer(2500, [=]() { Pop(mySelf); });
}

void Bubble::OnAttacked(GameObjectRef attacker)
{
	Pop(static_pointer_cast<Bubble>(shared_from_this()));
}

void Bubble::Pop(BubbleRef mySelf)
{
	GameRoomRef gameRoom = room.lock();
	if (gameRoom == nullptr)
		return;
	if (info.posinfo().state() == Protocol::CreatureState::POP)
		return;

	_owner->bubbleCount--;
	info.mutable_posinfo()->set_state(Protocol::CreatureState::POP);

	// Pop 패킷 Broadcast
	Protocol::S_Pop popPacket;
	popPacket.set_objectid(info.objectid());
	auto sendBuffer = ClientPacketHandler::MakeSendBuffer(popPacket);
	gameRoom->DoAsync(&GameRoom::Broadcast, sendBuffer);

	// 이 위치에 오브젝트가 있는지 확인
	vector<GameObjectRef> gameObjects = gameRoom->FindAllObjects(GetCellPos());
	for (GameObjectRef go : gameObjects)
		go->OnAttacked(shared_from_this());

	// 상하좌우로 Power만큼 웨이브 소환
	for (int32 waveDir = 0; waveDir < 4; waveDir++)
	{
		for (int32 i = 1; i <= _power; i++)
		{
			Vector2Int wavePos = GetFrontCellPos((Protocol::MoveDir)waveDir, i);
			if (gameRoom->gameMap.CanGo(wavePos, true) == false)
			{
				GameObjectRef go = gameRoom->gameMap.Find(wavePos);
				if (go != nullptr)
					go->OnAttacked(shared_from_this());
				break;
			}

			// 웨이브 생성
			WaveRef wave = make_shared<Wave>();
			wave->SetObjectId(GObjectManager.GenerateId(Protocol::GameObjectType::WAVE));
			wave->info.set_name("Wave_" + ::to_string(wave->GetObjectId()));
			{
				Protocol::PositionInfo* posInfo = wave->info.mutable_posinfo();
				posInfo->set_movedir((Protocol::MoveDir)waveDir);
				posInfo->set_posx(wavePos.x);
				posInfo->set_posy(wavePos.y);
			}
			wave->info.set_isedge((i == _power));

			gameRoom->DoAsync(&GameRoom::EnterGame, static_pointer_cast<GameObject>(wave));
		}
	}

	// 0.5초 후에 소멸
	gameRoom->DoTimer(500, [=]() { Despawn(); });
}

void Bubble::Despawn()
{
	GameRoomRef gameRoom = room.lock();
	if (gameRoom == nullptr)
		return;

	gameRoom->DoAsync(&GameRoom::LeaveGame, info.objectid());
}
