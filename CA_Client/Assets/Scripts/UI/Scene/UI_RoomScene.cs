using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_RoomScene : UI_Scene
{
    [SerializeField]
    public TMPro.TMP_Text RoomCode = null;

    public List<UI_Room_Player> PlayerUIs { get; } = new List<UI_Room_Player>();
    
    public override void Init()
    {
        base.Init();

        RefreshUI();
    }

    public void RefreshUI()
    {
        Clear();

        RoomCode.text = Managers.Room.RoomCode;

        List<GameSessionInfo> players = Managers.Room.Players.Values.ToList();
        players.Sort((left, right) =>
        {
            if (Managers.Room.HostSessionId == right.SessionId)
                return 1;
            return left.SessionId - right.SessionId;
        });

        GameObject grid = transform.Find("PlayerGrid").gameObject;
        foreach (GameSessionInfo player in players)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Room_Player", grid.transform);
            UI_Room_Player playerUI = go.GetOrAddComponent<UI_Room_Player>();
            playerUI.SetNickname(player.Nickname);
            PlayerUIs.Add(playerUI);
        }
    }

    private void Clear()
    {
        PlayerUIs.Clear();

        GameObject grid = transform.Find("PlayerGrid").gameObject;
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);
    }
}
