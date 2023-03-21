using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LoginScene : UI_Scene
{
	public Selectable firstInput;

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
		GetImage((int)Images.RegisterBtn).gameObject.BindEvent(OnClickRegisterButton);

		firstInput.Select();
	}

    private void OnClickRegisterButton(PointerEventData evt)
    {
        // ÆË¾÷ Ãâ·Â
        Managers.UI.ShowPopupUI<UI_RegisterPopup>();
    }

    private void OnClickLoginButton(PointerEventData evt)
	{
		string id = Get<GameObject>((int)GameObjects.Id).GetComponent<TMPro.TMP_InputField>().text;
		string password = Get<GameObject>((int)GameObjects.Password).GetComponent<TMPro.TMP_InputField>().text;

        LoginPacketReq packet = new LoginPacketReq()
        {
            AccountName = id,
            Password = password
        };

        Managers.Web.SendPostRequest<LoginPacketRes>("login/login", packet, (res) =>
        {
            Get<GameObject>((int)GameObjects.Id).GetComponent<TMPro.TMP_InputField>().text = "";
            Get<GameObject>((int)GameObjects.Password).GetComponent<TMPro.TMP_InputField>().text = "";

            if (res.LoginOk)
            {
                Managers.Network.AccountId = res.AccountId;
                Managers.Network.Token = res.Token;

                // °á°ú ÆË¾÷ Ãâ·Â
                Managers.UI.ShowPopupUI<UI_LoginSuccessPopup>();
            }
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.RightShift))
        {
            Selectable prev = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp();
            if (prev != null)
                prev.Select();
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            if (next != null)
                next.Select();
        }
    }
}
