using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Room;

        GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Room");
    }

    public override void Clear()
    {
    }
}
