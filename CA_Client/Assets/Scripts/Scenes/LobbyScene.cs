using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Lobby;

        GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Lobby");
    }

    public override void Clear()
    {
    }
}
