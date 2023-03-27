#pragma once
#include "GameObject.h"

enum ItemType
{
	NONE = 0,
	BALLOON = 1,
	POTION = 2,
	POTION_MAKE_POWER_MAX = 3,
	SKATE = 4
};

class Item : public GameObject
{
public:
	Item() { objectType = Protocol::GameObjectType::ITEM; }

	void OnSpawn();
	virtual void OnAttacked(GameObjectRef attacker) override;

private:
	void CheckCollision(ItemRef mySelf);
	void Despawn();

public:
	int32 itemType = ItemType::NONE;

private:
	bool _isExist = true;
};

