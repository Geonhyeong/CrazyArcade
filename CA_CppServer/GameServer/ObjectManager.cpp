#include "pch.h"
#include "ObjectManager.h"

ObjectManager GObjectManager;

Protocol::GameObjectType ObjectManager::GetObjectTypeById(int id)
{
	int type = (id >> 24) & 0x7F;
	return (Protocol::GameObjectType)type;
}

// [UNUSED(1)][TYPE(7)][ID(24)]
int32 ObjectManager::GenerateId(Protocol::GameObjectType type)
{
	WRITE_LOCK;
	return ((int)type << 24) | (_counter++);
}