#pragma once
#include "GameObject.h"

class Bubble : public GameObject
{
public:
	Bubble(PlayerRef owner, int32 power);

	void OnSpawn();
	virtual void OnAttacked(GameObjectRef attacker) override;

private:
	void Pop(BubbleRef mySelf);
	void Despawn();

private:
	PlayerRef _owner;
	int32 _power;
};

