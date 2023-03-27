#pragma once
#include "Map.h"

class GameObject : public enable_shared_from_this<GameObject>
{
public:
	~GameObject() { cout << "~GameObject" << endl; }

	Vector2Int GetCellPos() { return Vector2Int(info.posinfo().posx(), info.posinfo().posy()); }
	void SetCellPos(int32 x, int32 y);
	Vector2Int GetFrontCellPos() { return GetFrontCellPos(info.posinfo().movedir()); }
	Vector2Int GetFrontCellPos(Protocol::MoveDir dir, int32 distance = 1);
	int32 GetObjectId() { return info.objectid(); }
	void SetObjectId(int32 objectId) { info.set_objectid(objectId); }
	
	virtual void OnAttacked(GameObjectRef attacker) {}

public:
	Protocol::GameObjectType objectType = Protocol::GameObjectType::NONE;
	Protocol::ObjectInfo info;
	weak_ptr<GameRoom> room;
};