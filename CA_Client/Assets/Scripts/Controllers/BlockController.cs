using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void OnPop()
    {
        // TODO : ��ǳ���� ������ ������ ��� �߰�
    }
}
