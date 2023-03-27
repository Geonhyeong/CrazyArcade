#include "pch.h"
#include "Map.h"
#include <fstream>
#include "ObjectManager.h"
#include "GameRoom.h"
#include "Block.h"

void Map::LoadMap(int32 mapId, string pathPrefix)
{
	int32 NUM_ZEROS = 3;
	string str_mapId = ::to_string(mapId);
	string mapName = "Map_" + string(NUM_ZEROS - str_mapId.length(), '0') + str_mapId;

	// 맵 관련 파일
	ifstream readFile;
	readFile.open(pathPrefix + "/" + mapName + ".txt");

	if (readFile.is_open())
	{
		string temp;
		::getline(readFile, temp);		minX = ::stoi(temp);
		::getline(readFile, temp);		maxX = ::stoi(temp);
		::getline(readFile, temp);		minY = ::stoi(temp);
		::getline(readFile, temp);		maxY = ::stoi(temp);

		int32 xCount = maxX - minX + 1;
		int32 yCount = maxY - minY + 1;
		_collision.assign(yCount, vector<bool>(xCount, false));
		_objects.assign(yCount, vector<GameObjectRef>(xCount, nullptr));

		// collision 정보 읽기
		for (int32 y = 0; y < yCount; y++)
		{
			string line;
			::getline(readFile, line);
			for (int32 x = 0; x < xCount; x++)
			{
				_collision[y][x] = (line[x] == '1' ? true : false);
			}
		}

		for (int32 y = 0; y < yCount; y++)
		{
			string line;
			::getline(readFile, line);
			for (int32 x = 0; x < xCount; x++)
			{
				if (line[x] == '0')
					continue;

				// 블록 생성
				BlockRef block = make_shared<Block>();
				block->SetObjectId(GObjectManager.GenerateId(Protocol::GameObjectType::BLOCK));
				string blockName = "Block_";
				blockName.push_back(line[x]);
				block->info.set_name(blockName);
				{
					Protocol::PositionInfo* posInfo = block->info.mutable_posinfo();
					posInfo->set_state(Protocol::CreatureState::IDLE);
					posInfo->set_posx(x + minX);
					posInfo->set_posy(maxY - y);
				}

				_objects[y][x] = static_pointer_cast<GameObject>(block);
			}
		}
	}
}

void Map::LoadObjects(GameRoomRef room)
{
	if (room == nullptr)
		return;

	int32 xCount = maxX - minX + 1;
	int32 yCount = maxY - minY + 1;

	for (int32 y = 0; y < yCount; y++)
	{
		for (int32 x = 0; x < xCount; x++)
		{
			GameObjectRef go = _objects[y][x];
			if (go != nullptr)
				room->DoAsync(&GameRoom::EnterGame, go);
		}
	}
}

bool Map::CanGo(Vector2Int cellPos, bool checkObjects)
{
	if (cellPos.x < minX || cellPos.x > maxX)
		return false;
	if (cellPos.y < minY || cellPos.y > maxY)
		return false;

	int32 x = cellPos.x - minX;
	int32 y = maxY - cellPos.y;
	return !_collision[y][x] && (!checkObjects || _objects[y][x] == nullptr);
}

bool Map::ApplySpawn(GameObjectRef gameObject)
{
	ApplyDespawn(gameObject);

	if (gameObject->room.lock() == nullptr)
		return false;

	Vector2Int pos = gameObject->GetCellPos();
	if (CanGo(pos, true) == false)
		return false;


	int x = pos.x - minX;
	int y = maxY - pos.y;
	_objects[y][x] = gameObject;

	return true;
}

bool Map::ApplyDespawn(GameObjectRef gameObject)
{
	if (gameObject->room.lock() == nullptr)
		return false;

	Vector2Int pos = gameObject->GetCellPos();
	if (pos.x < minX || pos.x > maxX)
		return false;
	if (pos.y < minY || pos.y > maxY)
		return false;

	int x = pos.x - minX;
	int y = maxY - pos.y;
	if (_objects[y][x] == gameObject)
		_objects[y][x] = nullptr;

	return true;
}

GameObjectRef Map::Find(Vector2Int cellPos)
{
	if (cellPos.x < minX || cellPos.x > maxX)
		return nullptr;
	if (cellPos.y < minY || cellPos.y > maxY)
		return nullptr;

	int x = cellPos.x - minX;
	int y = maxY - cellPos.y;
	return _objects[y][x];
}
