#pragma once

class SessionManager
{
public:
	void			Generate(GameSessionRef session);
	void			Remove(GameSessionRef session);
	GameSessionRef	Find(int32 id);
	GameSessionRef	FindByAccountDbId(int32 accountDbId);

private:
	USE_LOCK;
	int32 _sessionId = 0;
	map<int32, GameSessionRef> _sessions;
};

extern SessionManager GSessionManager;