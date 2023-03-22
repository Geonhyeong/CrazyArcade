using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomScene : BaseScene
{
    UI_RoomScene _sceneUI;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Room;

        _sceneUI = Managers.UI.ShowSceneUI<UI_RoomScene>();
    }

    public override void Clear()
    {
    }
}
