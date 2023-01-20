using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : CreatureController
{
    protected override void Init()
    {
        base.Init();
        Type = GameObjectType.Block;
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

            case CreatureState.Pop:
                _animator.Play("POP");
                break;
        }
    }

    protected override void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                UpdateIdle();
                break;
            case CreatureState.Pop:
                UpdatePop();
                break;
        }
    }

    private void UpdateIdle()
    {

    }

    private void UpdatePop()
    {

    }
}