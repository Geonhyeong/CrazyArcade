#pragma once

class ObjectManager
{
public:
	static Protocol::GameObjectType GetObjectTypeById(int id);
	int32 GenerateId(Protocol::GameObjectType type);

private:
	USE_LOCK;
	int32 _counter = 1;
};

extern ObjectManager GObjectManager;
