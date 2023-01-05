using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class BlockController : InanimateController
{
    protected override void Init()
    {
        base.Init();
        transform.position += new Vector3(0, 0.075f);
    }

    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();
    }

    public override void OnPop()
    {
        State = InanimateState.Pop;

        GameObject.Destroy(gameObject, 0.4f);
        Managers.Object.Remove(gameObject);
    }
}
