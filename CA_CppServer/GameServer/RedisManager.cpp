#include "pch.h"
#include "RedisManager.h"

RedisManager* GRedisManager = nullptr;

RedisManager::RedisManager()
{
	Init();
}

RedisManager::~RedisManager()
{
	if (_connected == false)
		return;

	_connected = false;
	_redisClient.disconnect();
}

void RedisManager::Init()
{
	if (_connected == true)
		return;

	_redisClient.connect();	// "default : 127.0.0.1:6379"
	_connected = true;
}

string RedisManager::GetValue(string key)
{
	future<cpp_redis::reply> get_reply = _redisClient.get(key);
	_redisClient.sync_commit();

	cpp_redis::reply reply = get_reply.get();
	
	return reply.as_string();
}
