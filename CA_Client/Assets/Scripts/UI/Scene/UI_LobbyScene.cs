using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LobbyScene : UI_Scene
{
	private string nickname = "";

	enum GameObjects
	{
		Nickname,
		RoomCode
	}

	enum Images
	{
		HostBtn,
		PublicBtn,
		PrivateBtn
	}

	public override void Init()
    {
        base.Init();

		Bind<GameObject>(typeof(GameObjects));
		Bind<Image>(typeof(Images));

		GetImage((int)Images.HostBtn).gameObject.BindEvent(OnClickHostButton);
		GetImage((int)Images.PublicBtn).gameObject.BindEvent(OnClickPublicButton);
		GetImage((int)Images.PrivateBtn).gameObject.BindEvent(OnClickPrivateButton);
	}

	private void OnClickHostButton(PointerEventData evt)
    {
		if (HasNickname() == false)
			return;

		C_CreateRoom createRoomPacket = new C_CreateRoom();
		createRoomPacket.Nickname = nickname;
		Managers.Network.Send(createRoomPacket);
    }

	private void OnClickPublicButton(PointerEventData evt)
	{
		if (HasNickname() == false)
			return;

		C_EnterRoom enterRoomPacket = new C_EnterRoom();
		enterRoomPacket.Nickname = nickname;
		enterRoomPacket.RoomCode = "RANDOM";
		Managers.Network.Send(enterRoomPacket);
	}

	private void OnClickPrivateButton(PointerEventData evt)
	{
		if (HasNickname() == false)
			return;

		string roomCode = Get<GameObject>((int)GameObjects.RoomCode).GetComponent<TMPro.TMP_InputField>().text;
		if (roomCode.Length == 0)
			return;

		C_EnterRoom enterRoomPacket = new C_EnterRoom();
		enterRoomPacket.Nickname = nickname;
		enterRoomPacket.RoomCode = roomCode;
		Managers.Network.Send(enterRoomPacket);
	}

	private bool HasNickname()
    {
		nickname = Get<GameObject>((int)GameObjects.Nickname).GetComponent<TMPro.TMP_InputField>().text;
		
		if (nickname.Length == 0 || nickname.Contains(' '))
        {
			// TODO : 경고 팝업 출력
			return false;
        }

		Managers.Network.Nickname = nickname;
		return true;
    }
}
