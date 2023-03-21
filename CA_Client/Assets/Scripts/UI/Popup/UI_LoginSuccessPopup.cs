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
        // 로그인 성공하면 게임서버에 접속 시도
        Managers.Network.ConnectToGame();
        Managers.UI.ClosePopupUI();
    }
}
