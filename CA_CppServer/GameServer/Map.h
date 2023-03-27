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

class Map
{
public:
	void LoadMap(int32 mapId, string pathPrefix = "../../Common/MapData");
	void LoadObjects(GameRoomRef room);

	bool CanGo(Vector2Int cellPos, bool checkObjects = true);
	bool ApplySpawn(GameObjectRef gameObject);
	bool ApplyDespawn(GameObjectRef gameObject);
	GameObjectRef Find(Vector2Int cellPos);

public:
	int32 minX;
	int32 maxX;
	int32 minY;
	int32 maxY;

private:
	vector<vector<bool>> _collision;
	vector<vector<GameObjectRef>> _objects;	// 충돌체만 관리
};

