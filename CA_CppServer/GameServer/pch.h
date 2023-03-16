#pragma once

#define WIN32_LEAN_AND_MEAN             // ���� ������ �ʴ� ������ Windows ������� �����մϴ�.

#ifdef _DEBUG
#pragma comment(lib, "ServerCore\\Debug\\ServerCore.lib")
#pragma comment(lib, "Protobuf\\Debug\\libprotobufd.lib")
#else
#pragma comment(lib, "ServerCore\\Release\\ServerCore.lib")
#pragma comment(lib, "Protobuf\\Debug\\libprotobuf.lib")
#endif // _DEBUG

#include "CorePch.h"
#include "Protocol.pb.h"

using GameSessionRef = std::shared_ptr<class GameSession>;
using RoomRef = std::shared_ptr<class Room>;