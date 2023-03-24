#pragma once

struct Vector2Int
{
	int32 x;
	int32 y;

	Vector2Int(int32 _x, int32 _y)
	{
		x = _x;
		y = _y;
	}

	static Vector2Int up() { return Vector2Int(0, 1); }
	static Vector2Int down() { return Vector2Int(0, -1); }
	static Vector2Int left() { return Vector2Int(-1, 0); }
	static Vector2Int right() { return Vector2Int(1, 0); }

	Vector2Int operator+(const Vector2Int b) const
	{
		return Vector2Int(x + b.x, y + b.y);
	}

	Vector2Int operator*(const int32 b) const
	{
		return Vector2Int(x * b, y * b);
	}
};

class GameObject
{
public:
	~GameObject() { cout << "~GameObject" << endl; }

	Vector2Int GetCellPos() { return Vector2Int(info->posinfo().posx(), info->posinfo().posy()); }
	void SetCellPos(int32 x, int32 y);
	Vector2Int GetFrontCellPos() { return GetFrontCellPos(info->posinfo().movedir()); }
	Vector2Int GetFrontCellPos(Protocol::MoveDir dir, int32 distance = 1);
	int32 GetObjectId() { return info->objectid(); }
	void SetObjectId(int32 objectId) { info->set_objectid(objectId); }

public:
	Protocol::GameObjectType objectType = Protocol::GameObjectType::NONE;
	Protocol::ObjectInfo* info = new Protocol::ObjectInfo();
	weak_ptr<GameRoom> room;
};