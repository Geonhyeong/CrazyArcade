using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
    UI_LobbyScene _sceneUI;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Lobby;

        _sceneUI = Managers.UI.ShowSceneUI<UI_LobbyScene>();
    }

    public override void Clear()
    {
        
    }
}
