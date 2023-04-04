using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Test;

        Managers.Map.LoadMap(0);
    }

    public override void Clear()
    {
    }
}
