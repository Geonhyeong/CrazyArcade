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

private:
	int32 _sessionId;
};