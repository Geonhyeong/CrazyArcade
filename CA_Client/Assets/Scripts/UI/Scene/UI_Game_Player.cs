using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Game_Player : UI_Base
{
    [SerializeField]
    public TMPro.TMP_Text Nickname = null;

    public override void Init()
    {

    }

    public void SetNickname(string nickname)
    {
        Nickname.text = nickname;
    }
}
