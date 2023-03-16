#include "pch.h"
#include "SessionManager.h"
#include "GameSession.h"

SessionManager GSessionManager;

void SessionManager::Generate(GameSessionRef session)
{
	WRITE_LOCK;
	int32 sessionId = ++_sessionId;

	session->SetSessionId(sessionId);
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

GameSessionRef SessionManager::Find(int id)
{
	READ_LOCK;
	auto it = _sessions.find(id);
	if (it != _sessions.end())
		return it->second;

	return nullptr;
}