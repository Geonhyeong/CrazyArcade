#include "pch.h"
#include "SessionManager.h"
#include "GameSession.h"

SessionManager GSessionManager;

void SessionManager::Generate(GameSessionRef session)
{
	WRITE_LOCK;
	int32 sessionId = ++_sessionId;

	session->SetSessionId(sessionId);
	session->SetAccountDbId(-1);
	_sessions.insert(make_pair(sessionId, session));

	cout << "Session is Generated : " << sessionId << endl;
}

void SessionManager::Remove(GameSessionRef session)
{
	WRITE_LOCK;
	if (Find(session->GetSessionId()) == nullptr)
		return;

	_sessions.erase(session->GetSessionId());
	
	cout << "Session is Removed : " << session->GetSessionId() << endl;
}

GameSessionRef SessionManager::Find(int32 id)
{
	READ_LOCK;
	auto it = _sessions.find(id);
	if (it != _sessions.end())
		return it->second;

	return nullptr;
}

GameSessionRef SessionManager::FindByAccountDbId(int32 accountDbId)
{
	READ_LOCK;
	for (auto it : _sessions)
	{
		if (it.second->GetAccountDbId() == accountDbId)
			return it.second;
	}

	return nullptr;
}
