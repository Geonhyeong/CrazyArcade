using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Login;

        GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Login");
    }

    public override void Clear()
    {

    }
}
