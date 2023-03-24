#pragma once

#define WIN32_LEAN_AND_MEAN             // 거의 사용되지 않는 내용을 Windows 헤더에서 제외합니다.

#ifdef _DEBUG
#pragma comment(lib, "ServerCore\\Debug\\ServerCore.lib")
#pragma comment(lib, "Protobuf\\Debug\\libprotobufd.lib")
#pragma comment(lib, "cpp_redis\\Debug\\cpp_redis.lib")
#pragma comment(lib, "cpp_redis\\Debug\\tacopie.lib")
#else
#pragma comment(lib, "ServerCore\\Release\\ServerCore.lib")
#pragma comment(lib, "Protobuf\\Release\\libprotobuf.lib")
#pragma comment(lib, "cpp_redis\\Release\\cpp_redis.lib")
#pragma comment(lib, "cpp_redis\\Release\\tacopie.lib")
#endif // _DEBUG

#include "CorePch.h"
#include "Protocol.pb.h"

using GameSessionRef	= std::shared_ptr<class GameSession>;
using RoomRef			= std::shared_ptr<class Room>;
using GameRoomRef		= std::shared_ptr<class GameRoom>;
using GameObjectRef		= std::shared_ptr<class GameObject>;
using PlayerRef			= std::shared_ptr<class Player>;