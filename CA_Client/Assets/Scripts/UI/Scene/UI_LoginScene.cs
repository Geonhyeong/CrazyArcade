using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LoginScene : UI_Scene
{
	enum GameObjects
	{
		Id,
		Password
	}

	enum Images
	{
		LoginBtn,
		RegisterBtn
	}

	public override void Init()
	{
		base.Init();

		Bind<GameObject>(typeof(GameObjects));
		Bind<Image>(typeof(Images));

		GetImage((int)Images.LoginBtn).gameObject.BindEvent(OnClickLoginButton);
		GetImage((int)Images.RegisterBtn).gameObject.BindEvent(OnClickCreateButton);
	}

    public void OnClickCreateButton(PointerEventData evt)
    {
        string id = Get<GameObject>((int)GameObjects.Id).GetComponent<TMPro.TMP_InputField>().text;
        string password = Get<GameObject>((int)GameObjects.Password).GetComponent<TMPro.TMP_InputField>().text;

        RegisterPacketReq packet = new RegisterPacketReq()
        {
            AccountName = id,
            Password = password
        };

        Managers.Web.SendPostRequest<RegisterPacketRes>("login/register", packet, (res) =>
        {
            Debug.Log(res.RegisterOk);
            Get<GameObject>((int)GameObjects.Id).GetComponent<TMPro.TMP_InputField>().text = "";
            Get<GameObject>((int)GameObjects.Password).GetComponent<TMPro.TMP_InputField>().text = "";
        });
    }

    public void OnClickLoginButton(PointerEventData evt)
	{
		Debug.Log("OnClickLoginButton");

		string id = Get<GameObject>((int)GameObjects.Id).GetComponent<TMPro.TMP_InputField>().text;
		string password = Get<GameObject>((int)GameObjects.Password).GetComponent<TMPro.TMP_InputField>().text;

		// TODO : 로그인 서버 연동
        LoginPacketReq packet = new LoginPacketReq()
        {
            AccountName = id,
            Password = password
        };

        Managers.Web.SendPostRequest<LoginPacketRes>("login/login", packet, (res) =>
        {
            Debug.Log(res.LoginOk);
            Get<GameObject>((int)GameObjects.Id).GetComponent<TMPro.TMP_InputField>().text = "";
            Get<GameObject>((int)GameObjects.Password).GetComponent<TMPro.TMP_InputField>().text = "";

            if (res.LoginOk)
            {
                /*Managers.Network.AccountId = res.AccountId;
                Managers.Network.Token = res.Token;

                UI_SelectServerPopup popup = Managers.UI.ShowPopupUI<UI_SelectServerPopup>();
                popup.SetServers(res.ServerList);*/
            }
        });

		// TODO : 로그인 결과 팝업 출력

		// TODO : 로그인 서버에서 OK가 되면 네트워크 매니저에 정보 저장 후 게임서버에 접속
		/*Managers.Network.Id = id;
		Managers.Network.Token = token;*/
		/*Managers.Network.ConnectToGame();
		Managers.Scene.LoadScene(Define.Scene.Lobby);*/

    }
}
