#pragma once
#include "GameObject.h"

class Block : public GameObject
{
public:
	Block() { objectType = Protocol::GameObjectType::BLOCK; }

	virtual void OnAttacked(GameObjectRef attacker) override;

private:
	void Despawn();
	void DropItem();
};

