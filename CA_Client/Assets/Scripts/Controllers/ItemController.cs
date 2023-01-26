using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : CreatureController
{
    protected override void Init()
    {
        base.Init();
        Type = GameObjectType.Item;
        transform.position += new Vector3(0, 0.075f);
    }

    protected override void UpdateAnimation()
    {
        if (_animator == null)
            return;

        switch (State)
        {
            case CreatureState.Idle:
                _animator.Play("IDLE");
                break;
        }
    }
}
