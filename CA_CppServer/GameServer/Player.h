#pragma once
#include "GameObject.h"

class Player : public GameObject
{
public:
	Player() { objectType = Protocol::GameObjectType::PLAYER; }

	//void GetItem(Item item);
	//virtual void OnAttacked(GameObject attacker) override;

private:
	void CheckCollision();
	void OnDead();

public:
	GameSessionRef GetOwnerSession() { return _ownerSession; }
	void SetOwnerSession(GameSessionRef ownerSession) { _ownerSession = ownerSession; }

private:
	GameSessionRef _ownerSession;
	//int32 _bubbleCount = 0;
};

