#pragma once
#include "Session.h"

class GameSession : public PacketSession
{
public:
	~GameSession()
	{
		cout << "~GameSession" << endl;
	}

	virtual void OnConnected() override;
	virtual void OnDisconnected() override;
	virtual void OnRecvPacket(BYTE* buffer, int32 len) override;
	virtual void OnSend(int32 len) override;

public:
	int32 GetSessionId() { return _sessionId; }
	void SetSessionId(int32 sessionId) { _sessionId = sessionId; }
	string GetNickname() { return _nickname; }
	void SetNickname(string nickname) { _nickname = nickname; }
	int32 GetAccountDbId() { return _accountDbId; }
	void SetAccountDbId(int32 accountDbId) { _accountDbId = accountDbId; }

public:
	weak_ptr<class Room> room;

private:
	int32 _sessionId;
	int32 _accountDbId;
	string _nickname;
};