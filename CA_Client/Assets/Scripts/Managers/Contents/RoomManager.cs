using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager
{
    public int RoomId { get; set; }
    public string RoomCode { get; set; }
    public int HostSessionId { get; set; }

    public Dictionary<int, GameSessionInfo> Players { get; } = new Dictionary<int, GameSessionInfo>();
    public GameSessionInfo MyGameSession { get; set; }

    public void Add(GameSessionInfo player)
    {
        Players.Add(player.SessionId, player);
    }

    public GameSessionInfo Get(int sessionId)
    {
        GameSessionInfo player = null;
        Players.TryGetValue(sessionId, out player);
        return player;
    }

    public GameSessionInfo Find(Func<GameSessionInfo, bool> condition)
    {
        foreach (GameSessionInfo player in Players.Values)
        {
            if (condition.Invoke(player))
                return player;
        }

        return null;
    }

    public void Clear()
    {
        Players.Clear();
    }

    public void RefreshUI()
    {
        UI_RoomScene roomSceneUI = Managers.UI.SceneUI as UI_RoomScene;
        if (roomSceneUI == null)
            return;

        roomSceneUI.RefreshUI();
    }
}
