using Google.Protobuf.Protocol;
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            C_LeaveRoom leaveRoomPacket = new C_LeaveRoom();
            Managers.Network.Send(leaveRoomPacket);
        }
    }
}
