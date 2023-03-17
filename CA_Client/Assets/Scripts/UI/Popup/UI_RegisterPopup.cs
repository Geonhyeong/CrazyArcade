using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_RegisterPopup : UI_Popup
{
	public Selectable firstInput;

	enum GameObjects
	{
		Id,
		Nickname,
		Password,
		PasswordCheck
	}

	enum Images
	{
		SubmitBtn,
		CloseBtn
	}

	public override void Init()
    {
        base.Init();

		Bind<GameObject>(typeof(GameObjects));
		Bind<Image>(typeof(Images));

		GetImage((int)Images.SubmitBtn).gameObject.BindEvent(OnClickSubmitButton);
		GetImage((int)Images.CloseBtn).gameObject.BindEvent(OnClickCloseButton);

		firstInput.Select();
	}

	private void OnClickSubmitButton(PointerEventData evt)
    {
		string id = Get<GameObject>((int)GameObjects.Id).GetComponent<TMPro.TMP_InputField>().text;
		string nickname = Get<GameObject>((int)GameObjects.Nickname).GetComponent<TMPro.TMP_InputField>().text;
		string password = Get<GameObject>((int)GameObjects.Password).GetComponent<TMPro.TMP_InputField>().text;
		string passwordCheck = Get<GameObject>((int)GameObjects.PasswordCheck).GetComponent<TMPro.TMP_InputField>().text;

		if (password != passwordCheck)
        {
			Get<GameObject>((int)GameObjects.Password).GetComponent<TMPro.TMP_InputField>().text = "";
			Get<GameObject>((int)GameObjects.PasswordCheck).GetComponent<TMPro.TMP_InputField>().text = "";
			return;
		}

		RegisterPacketReq packet = new RegisterPacketReq()
		{
			AccountName = id,
			Nickname = nickname,
			Password = password
		};

		Managers.Web.SendPostRequest<RegisterPacketRes>("login/register", packet, (res) =>
		{
			Debug.Log("Register : " + res.RegisterOk);
			if (res.RegisterOk)
            {
				Managers.UI.ClosePopupUI();
            }
			else
            {
				Get<GameObject>((int)GameObjects.Nickname).GetComponent<TMPro.TMP_InputField>().text = "";
			}
		});
    }

	private void OnClickCloseButton(PointerEventData evt)
    {
		Managers.UI.ClosePopupUI();
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
