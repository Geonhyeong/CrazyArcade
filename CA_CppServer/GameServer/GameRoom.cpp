#include "pch.h"
#include "GameRoom.h"
#include "ObjectManager.h"
#include "Player.h"
#include "GameRoomManager.h"
#include "GameSession.h"
#include "ClientPacketHandler.h"
#include "Block.h"
#include "Bubble.h"
#include "Wave.h"
#include "Item.h"
#include <random>

GameRoom::~GameRoom()
{
	cout << "~GameRoom" << endl;

	_ownerRoom = nullptr;
	_players.clear();
	_blocks.clear();
}

void GameRoom::Init(RoomRef ownerRoom, int32 mapId)
{
	_ownerRoom = ownerRoom;
	gameMap.LoadMap(mapId);	// TODO : 일단 지금은 맵이 1개
	gameMap.LoadObjects(static_pointer_cast<GameRoom>(shared_from_this()));
}

void GameRoom::EnterGame(GameObjectRef gameObject)
{
	if (gameObject == nullptr)
		return;

	Protocol::GameObjectType type = ObjectManager::GetObjectTypeById(gameObject->GetObjectId());

	if (type == Protocol::GameObjectType::PLAYER)
	{
		PlayerRef player = reinterpret_pointer_cast<Player>(gameObject);
		player->room = static_pointer_cast<GameRoom>(shared_from_this());
		{
			// 위치는 정해진 몇 곳에 순차적으로 배정
			Protocol::PositionInfo* posInfo = player->info.mutable_posinfo();
			/*Vector2Int startPoint = START_POINTS[_players.size()];
			posInfo->set_posx(startPoint.x);
			posInfo->set_posy(startPoint.y);*/
			::random_device rd;
			::mt19937 gen(rd());
			::uniform_int_distribution<int> dis(-20, 20);
			posInfo->set_posx(dis(gen));
			posInfo->set_posy(dis(gen));
		}
		_players.insert(make_pair(player->GetObjectId(), player));
		
		cout << "Player " << player->GetObjectId() << " : Spawn {" << player->GetCellPos().x << "}, {" << player->GetCellPos().y << "}" << endl;

		// 본인한테 정보 전송
		{
			GameSessionRef gameSession = player->GetOwnerSession();

			Protocol::S_EnterGame enterPacket;
			enterPacket.mutable_player()->CopyFrom(player->info);
			auto sendBuffer = ClientPacketHandler::MakeSendBuffer(enterPacket);
			gameSession->Send(sendBuffer);

			Protocol::S_Spawn spawnPacket;
			for (auto& it : _players)
			{
				PlayerRef p = it.second;
				if (player != p)
				{
					auto object = spawnPacket.add_objects();
					object->CopyFrom(p->info);
				}
			}

			for (auto& it : _blocks)
			{
				BlockRef block = it.second;
				auto object = spawnPacket.add_objects();
				object->CopyFrom(block->info);
				
			}

			for (auto& it : _bubbles)
			{
				BubbleRef bubble = it.second;
				auto object = spawnPacket.add_objects();
				object->CopyFrom(bubble->info);
			}

			for (auto& it : _waves)
			{
				WaveRef wave = it.second;
				auto object = spawnPacket.add_objects();
				object->CopyFrom(wave->info);
			}

			for (auto& it : _items)
			{
				ItemRef item = it.second;
				auto object = spawnPacket.add_objects();
				object->CopyFrom(item->info);
			}
			auto sendBuffer2 = ClientPacketHandler::MakeSendBuffer(spawnPacket);
			gameSession->Send(sendBuffer2);
		}
	}
	else if (type == Protocol::GameObjectType::BLOCK)
	{
		BlockRef block = reinterpret_pointer_cast<Block>(gameObject);
		block->room = static_pointer_cast<GameRoom>(shared_from_this());
		_blocks.insert(make_pair(block->GetObjectId(), block));

		//cout << "Block " << block->GetObjectId() << " : Spawn {" << block->GetCellPos().x << "}, {" << block->GetCellPos().y << "}" << endl;
	}
	else if (type == Protocol::GameObjectType::BUBBLE)
	{
		BubbleRef bubble = reinterpret_pointer_cast<Bubble>(gameObject);
		bubble->room = static_pointer_cast<GameRoom>(shared_from_this());
		_bubbles.insert(make_pair(bubble->GetObjectId(), bubble));

		gameMap.ApplySpawn(bubble);	// 충동체이므로 Map에 적용
		bubble->OnSpawn();
	}
	else if (type == Protocol::GameObjectType::WAVE)
	{
		WaveRef wave = reinterpret_pointer_cast<Wave>(gameObject);
		wave->room = static_pointer_cast<GameRoom>(shared_from_this());
		_waves.insert(make_pair(wave->GetObjectId(), wave));

		wave->OnSpawn();
	}
	else if (type == Protocol::GameObjectType::ITEM)
	{
		ItemRef item = reinterpret_pointer_cast<Item>(gameObject);
		item->room = static_pointer_cast<GameRoom>(shared_from_this());
		_items.insert(make_pair(item->GetObjectId(), item));

		item->OnSpawn();
	}

	// 타인한테 정보 전송
	{
		Protocol::S_Spawn spawnPacket;
		auto object = spawnPacket.add_objects();
		object->CopyFrom(gameObject->info);
		auto sendBuffer = ClientPacketHandler::MakeSendBuffer(spawnPacket);
		
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
	else if (type == Protocol::GameObjectType::BLOCK)
	{
		BlockRef block = _blocks.find(objectId)->second;
		gameMap.ApplyDespawn(block);
		block->room.reset();
		_blocks.erase(objectId);

		cout << "Block " << block->GetObjectId() << " : Despawn" << endl;
	}
	else if (type == Protocol::GameObjectType::BUBBLE)
	{
		BubbleRef bubble = _bubbles.find(objectId)->second;
		gameMap.ApplyDespawn(bubble);
		bubble->room.reset();
		_bubbles.erase(objectId);

		cout << "Bubble " << bubble->GetObjectId() << " : Despawn" << endl;
	}
	else if (type == Protocol::GameObjectType::WAVE)
	{
		WaveRef wave = _waves.find(objectId)->second;
		wave->room.reset();
		_waves.erase(objectId);

		cout << "Wave " << wave->GetObjectId() << " : Despawn" << endl;
	}
	else if (type == Protocol::GameObjectType::ITEM)
	{
		ItemRef item = _items.find(objectId)->second;
		item->room.reset();
		_items.erase(objectId);

		cout << "Item " << item->GetObjectId() << " : Despawn" << endl;
	}

	// 타인한테 정보 전송
	{
		Protocol::S_Despawn despawnPacket;
		despawnPacket.add_objectids(objectId);
		auto sendBuffer = ClientPacketHandler::MakeSendBuffer(despawnPacket);

		for (auto& it : _players)
		{
			PlayerRef p = it.second;
			if (p->GetObjectId() != objectId)
				p->GetOwnerSession()->Send(sendBuffer);
		}
	}
}

void GameRoom::HandleMove(PlayerRef player, Protocol::C_Move movePacket)
{
	if (_gameRoomId != player->room.lock()->GetGameRoomId())
		return;

	// TODO : 검증 더 꼼꼼히
	Protocol::PositionInfo movePosInfo = movePacket.posinfo();

	// 다른 좌표로 이동할 경우, 갈 수 있는지 체크
	if (movePosInfo.posx() != player->info.posinfo().posx() || movePosInfo.posy() != player->info.posinfo().posy())
	{
		Vector2Int dest(movePosInfo.posx(), movePosInfo.posy());
		if (gameMap.CanGo(dest) == false)
		{
			return;
		}
	}

	// 서버에서 위치 이동
	player->info.mutable_posinfo()->CopyFrom(movePosInfo);
	cout << "Player_" << player->GetObjectId() << " S_Move (" << movePosInfo.posx() << ", " << movePosInfo.posy() << ")" << endl;

	// 다른 플레이어한테도 알려줌
	Protocol::S_Move resMovePacket;
	resMovePacket.set_objectid(player->GetObjectId());
	resMovePacket.mutable_posinfo()->CopyFrom(movePosInfo);
	auto sendBuffer = ClientPacketHandler::MakeSendBuffer(resMovePacket);
	Broadcast(sendBuffer);
}

void GameRoom::HandleSkill(PlayerRef player, Protocol::C_Skill skillPacket)
{
	if (_gameRoomId != player->room.lock()->GetGameRoomId())
		return;

	// 스킬 사용 가능 여부 체크
	Vector2Int skillPos = player->GetCellPos();
	if (gameMap.CanGo(skillPos) == false)
		return;
	if (player->bubbleCount >= player->info.statinfo().availbubble())
		return;

	// 버블 생성
	BubbleRef bubble = make_shared<Bubble>(player, player->info.statinfo().power());
	bubble->SetObjectId(GObjectManager.GenerateId(Protocol::GameObjectType::BUBBLE));
	bubble->info.set_name("Bubble_" + ::to_string(bubble->GetObjectId()));
	{
		Protocol::PositionInfo* posInfo = bubble->info.mutable_posinfo();
		posInfo->set_state(Protocol::CreatureState::IDLE);
		posInfo->set_posx(skillPos.x);
		posInfo->set_posy(skillPos.y);
	}
	DoAsync(&GameRoom::EnterGame, static_pointer_cast<GameObject>(bubble));
}

void GameRoom::CheckAndEnd()
{
	int32 deathCount = 0;
	for (auto& it : _players)
	{
		if (it.second->info.posinfo().state() == Protocol::CreatureState::DEAD)
			deathCount++;
	}

	// 남은 사람이 2명 미만이면 게임을 끝낸다
	if (_players.size() - deathCount < 2)
	{
		// S_EndGame 패킷 Broadcast
		Protocol::S_EndGame endGamePacket;
		auto sendBuffer = ClientPacketHandler::MakeSendBuffer(endGamePacket);
		DoAsync(&GameRoom::Broadcast, sendBuffer);

		// 모두 퇴장
		for (auto& it : _players)
			DoAsync(&GameRoom::LeaveGame, it.second->GetObjectId());
	}
}

void GameRoom::Broadcast(SendBufferRef sendBuffer)
{
	for (auto it : _players)
	{
		PlayerRef p = it.second;
		p->GetOwnerSession()->Send(sendBuffer);
	}
}

vector<GameObjectRef> GameRoom::FindAllObjects(Vector2Int cellPos)
{
	vector<GameObjectRef> list;

	for (auto& it : _players)
	{
		PlayerRef player = it.second;
		if (cellPos.x == player->GetCellPos().x && cellPos.y == player->GetCellPos().y)
			list.push_back(player);
	}
	for (auto& it : _blocks)
	{
		BlockRef block = it.second;
		if (cellPos.x == block->GetCellPos().x && cellPos.y == block->GetCellPos().y)
			list.push_back(block);
	}
	for (auto& it : _bubbles)
	{
		BubbleRef bubble = it.second;
		if (cellPos.x == bubble->GetCellPos().x && cellPos.y == bubble->GetCellPos().y)
			list.push_back(bubble);
	}
	for (auto& it : _waves)
	{
		WaveRef wave = it.second;
		if (cellPos.x == wave->GetCellPos().x && cellPos.y == wave->GetCellPos().y)
			list.push_back(wave);
	}
	for (auto& it : _items)
	{
		ItemRef item = it.second;
		if (cellPos.x == item->GetCellPos().x && cellPos.y == item->GetCellPos().y)
			list.push_back(item);
	}

	return list;
}
