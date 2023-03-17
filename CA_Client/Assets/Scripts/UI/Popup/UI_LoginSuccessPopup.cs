using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LoginSuccessPopup : UI_Popup
{
    enum Images
    {
        OkBtn
    }

    public override void Init()
    {
        base.Init();

        Bind<Image>(typeof(Images));

        GetImage((int)Images.OkBtn).gameObject.BindEvent(OnClickOkButton);
    }

    private void OnClickOkButton(PointerEventData evt)
    {
        // �α��� �����ϸ� �κ�� ����
        Managers.Network.ConnectToGame();
        Managers.Scene.LoadScene(Define.Scene.Lobby);
        Managers.UI.ClosePopupUI();
    }
}
