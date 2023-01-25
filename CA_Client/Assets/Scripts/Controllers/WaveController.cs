using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class WaveController : CreatureController
{
    public bool IsEdge { get; set; } = false;

    protected override void Init()
    {
        base.Init();
        Type = GameObjectType.Wave;
    }

    protected override void UpdateAnimation()
    {
        if (_animator == null)
            return;

        switch (Dir)
        {
            case MoveDir.Up:
                _animator.Play(IsEdge ? "UP_EDGE" : "UP");
                break;

            case MoveDir.Down:
                _animator.Play(IsEdge ? "DOWN_EDGE" : "DOWN");
                break;

            case MoveDir.Left:
                _animator.Play(IsEdge ? "LEFT_EDGE" : "LEFT");
                break;

            case MoveDir.Right:
                _animator.Play(IsEdge ? "RIGHT_EDGE" : "RIGHT");
                break;
        }
    }
}
