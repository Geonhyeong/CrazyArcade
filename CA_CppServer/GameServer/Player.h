#pragma once
#include "GameObject.h"

enum
{
	MAX_SPEED_LEVEL = 7,
	MAX_POWER = 5,
	MAX_BUBBLE_COUNT = 7
};

class Player : public GameObject
{
public:
	Player() { objectType = Protocol::GameObjectType::PLAYER; }

	void GetItem(ItemRef item);
	virtual void OnAttacked(GameObjectRef attacker) override;

	GameSessionRef GetOwnerSession() { return _ownerSession; }
	void SetOwnerSession(GameSessionRef ownerSession) { _ownerSession = ownerSession; }

private:
	void CheckCollision();
	void OnDead(PlayerRef mySelf);

public:
	int32 bubbleCount = 0;

private:
	GameSessionRef _ownerSession;
};