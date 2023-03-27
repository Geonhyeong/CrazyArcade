#pragma once
#include "GameObject.h"

class Wave : public GameObject
{
public:
	Wave() { objectType = Protocol::GameObjectType::WAVE; }

	void OnSpawn();

private:
	void CheckCollision(WaveRef mySelf);
	void Despawn();
};

