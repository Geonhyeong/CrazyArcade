using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_GameScene : UI_Scene
{
    [SerializeField]
    public List<UI_Game_Player> PlayerUIs { get; } = new List<UI_Game_Player>();

    private Coroutine _coroutine;
    private GameObject gameStartUI;
    private GameObject gameOverUI;

    enum Images
    {
        LeaveRoomBtn
    }

    public override void Init()
    {
        base.Init();

        gameStartUI = GameObject.Find("UI_GameStartPopup");
        gameOverUI = GameObject.Find("UI_GameOverPopup");
        gameOverUI.SetActive(false);

        Bind<Image>(typeof(Images));
        GetImage((int)Images.LeaveRoomBtn).gameObject.BindEvent(OnClickLeaveRoomButton);

        RefreshUI();

        _coroutine = StartCoroutine("CoInactivePopup", 3.0f);
    }

    public void RefreshUI()
    {
        Clear();

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
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Game_Player", grid.transform);
            UI_Game_Player playerUI = go.GetOrAddComponent<UI_Game_Player>();
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

    public void GameOver()
    {
        gameOverUI.SetActive(true);

        _coroutine = StartCoroutine("CoGameOver", 3.0f);
    }

    private void OnClickLeaveRoomButton(PointerEventData evt)
    {
        C_LeaveRoom leaveRoomPacket = new C_LeaveRoom();
        Managers.Network.Send(leaveRoomPacket);
    }

    IEnumerator CoInactivePopup(float time)
    {
        yield return new WaitForSeconds(time);

        gameStartUI.SetActive(false);

        _coroutine = null;
    }

    IEnumerator CoGameOver(float time)
    {
        yield return new WaitForSeconds(time);

        Managers.Map.DestroyMap();
        Managers.Object.Clear();

        Managers.Scene.LoadScene(Define.Scene.Room);

        _coroutine = null;
    }
}
