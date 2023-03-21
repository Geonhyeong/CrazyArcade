#pragma once
#include <cpp_redis/cpp_redis>

class RedisManager
{
public:
	RedisManager();
	~RedisManager();

	void Init();
	string GetValue(string key);

private:
	bool _connected = false;
	cpp_redis::client _redisClient;
};

extern RedisManager* GRedisManager;